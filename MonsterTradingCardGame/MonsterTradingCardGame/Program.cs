using MonsterTradingCardGame;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using Type = MonsterTradingCardGame.Type;

Database db = new();
db.connect();
//db.setup();
Server s = new Server(9999, db);
s.Start();

//List<Card> cards = new() {
//    new Card(1, "a", Element.WIND, 12, Type.GOBLIN),
//    new Card(2, "b", Element.WIND, 12, Type.GOBLIN),
//    new Card(3, "b", Element.WIND, 12, Type.GOBLIN),
//    new Card(4, "b", Element.WIND, 12, Type.GOBLIN),
//};
//Console.WriteLine(db.addCardsToDeck(Guid.Parse("2d79ff5d-c379-44ee-8ef3-239d11946c88"), cards));

