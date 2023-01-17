using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGameTests {
    [TestClass]
    public class UserTests {
        [TestMethod]
        public void UserTest_basic01() {
            Guid id = Guid.NewGuid();
            User user = new(id, "testUser", Role.ADMIN, 100);

            Assert.IsNotNull(user);
            Assert.That(user.id, Is.EqualTo(id));
            Assert.That(user.name, Is.EqualTo("testUser"));
            Assert.That(user.role, Is.EqualTo(Role.ADMIN));
            Assert.That(user.coins, Is.EqualTo(100));
            Assert.That(user.defeats, Is.EqualTo(0));
        }

        [TestMethod]
        public void UserTest_toJson() {
            Guid id = Guid.NewGuid();
            User user = new(id, "testToJson", Role.PLAYER, 120, elo: 1200);

            Assert.IsNotNull(user);
            JObject res = user.toResponseObject();

            Assert.IsNotNull(res);
            Assert.That(res["id"].ToString(), Is.EqualTo(id.ToString()));
            Assert.That(res["elo"].ToString(), Is.EqualTo("1200"));
        }
    }
}
