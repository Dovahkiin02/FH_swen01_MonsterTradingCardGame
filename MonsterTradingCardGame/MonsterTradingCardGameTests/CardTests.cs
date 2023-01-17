using Newtonsoft.Json.Linq;
using Type = MonsterTradingCardGame.Type;

namespace MonsterTradingCardGameTests {
    [TestClass]
    public class CardTests {
        [TestMethod]
        public void cardTest_basic01() {
            Card card = new(1, "WaterGoblin", Element.WATER, 20, MonsterTradingCardGame.Type.GOBLIN);

            Assert.IsNotNull(card);
            Assert.That(card.id, Is.EqualTo(1));
            Assert.That(card.element, Is.EqualTo(Element.WATER));
            Assert.That(card.damage, Is.EqualTo(20));
            Assert.That(card.type, Is.EqualTo(Type.GOBLIN));
        }

        [TestMethod]
        public void cardTest_basic02() {
            Card card = new(25, "FireSpell", Element.FIRE, 5, Type.SPELL);

            Assert.IsNotNull(card);
            Assert.That(card.id, Is.EqualTo(25));
            Assert.That(card.element, Is.EqualTo(Element.FIRE));
            Assert.That(card.damage, Is.EqualTo(5));
            Assert.That(card.type, Is.EqualTo(Type.SPELL));
        }

        [TestMethod]
        public void cardTest_create() {
            JObject obj = new();
            obj["body"] = new JObject(
                new JProperty("id", 20),
                new JProperty("name", "GrassDragon"),
                new JProperty("element", Element.GRASS),
                new JProperty("damage", 100),
                new JProperty("type", Type.DRAGON)
            );

            Card card = Card.buildCard(obj["body"]);

            Assert.IsNotNull(card);
            Assert.That(card.id, Is.EqualTo(20));
            Assert.That(card.element, Is.EqualTo(Element.GRASS));
            Assert.That(card.damage, Is.EqualTo(100));
            Assert.That(card.type, Is.EqualTo(Type.DRAGON));
        }
    }
}
