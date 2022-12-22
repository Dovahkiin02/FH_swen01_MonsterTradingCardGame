using MonsterTradingCardGame;
using Npgsql;

Database db = new();
db.connect();
Server s = new Server(9999, db);
s.Start();
