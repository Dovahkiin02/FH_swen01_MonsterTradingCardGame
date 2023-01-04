using System.Data;
using System.Reflection.PortableExecutable;
using System.Text;
using Npgsql;
using NpgsqlTypes;

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

        public void test() {
            string sql = "SELECT version()";

            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);

            string version = cmd.ExecuteScalar().ToString();
            Console.WriteLine($"PostgreSQL version: {version}");
        }

        public void setup() {
            using NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = con;

            string strText = File.ReadAllText("setup.sql", Encoding.UTF8);
            cmd.CommandText = strText;

            cmd.ExecuteNonQuery();
        }

        public Guid? verifyUser(string username, string password) {
            string sql = @"
                select id
                  from player
                 where name = @username
                   and password = crypt(@password, password)
                ;";
            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            object? result = cmd.ExecuteScalar();
            if (result == null) {
                return null;
            }
            return Guid.Parse(result.ToString());
        }

        public string? getUsername(Guid userId) {
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

        public bool addUser(string username, string password, int coins, Role role) {
            string sql = @"
                insert into player
                    (id     ,  name,        password                ,  coins ,  role, wins, defeats, ties)
                values
                    (default, @name, crypt(@password, gen_salt('bf')), @coins, @role,  0  ,   0    , 0   )
                ;";
            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("@name", username);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@coins", coins);
            cmd.Parameters.AddWithValue("@role", (int)role);

            try {
                return cmd.ExecuteNonQuery() > 0;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public User? getUser(Guid userId) {
            string sql = @"
                select id, name, role, coins, wins, defeats, ties
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
                username: reader.GetString(1), 
                role: (Role)reader.GetInt16(2), 
                coins: reader.GetInt32(3), 
                wins: reader.GetInt32(4), 
                defeats: reader.GetInt32(5), 
                ties: reader.GetInt32(6)
                );
        }

        public int getCoins(Guid userId) {
            string sql = @"
                select coins
                  from player
                 where id = @id
                 ;";

            using NpgsqlCommand cmd = new NpgsqlCommand(sql);
            cmd.Parameters.AddWithValue("@id", userId);
            return int.Parse(cmd.ExecuteScalar()?.ToString() ?? "-1");
        }

        public bool addCard(string name, Element elemet, int damage, Type type) {
            string sql = @"
                insert into card
                    (id     ,  name,  element,  damage,  type)
                values
                    (default, @name, @element, @damage, @type)
                ;";
            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("@name"   , name);
            cmd.Parameters.AddWithValue("@element", (int)elemet);
            cmd.Parameters.AddWithValue("@damage" , damage);
            cmd.Parameters.AddWithValue("@type"   , (int)type);
            try {
                return cmd.ExecuteNonQuery() > 0;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
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
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return null;
            }
            
            if (package == null) {
                throw new Exception("found no cards for Package");
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
                        (id     , player,  card)
                    values
                        (default, @id   , @card) 
                    ;";
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.Add("@card", NpgsqlDbType.Integer);

                foreach (Card card in package) {
                    cmd.Parameters["@card"].Value = card.id;
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            } catch (Exception e) {
                // log exception here
                Console.WriteLine("buyPackage failed");
                Console.WriteLine(e.Message);
                try {
                    transaction.Rollback();
                } catch (Exception ex2) {
                    // log exception here
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
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return null;
            }
        }

        public List<Tuple<int, Card>>? getDeck(Guid userId) {
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
            } catch (Exception ex) {
                Console.WriteLine(ex);
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
                ;";

            using NpgsqlCommand cmd = new (sql, con);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@cards", cards);
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

        public bool addCardsToDeck(Guid userId, List<int> cards) {
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
                    (player, card)
                select @id player, s.card
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

        //public bool addOfferToStore(Guid userId, )
    }
}
