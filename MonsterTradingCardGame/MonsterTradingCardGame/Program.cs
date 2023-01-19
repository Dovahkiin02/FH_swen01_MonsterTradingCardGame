using MonsterTradingCardGame;

Database db = new();
db.connect();

db.setup();
Server s = new(9999, db);
s.Start();

//Guid manuel = Guid.Parse("d04b0154-52f6-458f-bfed-8a87e869d0b8");
//Guid peter = Guid.Parse("ec7bc835-c921-4c88-8d90-6e714e51a40d");
//int stackId = 1;

//db.addOfferToStore(stackId, 2);
//Console.WriteLine(db.buyCardFromStore(peter, stackId));


