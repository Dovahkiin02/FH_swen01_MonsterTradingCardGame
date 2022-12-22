using MonsterTradingCardGame;
using Npgsql;

Database db = new();
db.connect();
db.setup();
//Server s = new Server(9999, db);
//s.Start();

//string token = JwtHandler.getJwt(Guid.NewGuid());

//Thread.Sleep(100);

//Console.WriteLine(JwtHandler.validateJwt(token, out string err) + $"\n{err}");
