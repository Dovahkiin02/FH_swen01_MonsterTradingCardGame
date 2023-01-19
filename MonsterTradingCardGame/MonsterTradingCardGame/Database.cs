using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Transactions;
using Npgsql;
using NpgsqlTypes;
using static MonsterTradingCardGame.GameHandler;

namespace MonsterTradingCardGame {
    internal class Database {
        private readonly NpgsqlConnection con;
        const int packageSize = 5;
        public Database() {
            string host = "localhost";
            string username = "postgres";
            string pw = "asdf";
            string db = "mtcg";
            con = new NpgsqlConnection($"Host={host};Username={username};Password={pw};Database={db}");
        }

        public void connect() {
            con.Open();
        }

        public bool isConnected() {
            return con.State == ConnectionState.Open;
        }

        public void setup() {
            using NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = con;

            string strText = File.ReadAllText("setup.sql", Encoding.UTF8);
            cmd.CommandText = strText;

            cmd.ExecuteNonQuery();
        }

        public Guid? verifyUser(string name, string password) {
            string sql = @"
                select id
                  from player
                 where name = @name
                   and password = crypt(@password, password)
                ;";
            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@password", password);
            object? result = cmd.ExecuteScalar();
            if (result == null) {
                return null;
            }
            return Guid.Parse(result.ToString());
        }

        public virtual string? getUsername(Guid userId) {
            string sql = @"
                select name
                  from player
                 where id = @id
                ;";
            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("@id", userId);
            object? result = cmd.ExecuteScalar();
            if (result == null) {
                return null;
            }
            return result.ToString();
        }

