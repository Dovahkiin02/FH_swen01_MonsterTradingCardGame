using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame {
    public enum Element {
        NORMAL = 1,
        FIRE = 2,
        WATER = 3,
        GRASS = 4,
    }
    public enum Type {
        SPELL = 1,
        GOBLIN,
        ORK,
        DRAGON,
        WIZZARD,
        KNIGHT,
        KRAKEN,
        ELF,
    }
    [JsonObject]
    public record Card (int id, string name, Element element, int damage, Type type) {
        public override string ToString() {
            return $@"
                id: {id},
                name: {name},
                element: {element.ToString()},
                damage: {damage},
                type: {type.ToString()}
            ";
        }

        

        public static Card buildCard(JToken jToken) => new(
            jToken.Value<int>("id"),
            jToken.Value<string>("name"),
            jToken.Value<Element>("element"),
            jToken.Value<int>("damage"),
            jToken.Value<Type>("type")
            );

        
    }
}
