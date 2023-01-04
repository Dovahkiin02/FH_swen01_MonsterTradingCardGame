using MonsterTradingCardGame;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using Type = MonsterTradingCardGame.Type;

Database db = new();
db.connect();

//db.setup();
Server s = new Server(9999, db);
s.Start();

//Guid user1 = Guid.Parse("d95817b7-016b-41af-bb6f-8048b7ab036f");
//Guid user2 = Guid.Parse("48c553b2-b190-4c56-bdc6-cbbd837fdf6a");

//db.updateStats(user1, user2, FightResult.DRAW);

//List<Card> cards = new() {
//    new Card(1, "a", Element.WIND, 12, Type.GOBLIN),
//    new Card(2, "b", Element.WIND, 12, Type.GOBLIN),
//    new Card(3, "b", Element.WIND, 12, Type.GOBLIN),
//    new Card(4, "b", Element.WIND, 12, Type.GOBLIN),
//};
//Console.WriteLine(db.addCardsToDeck(Guid.Parse("2d79ff5d-c379-44ee-8ef3-239d11946c88"), cards));