        public bool addUser(string name, string password, int coins, Role role, int elo, out string errMsg) {
            errMsg = "";
            string sql = @"
                insert into player
                    (id     ,  name,        password                ,  coins ,  role, wins, defeats, draws, elo)
                values
                    (default, @name, crypt(@password, gen_salt('bf')), @coins, @role,  0  ,   0    , 0    , @elo  )
                ;";
            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("password", password);
            cmd.Parameters.AddWithValue("coins", coins);
            cmd.Parameters.AddWithValue("role", (int)role);
            cmd.Parameters.AddWithValue("elo", elo);

            try {
                return cmd.ExecuteNonQuery() > 0;
            } catch (PostgresException e) {
                errMsg = "user already exists";
                Console.WriteLine(e.Message);
                return false;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public User? getUser(Guid userId) {
            string sql = @"
                select id, name, role, coins, wins, defeats, draws, elo
                  from Player 
                 where id = @userId
            ;";
            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@userId", userId);
            using NpgsqlDataReader reader = cmd.ExecuteReader();
            if (!reader.Read()) { 
                return null;
            }
            return new User(
                id: reader.GetGuid(0), 
                name: reader.GetString(1), 
                role: (Role)reader.GetInt16(2), 
                coins: reader.GetInt32(3), 
                wins: reader.GetInt32(4), 
                defeats: reader.GetInt32(5), 
                draws: reader.GetInt32(6),
                elo: reader.GetInt32(7)
                );
        }

        public bool updateUser(Guid userId, Dictionary<string, object> parameter) {
            if (parameter == null || parameter.Count == 0) {
                return false;
            }
            string sql = @"
                update player
                   set {0}
                 where id = @id
                ;";
            
            sql = string.Format(sql, string.Join(", ", parameter.Keys.Select(e => 
                                            $"{e} = {(e == "password" ? $"crypt(@{e}, gen_salt('bf'))" : $"@{e}")}"
                                            ).ToList()));

            using NpgsqlCommand cmd = new (sql, con);
            cmd.Parameters.AddWithValue("id", userId);
            parameter.ToList().ForEach(kvp => cmd.Parameters.AddWithValue(kvp.Key, kvp.Value));
            try {
                int result = cmd.ExecuteNonQuery();
                if (result <= 0) {
                    return false;
                }
                return true;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }    
        }

        public bool addCard(string name, Element elemet, int damage, Type type) {
            string sql = @"
                insert into card
                    (id     ,  name,  element,  damage,  type)
                values
                    (default, @name, @element, @damage, @type)
                ;";
            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("name"   , name);
            cmd.Parameters.AddWithValue("element", (int)elemet);
            cmd.Parameters.AddWithValue("damage" , damage);
            cmd.Parameters.AddWithValue("type"   , (int)type);
            try {
                return cmd.ExecuteNonQuery() > 0;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool cardInStack(Guid userId, int cardId) {
            string sql = @"
                select id
                  from stack
                 where user = @user
                   and card = @card
                ;";
            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("user", userId);
            cmd.Parameters.AddWithValue("card", cardId);
            return cmd.ExecuteScalar() != null;
        }

        private string repeatString(string str, int count) {
            return new StringBuilder(str.Length * count)
                .Insert(0, str, count).ToString();
        }

        private List<Card>? getCardListFromReader(NpgsqlDataReader reader) {
            if (reader == null) {
                return null;
            }
            List<Card> cardList = new();
            while (reader.Read()) {
                cardList.Add(new Card(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    (Element)reader.GetInt32(2),
                    reader.GetInt32(3),
                    (Type)reader.GetInt32(4)));
            }
            return cardList;
        }

        private List<Tuple<int, Card>>? getIndexedCardListFromReader(NpgsqlDataReader reader) {
            if (reader == null) {
                return null;
            }
            List<Tuple<int, Card>> cardList = new();
            while (reader.Read()) {
                Card card = new Card(reader.GetInt32(1),
                                    reader.GetString(2),
                                    (Element)reader.GetInt32(3),
                                    reader.GetInt32(4),
                                    (Type)reader.GetInt32(5));
                cardList.Add(new Tuple<int, Card>(reader.GetInt32(0), card));
            }
            return cardList;
        }

        public List<Card>? buyPackage(Guid userId, int cost) {
            using NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = con;

            cmd.CommandText = @$"
                select *
                  from (
	                select * from Card
	                    {repeatString(@"
                            union all
                            select * from Card", packageSize - 1)}
                    ) as c
                order by random()
                limit {packageSize};";

            List<Card>? package;
            try {
                using NpgsqlDataReader reader = cmd.ExecuteReader();
                package = getCardListFromReader(reader);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return null;
            }
            
            if (package == null) {
                throw new Exception("found no cards for Package");
            } else if (package.Count == 0) {
                Console.WriteLine("package is empty");
                return null;
            }

            using NpgsqlTransaction transaction = con.BeginTransaction();
            cmd.Transaction = transaction;

            try {
                cmd.CommandText = @"
                    update player
                       set coins = coins - @cost
                     where id = @id
                    ;";
                cmd.Parameters.AddWithValue("@cost", cost);

                cmd.Parameters.Add("@id", NpgsqlDbType.Uuid).Value = userId;
                cmd.ExecuteNonQuery();

                cmd.CommandText = $@"
                    insert into stack
                        (id     , player,  card, inDeck)
                    values
                        (default, @id   , @card, False ) 
                    ;";
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.Add("@card", NpgsqlDbType.Integer);

                foreach (Card card in package) {
                    cmd.Parameters["@card"].Value = card.id;
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            } catch (Exception e) {
                Console.WriteLine("buyPackage failed");
                Console.WriteLine(e.Message);
                try {
                    transaction.Rollback();
                } catch (Exception e2) {
                    Console.WriteLine(e2);
                    Console.WriteLine("Rollback failed, probably connection down");
                }
                return null;
            }

            return package;
        }

        public List<Tuple<int, Card>>? getStack(Guid userId) {   
            string sql = @"
                select s.id, c.id as cardId, c.name, c.element, c.damage, c.type
                  from stack s
                  join card c on c.id = s.card
                 where s.player = @id 
                ;";
            try {
                using NpgsqlCommand cmd = new (sql, con);
                cmd.Parameters.AddWithValue("id", userId);
                using NpgsqlDataReader reader = cmd.ExecuteReader();
                return getIndexedCardListFromReader(reader);
            } catch (Exception e) {
                Console.WriteLine(e);
                return null;
            }
        }

        public virtual List<Tuple<int, Card>>? getDeck(Guid userId) {
            string sql = @"
                select d.id, d.card as cardId, c.name, c.element, c.damage, c.type
                  from deck d
                  join card c on c.id = d.card
                 where d.player = @id 
                ;";
            try {
                using NpgsqlCommand cmd = new (sql, con);
                cmd.Parameters.AddWithValue("id", userId);
                using NpgsqlDataReader reader = cmd.ExecuteReader();
                return getIndexedCardListFromReader(reader);
            } catch (Exception e) {
                Console.WriteLine(e);
                return null;
            }
        }

        public bool checkCardsInStack(Guid userId, List<int> cards) {
            if (cards.Count == 0)
                return false;

            string sql = @"
                select count(*)
                  from stack
                 where player = @id
                   and id = any(@cards)
                   and id not in (select stackid
                                    from store
                                 )
                ;";

            using NpgsqlCommand cmd = new (sql, con);
            cmd.Parameters.AddWithValue("id", userId);
            cmd.Parameters.AddWithValue("cards", cards);
            var result = cmd.ExecuteScalar();
            if (result == null)
                return false;
            if (int.TryParse(result.ToString(), out int rowNumber)) {
                if (rowNumber == cards.Count) {
                    return true;
                }
            }
            return false;
        }

        public bool setDeck(Guid userId, List<int> cards) {
            using NpgsqlCommand cmd = new();
            cmd.Connection = con;

            using NpgsqlTransaction transaction = con.BeginTransaction();
            cmd.Transaction = transaction;

            string deleteSql = @"
                delete from deck
                 where player = @id
            ;";
            string insertSql = @"
                insert into deck
                    (id, player, card)
                select s.id, @id player, s.card
                  from stack s
                  join unnest(@cards) c on s.id = c
                 where s.player = @id
                ;";
            try {
                cmd.CommandText = deleteSql;
                cmd.Parameters.AddWithValue("id", userId);
                int result = cmd.ExecuteNonQuery();
                if (result < 0) {
                    Console.WriteLine("delete cards from deck failed");
                    transaction.Rollback();
                    return false;
                }

                cmd.CommandText = insertSql;
                cmd.Parameters.AddWithValue("id", userId);
                cmd.Parameters.AddWithValue("cards", cards);
                result = cmd.ExecuteNonQuery();
                if (result <= 0) {
                    Console.WriteLine("insert cards into deck failed");
                    transaction.Rollback();
                    return false;
                }
                transaction.Commit();
                return true;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                transaction.Rollback();
                return false;
            }
        }

        public virtual bool updateStats(Guid user1, Guid user2, FightResult fightResult) {
            string sql = @"
                update player
                   set {0} = {0} + 1,
                       elo = elo {1}
                 where id = @id
                ;";
            string sqlUser1, sqlUser2;

            switch (fightResult) {
                case FightResult.PLAYER1:
                    sqlUser1 = string.Format(sql, "wins", "+ 3");
                    sqlUser2 = string.Format(sql, "defeats", "- 5");
                    break;
                case FightResult.PLAYER2:
                    sqlUser1 = string.Format(sql, "defeats", "- 5");
                    sqlUser2 = string.Format(sql, "wins", "+ 3");
                    break;
                default: // no elo gains or losses for draw
                    sqlUser1 = string.Format(sql, "draws", "");
                    sqlUser2 = string.Format(sql, "draws", "");
                    break;
            }

            using NpgsqlCommand cmdUser1 = new(sqlUser1, con);
            cmdUser1.Parameters.AddWithValue("id", user1);
            
            using NpgsqlTransaction transaction = con.BeginTransaction();
            cmdUser1.Transaction = transaction;
            try {
                int result = cmdUser1.ExecuteNonQuery();
                if (result <= 0) {
                    transaction.Rollback();
                    return false;
                }
                using NpgsqlCommand cmdUser2 = new(sqlUser2, con);
                cmdUser2.Parameters.AddWithValue("id", user2);

                result = cmdUser2.ExecuteNonQuery();
                if (result <= 0) {
                    transaction.Rollback();
                    return false;
                }
                transaction.Commit();
                return true;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                transaction.Rollback();
                return false;
            }
            
        }

        public bool checkCardAvailability(Guid userId, int stackId) {
            string sql = @"
                select id
                  from stack
                 where id = @stackId
                   and player = @id
                ;";

            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("id", userId);
            cmd.Parameters.AddWithValue("stackId", stackId);

            return cmd.ExecuteScalar() != null;
        }

        public List<ValueTuple<int, User>>? getScoreboard() {
            string sql = @"
                select row_number() over (order by elo desc), id, name, role, coins, wins, defeats, draws, elo
                  from player
                 where name != 'admin'
                ;";

            using NpgsqlCommand cmd = new(sql, con);
            using NpgsqlDataReader reader = cmd.ExecuteReader();
            if (reader == null) {
                return null;
            }

            List<ValueTuple<int, User>> scoreboard = new();
            while (reader.Read()) {
                scoreboard.Add((
                    reader.GetInt32(0),
                    new User (
                        id:      reader.GetGuid(1),
                        name:    reader.GetString(2),
                        role:    (Role)reader.GetInt32(3),
                        coins:   reader.GetInt32(4),
                        wins:    reader.GetInt32(5),
                        defeats: reader.GetInt32(6),
                        draws:   reader.GetInt32(7),
                        elo:     reader.GetInt32(8)
                    )
                ));
            }
            return scoreboard;
        }

        public List<ValueTuple<int, int, string, Card>>? getStoreOffers() {
            string sql = @"
                select st.id, s.price, p.name, c.id, c.name, c.element, c.damage, c.type
                  from store s
                  join stack st on st.id = s.stackid
                  join card c on st.card = c.id
                  join player p on p.id = st.player
                ;";

            using NpgsqlCommand cmd = new(sql, con);
            using var reader = cmd.ExecuteReader();
            if (reader == null) {
                return null;
            }
            List<ValueTuple<int, int, string, Card>> offers = new();
            while (reader.Read()) {
                offers.Add((
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    new Card(
                        id:      reader.GetInt32(3),
                        name:    reader.GetString(4),
                        element: (Element)reader.GetInt32(5),
                        damage:  reader.GetInt32(6),
                        type:    (Type)reader.GetInt32(7)
                    )
                ));
            }
            return offers;
        }

        public bool addOfferToStore(int stackId, int price) {
            string sql = @"
                insert into store
                    (stackid, price )
                values
                    (@id    , @price)
                ;";

            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("id", stackId);
            cmd.Parameters.AddWithValue("price", price);

            try {
                int result = cmd.ExecuteNonQuery();
                if (result <= 0) {
                    Console.WriteLine("insert into store failed");
                    return false;
                }

                return true;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool checkDuplicateOfferInStore(int stackId) {
            string sql = @"
                select stackid
                  from store
                 where stackid = @stackId
                ;";
            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("stackId", stackId);

            return cmd.ExecuteScalar() != null;
        }

        public bool buyCardFromStore(Guid userId, int stackId) {
            using NpgsqlCommand cmd = new();
            cmd.Connection = con;
            string getPriceSql = @"
                select price
                  from store
                 where stackid = @stackId
                ;";

            cmd.CommandText = getPriceSql;
            cmd.Parameters.AddWithValue("stackId", stackId);
            object? resObj = cmd.ExecuteScalar();
            if (resObj == null) {
                return false;
            }
            int price = Convert.ToInt32(resObj);

            string updatePlayerSql = @"
                update player
                   set coins = coins - @price
                 where id = @id
                ;";

            string updateStackSql = @"
                update stack
                   set player = @id
                 where id = @stackId
                ;";

            string deleteOfferSql = @"
                delete from store
                 where stackid = @stackId
                ;";

            using NpgsqlTransaction transaction = con.BeginTransaction();
            cmd.Transaction = transaction;
            try {
                cmd.CommandText = updatePlayerSql;
                cmd.Parameters.AddWithValue("price", price);
                cmd.Parameters.AddWithValue("id", userId);
                if (cmd.ExecuteNonQuery() <= 0) {
                    transaction.Rollback();
                    return false;
                }

                cmd.CommandText = updateStackSql;
                if (cmd.ExecuteNonQuery() <= 0) {
                    transaction.Rollback();
                    return false;
                }

                cmd.CommandText = deleteOfferSql;
                if (cmd.ExecuteNonQuery() <= 0) {
                    transaction.Rollback();
                    return false;
                }
                transaction.Commit();
                return true;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                transaction.Rollback();
                return false;
            }
        }

        public bool checkOffer(Guid userId, int stackId) {
            string sql = @"
                select st.id
                  from store s
                  join stack st on st.id = s.stackid
                 where st.id = @stackId
                   and st.player != @id
                ;";

            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("stackId", stackId);
            cmd.Parameters.AddWithValue("id", userId);

            return cmd.ExecuteScalar() != null;
        }

        public int? getPriceOfOffer(int stackId) {
            string sql = @"
                select price
                  from store
                 where stackid = @stackId
                ;";

            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("stackId", stackId);

            return (int?)cmd.ExecuteScalar();
        }
    }
}
