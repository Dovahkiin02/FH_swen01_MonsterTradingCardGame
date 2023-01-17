using System.Collections.Concurrent;
using System.Text;

namespace MonsterTradingCardGame {
    enum Status {
        WAITING,
        STARTED,
        FINISHED,
        FAILED
    }

    enum FightResult {
        PLAYER1,
        PLAYER2,
        DRAW
    }

    internal class GameHandler {
        private Database db;
        internal ConcurrentDictionary<Guid, Status> ledger = new();
        private ConcurrentDictionary<Guid, Tuple<FightResult, string>> protocols = new();

        private Dictionary<Element, ValueTuple<float, float, float, float>> elementInteractions = new();
        private Dictionary<Type, Dictionary<Type, float>> typeInteractions = new();

        private const int maxFightRounds = 100;

        public GameHandler(Database db) {
            this.db = db;
            setElementInteractions();
            setTypeInteractions();
        }

        public Status? viewStatus(Guid userId) {
            if (ledger.TryGetValue(userId, out Status status)) {
                return status;
            }
            return null;
        }
        public Status? getStatus(Guid userId) {
            if (ledger.TryGetValue(userId, out Status status)) {
                if (status == Status.FINISHED || status == Status.FAILED) { // if game finished remove the entry
                    ledger.TryRemove(userId, out Status value);
                }
                return status;
            }
            return null;
        }

        public Tuple<FightResult, string>? getProtocol(Guid userId) {
            if (protocols.TryRemove(userId, out Tuple<FightResult, string>? protocol)) {
                return protocol;
            }
            return null;
        }

        public Status joinGame(Guid user1) { 
            if (ledger.TryGetValue(user1, out Status returnStatus)) {
                return returnStatus;
            }
            IEnumerable<Guid> waitingUsers = ledger.Where(e => e.Value == Status.WAITING).Select(e => e.Key);
            if (ledger.Count == 0 || waitingUsers.Count() == 0) {
                ledger.TryAdd(user1, Status.WAITING);
                return Status.WAITING;
            }

            foreach (var user2 in waitingUsers) {
                if (ledger.TryGetValue(user2, out Status status)) {
                    if (status == Status.WAITING) {
                        ThreadPool.QueueUserWorkItem(state => Game(user1, user2));
                        return Status.STARTED;
                    }
                }
            }
            Console.WriteLine("something went wrong while trying to retrieve a user to start a game");
            ledger.TryAdd(user1, Status.WAITING);
            return Status.WAITING;
        }

        internal void Game(Guid user1, Guid user2) {
            if (user1 == user2) {
                gameFailed(user1, user2);
                return;
            }

            List<Card>? deck1 = db.getDeck(user1)?.Select(t => t.Item2).ToList();
            List<Card>? deck2 = db.getDeck(user2)?.Select(t => t.Item2).ToList();

            string? username1 = db.getUsername(user1);
            string? username2 = db.getUsername(user2);
            if (username1 == null || username2 == null || deck1 == null || deck2 == null) {
                gameFailed(user1, user2);
                return;
            }

            gameStarted(user1, user2);

            StringBuilder protocol = new();
            Random rand = new();
            int cardIndex1, cardIndex2;
            

            for (int i = 0; deck1.Count > 0 && deck2.Count > 0 && i < maxFightRounds; i++) {
                cardIndex1 = rand.Next(deck1.Count);
                cardIndex2 = rand.Next(deck2.Count);
                FightRecord record = fight(deck1[cardIndex1], deck2[cardIndex2]);
                if (record.fightResult == FightResult.PLAYER1) {
                    deck1.Add(deck2[cardIndex2]);
                    deck2.RemoveAt(cardIndex2);
                } else if (record.fightResult == FightResult.PLAYER2) {
                    deck2.Add(deck1[cardIndex1]);
                    deck1.RemoveAt(cardIndex1);
                }

                protocol.AppendLine((record with { player1 = username1, player2 = username2 }).ToString());
            }

            FightResult fightResult;
            if (deck1.Count == 0) { // Player1 lost
                protocol.AppendLine($"{username2} wins");
                fightResult = FightResult.PLAYER2;
            } else if (deck2.Count == 0) { // Player2 lost
                protocol.AppendLine($"{username1} wins");
                fightResult = FightResult.PLAYER1;
            } else { // Tie
                protocol.AppendLine($"draw");
                fightResult = FightResult.DRAW;
            }

            db.updateStats(user1, user2, fightResult);

            gameFinished(user1, user2);
            protocols.AddOrUpdate(user1, Tuple.Create(fightResult, protocol.ToString()), (key, value) => Tuple.Create(fightResult, protocol.ToString()));
            protocols.AddOrUpdate(user2, Tuple.Create(fightResult, protocol.ToString()), (key, value) => Tuple.Create(fightResult, protocol.ToString()));
        }

