using System.Collections.ObjectModel;
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
            return con.State == System.Data.ConnectionState.Open;
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

            string strText = System.IO.File.ReadAllText("setup.sql", System.Text.Encoding.UTF8);
            cmd.CommandText = strText;

            cmd.ExecuteNonQuery();
        }

        public bool verifyUser(string name, string password) {
            string sql = @"
                select id
                  from player
                 where name = @name
                   and password = crypt(@password, password)
                ;";
            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@password", password);
            return cmd.ExecuteScalar() != null;
        }

        public bool addUser(User user) {
            string sql = @"
                insert into player
                    (id     ,  name,        password                ,  coins , wins, defeats, ties)
                values
                    (default, @name, crypt(@password, gen_salt('bf')), @coins,  0  ,   0    , 0   )
                ;";
            using NpgsqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("@name", user.username);
            cmd.Parameters.AddWithValue("@password", user.password);
            cmd.Parameters.AddWithValue("@coins", user.coins);
            try {
                return cmd.ExecuteNonQuery() > 0;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public int getCoins(string userId) {
            string sql = @"
                select coins
                  from player
                 where id = @id
                 ;";

            using NpgsqlCommand cmd = new NpgsqlCommand(sql);
            cmd.Parameters.AddWithValue("@id", userId);
            return int.Parse(cmd.ExecuteScalar()?.ToString() ?? "-1");
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

            NpgsqlTransaction transaction = con.BeginTransaction();
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

        public List<Card>? getStack(Guid userId) {
            string sql = @"
                select c.id, c.name, c.element, c.damage, c.type
                  from stack s
                  join card c on c.id = s.card
                 where s.player = @id 
                ;";
            try {
                using NpgsqlCommand cmd = new (sql, con);
                cmd.Parameters.AddWithValue("id", userId);
                using NpgsqlDataReader reader = cmd.ExecuteReader();
                return getCardListFromReader(reader);
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return null;
            }
        }

        public List<Card>? getDeck(Guid userId) {
            string sql = @"
                select c.id, c.name, c.element, c.damage, c.type
                  from deck d
                  join card c on c.id = d.card
                 where s.player = @id 
                ;";
            try {
                using NpgsqlCommand cmd = new (sql, con);
                cmd.Parameters.AddWithValue("id", userId);
                using NpgsqlDataReader reader = cmd.ExecuteReader();
                return getCardListFromReader(reader);
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return null;
            }
        }

        public bool isDeckDefined(Guid userId) {
            string sql = @"
                select id
                  from deck
                 where player = @id
                 limit 1
                ;";
            try {
                using NpgsqlCommand cmd = new(sql, con);
                cmd.Parameters.AddWithValue("id", userId);
                using NpgsqlDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return false;
            }
        }

        //public bool addOfferToStore(Guid userId, )
    }
}
