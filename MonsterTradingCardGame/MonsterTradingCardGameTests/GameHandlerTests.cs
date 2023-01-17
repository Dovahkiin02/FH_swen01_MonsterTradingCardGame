using Moq;
using Type = MonsterTradingCardGame.Type;

namespace MonsterTradingCardGameTests {

    [TestClass]
    public class GameHandlerTests01 {
        private static Mock<Database> mockedDb = new();
        private static GameHandler gh = new(mockedDb.Object);

        [TestMethod]
        public void getDamageTest_01() {
            Card card1 = new(1, "WaterGoblin", Element.WATER, 12, MonsterTradingCardGame.Type.GOBLIN);
            Card card2 = new(2, "FireSpell", Element.FIRE, 4, MonsterTradingCardGame.Type.SPELL);

            float damageCard1 = gh.getDamageOfCard(card1, card2);
            Assert.That(damageCard1, Is.EqualTo(24));

            float damageCard2 = gh.getDamageOfCard(card2, card1);
            Assert.That(damageCard2, Is.EqualTo(2));
        }

        [TestMethod]
        public void getDamageTest_02() {
            Card card1 = new(1, "WaterGoblin", Element.WATER, 12, MonsterTradingCardGame.Type.GOBLIN);
            Card card2 = new(2, "FireDragon", Element.FIRE, 4, MonsterTradingCardGame.Type.DRAGON);

            float damageCard1 = gh.getDamageOfCard(card1, card2);
            Assert.That(damageCard1, Is.EqualTo(0));

            float damageCard2 = gh.getDamageOfCard(card2, card1);
            Assert.That(damageCard2, Is.EqualTo(4));
        }

        [TestMethod]
        public void getDamageTest_03() {
            Card card1 = new(1, "GrassSpell", Element.GRASS, 2, MonsterTradingCardGame.Type.SPELL);
            Card card2 = new(2, "NormalSpell", Element.NORMAL, 5, MonsterTradingCardGame.Type.SPELL);

            float damageCard1 = gh.getDamageOfCard(card1, card2);
            Assert.That(damageCard1, Is.EqualTo(2));

            float damageCard2 = gh.getDamageOfCard(card2, card1);
            Assert.That(damageCard2, Is.EqualTo(5));
        }

        [TestMethod]
        public void getDamageTest_04() {
            Card card1 = new(1, "FireKraken", Element.FIRE, 20, MonsterTradingCardGame.Type.KRAKEN);
            Card card2 = new(2, "NormalSpell", Element.NORMAL, 5, MonsterTradingCardGame.Type.SPELL);

            float damageCard1 = gh.getDamageOfCard(card1, card2);
            Assert.That(damageCard1, Is.EqualTo(20));

            float damageCard2 = gh.getDamageOfCard(card2, card1);
            Assert.That(damageCard2, Is.EqualTo(0));
        }
    }

    [TestClass]
    public class GameHandlerTests02 { 
        [TestMethod]
        public void fightTest_01() {
            Mock<Database> mockedDb = new();
            GameHandler gh = new(mockedDb.Object);

            Card card1 = new(1, "WaterGoblin", Element.WATER, 12, MonsterTradingCardGame.Type.GOBLIN);
            Card card2 = new(2, "FireSpell", Element.FIRE, 4, MonsterTradingCardGame.Type.SPELL);
            
            GameHandler.FightRecord record = gh.fight(card1, card2);

            Assert.IsNotNull(record);
            Assert.That(record.player1, Is.EqualTo("Player1"));
            Assert.That(record.cardDamage1, Is.EqualTo(24));
            Assert.That(record.cardDamage2, Is.EqualTo(2));
            Assert.That(record.fightResult, Is.EqualTo(FightResult.PLAYER1));
        }

        [TestMethod]
        public void fightTest_02() {
            Mock<Database> mockedDb = new();
            GameHandler gh = new(mockedDb.Object);

            Card card1 = new(1, "WaterElf", Element.WATER, 12, MonsterTradingCardGame.Type.ELF);
            Card card2 = new(2, "FireDragon", Element.FIRE, 25, MonsterTradingCardGame.Type.DRAGON);

            GameHandler.FightRecord record = gh.fight(card1, card2);

            Assert.IsNotNull(record);
            Assert.That(record.cardDamage1, Is.EqualTo(12));
            Assert.That(record.cardDamage2, Is.EqualTo(0));
            Assert.That(record.fightResult, Is.EqualTo(FightResult.PLAYER1));
        }

        [TestMethod]
        public void fightTest_03() {
            Mock<Database> mockedDb = new();
            GameHandler gh = new(mockedDb.Object);

            Card card1 = new(1, "NormalOrk", Element.WATER, 12, MonsterTradingCardGame.Type.ORK);
            Card card2 = new(2, "GrassWizzard", Element.FIRE, 1, MonsterTradingCardGame.Type.WIZZARD);

            GameHandler.FightRecord record = gh.fight(card1, card2);

            Assert.IsNotNull(record);
            Assert.That(record.cardDamage1, Is.EqualTo(0));
            Assert.That(record.cardDamage2, Is.EqualTo(1));
            Assert.That(record.fightResult, Is.EqualTo(FightResult.PLAYER2));

            string msg = (record with { player1 = "manuel", player2 = "peter" }).ToString();
            Assert.That(msg, Is.EqualTo("manuel: NormalOrk (0 Damage) vs peter: GrassWizzard (1 Damage) => GrassWizzard wins"));
        }