        internal FightRecord fight(Card card1, Card card2) {
            FightResult result;
            float damageCard1 = getDamageOfCard(card1, card2);
            float damageCard2 = getDamageOfCard(card2, card1);

            if (damageCard1 > damageCard2) {
                result = FightResult.PLAYER1;
            } else if (damageCard1 < damageCard2) {
                result = FightResult.PLAYER2;
            } else {
                result = FightResult.DRAW;
            }

            return new FightRecord(card1.name, damageCard1, card2.name, damageCard2, result);
        }

        internal float getDamageOfCard(Card card, Card enemyCard) {
            if (card.type != Type.SPELL && enemyCard.type != Type.SPELL) {
                return card.damage * getTypeMultiplicator(card.type, enemyCard.type);
            } else {
                return card.damage 
                    * getElementMultiplicator(card.element, enemyCard.element) 
                    * getTypeMultiplicator(card.type, enemyCard.type);
            }
        }

        private float getTypeMultiplicator(Type type, Type enemeyType) {
            if (typeInteractions.TryGetValue(type, out Dictionary<Type, float>? dict)) {
                if (dict.TryGetValue(enemeyType, out float res)) {
                    return res;
                }
            }
            return 1;
        }

        private float getElementMultiplicator(Element ele, Element enemyEle) {
            switch (enemyEle) {
                case Element.NORMAL:
                    return elementInteractions[ele].Item1;
                case Element.FIRE:
                    return elementInteractions[ele].Item2;
                case Element.WATER:
                    return elementInteractions[ele].Item3;
                default: // Grass
                    return elementInteractions[ele].Item4;
            }
        }

        private void setElementInteractions() {
            elementInteractions.Add(Element.NORMAL, (1f, 1f, 1f, 1f));
            elementInteractions.Add(Element.FIRE  , (1f, 0.5f, 0.5f, 2f));
            elementInteractions.Add(Element.WATER , (1f, 2f, 0.5f, 0.5f));
            elementInteractions.Add(Element.GRASS , (1f, 0.5f, 2f, 0.5f));
        }

        private void setTypeInteractions() {
            typeInteractions.Add(Type.GOBLIN, new Dictionary<Type, float> { { Type.DRAGON, 0f } });
            typeInteractions.Add(Type.ORK, new Dictionary<Type, float> { { Type.WIZZARD, 0f } });
            typeInteractions.Add(Type.SPELL, new Dictionary<Type, float> { { Type.KRAKEN, 0f } });
            typeInteractions.Add(Type.DRAGON, new Dictionary<Type, float> { { Type.ELF, 0f } });
        }

        private void gameStarted(Guid user1, Guid user2) {
            ledger.AddOrUpdate(user1, Status.STARTED, (key, value) => Status.STARTED);
            ledger.AddOrUpdate(user2, Status.STARTED, (key, value) => Status.STARTED);
        }

        private void gameFinished(Guid user1, Guid user2) {
            ledger.AddOrUpdate(user1, Status.FINISHED, (key, value) => Status.FINISHED);
            ledger.AddOrUpdate(user2, Status.FINISHED, (key, value) => Status.FINISHED);
        }

        private void gameFailed(Guid user1, Guid user2) {
            ledger.AddOrUpdate(user1, Status.FAILED, (key, value) => Status.FAILED);
            ledger.AddOrUpdate(user2, Status.FAILED, (key, value) => Status.FAILED);

        }

        internal record FightRecord(string cardName1, float cardDamage1, string cardName2, float cardDamage2, FightResult fightResult, string player1 = "Player1", string player2 = "Player2") {
            public override string ToString() {
                string resultPhrase;
                if (fightResult == FightResult.PLAYER1) {
                    resultPhrase = cardName1 + " wins";
                } else if (fightResult == FightResult.PLAYER2) {
                    resultPhrase = cardName2 + " wins";
                } else {
                    resultPhrase = "draw";
                }
                return $"{player1}: {cardName1} ({cardDamage1} Damage) vs {player2}: {cardName2} ({cardDamage2} Damage) => {resultPhrase}";
            }
        }
    }
}
