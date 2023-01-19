using MonsterTradingCardGame;

Database db = new();
db.connect();

//db.setup();
Server s = new(9999, db);
s.Start();
