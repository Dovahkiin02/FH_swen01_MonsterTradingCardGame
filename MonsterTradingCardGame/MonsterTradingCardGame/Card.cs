using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame {
    public enum Element {
        FIRE  = 1,
        WATER = 2,
        WIND  = 3,
        EARTH = 4,
    }
    public enum Type {
        MONSTER = 1,
        SPELL   = 2,
    }
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
    }
}