        [TestMethod]
        public void joinGameTest_01() {
            Mock<Database> mockedDb = new();
            GameHandler gh = new(mockedDb.Object);

            Guid user1 = Guid.NewGuid();
            Guid user2 = Guid.NewGuid();

            Status status1 = gh.joinGame(user1);
            Assert.That(status1, Is.EqualTo(Status.WAITING));

            Status status2 = gh.joinGame(user2);
            Assert.That(status2, Is.EqualTo(Status.STARTED));
        }

        [TestMethod]
        public void joinGameTest_02() {
            Mock<Database> mockedDb = new();
            GameHandler gh = new(mockedDb.Object);

            Guid user1 = Guid.NewGuid();

            Status status1 = gh.joinGame(user1);
            Assert.That(status1, Is.EqualTo(Status.WAITING));

            Status status2 = gh.joinGame(user1);
            Assert.That(status2, Is.EqualTo(Status.WAITING));
        }

        [TestMethod]
        public void gameTest_01() {
            Guid user1 = Guid.NewGuid();
            Guid user2 = Guid.NewGuid();
            Mock<Database> mockedDB = new();
            mockedDB.Setup(x => x.getDeck(It.IsAny<Guid>())).Returns((Guid id) => {
                List<Tuple<int, Card>>? deck = new();
                if (id == user1) {
                    for (int j = 0; j < 4; j++)
                        deck.Add(Tuple.Create<int, Card>(j, new(1, "NormalKraken", Element.NORMAL, 12, Type.KRAKEN)));
                    return deck;
                } else {
                    for (int j = 0; j < 4; j++)
                        deck.Add(Tuple.Create<int, Card>(j, new(1, "FireSpell", Element.FIRE, 12, Type.SPELL)));
                    return deck;
                }
            });
            mockedDB.Setup(x => x.getUsername(It.IsAny<Guid>())).Returns((Guid user) =>
                user == user1 ? "manuel" : "peter"
            );
            mockedDB.Setup(x => x.updateStats(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<FightResult>()))
                .Callback((Guid _u, Guid _u2, FightResult fightResult) => {
                    Assert.That(fightResult, Is.EqualTo(FightResult.PLAYER1));
                });

            GameHandler gh = new(mockedDB.Object);

            gh.Game(user1, user2);

            Assert.That(gh.viewStatus(user1), Is.EqualTo(Status.FINISHED));
            Assert.That(gh.getProtocol(user1)?.Item1, Is.EqualTo(FightResult.PLAYER1));
            mockedDB.Verify(x => x.updateStats(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<FightResult>()), Times.Once);
        }

        [TestMethod]
        public void gameTest_02() {
            Guid user1 = Guid.NewGuid();
            Guid user2 = Guid.NewGuid();
            Mock<Database> mockedDB = new();
            mockedDB.Setup(x => x.getDeck(It.IsAny<Guid>())).Returns((Guid id) => {
                List<Tuple<int, Card>>? deck = new();
                if (id == user1) {
                    for (int j = 0; j < 4; j++)
                        deck.Add(Tuple.Create<int, Card>(j, new(1, "NormalKraken", Element.NORMAL, 12, Type.KRAKEN)));
                    return deck;
                } else {
                    for (int j = 0; j < 4; j++)
                        deck.Add(Tuple.Create<int, Card>(j, new(1, "FireSpell", Element.FIRE, 12, Type.SPELL)));
                    return deck;
                }
            });
            mockedDB.Setup(x => x.getUsername(It.IsAny<Guid>())).Returns((Guid user) =>
                user == user1 ? "manuel" : null
            );
            mockedDB.Setup(x => x.updateStats(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<FightResult>()))
                .Callback((Guid _u, Guid _u2, FightResult fightResult) => {
                    Assert.That(fightResult, Is.EqualTo(FightResult.PLAYER1));
                });

            GameHandler gh = new(mockedDB.Object);

            gh.Game(user1, user2);

            Assert.That(gh.viewStatus(user1), Is.EqualTo(Status.FAILED));
            Assert.That(gh.viewStatus(user2), Is.EqualTo(Status.FAILED));
            mockedDB.Verify(x => x.updateStats(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<FightResult>()), Times.Never);
        }

        [TestMethod]
        public void gameTest_03() {
            Guid user1 = Guid.NewGuid();
            Mock<Database> mockedDb = new();

            GameHandler gh = new(mockedDb.Object);

            gh.Game(user1, user1);

            Assert.That(gh.viewStatus(user1), Is.EqualTo(Status.FAILED));
        }
    }
}
