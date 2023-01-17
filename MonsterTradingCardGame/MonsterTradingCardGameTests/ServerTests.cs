using Moq;
using Newtonsoft.Json;

namespace MonsterTradingCardGameTests {
    [TestClass]
    public class ServerTest {
        private static Mock<Database> mockedDb = new ();

        private static Server server = new(9999, mockedDb.Object);

        [TestMethod]
        public void parseRequestTest_Basic() {
            string mockedRequest = "POST /user HTTP/1.1\nHost: localhost\nContent-Type: text/json\n\n{\"key\": \"value\"}";

            HttpRequest req = server.parseRequest(mockedRequest);

            Assert.IsNotNull(req);
            Assert.That(req.method, Is.EqualTo("POST"));
            Assert.That(req.resource, Is.EqualTo("/user"));
            Assert.That(req.headers["Content-Type"], Is.EqualTo("text/json"));
        }

        [TestMethod]
        public void parseRequestTest_Body() {
            string mockedRequest = "GET /user HTTP/1.1\nHost: localhost\nContent-Type: text/json\n\n{\"id\": \"20\"}";

            HttpRequest req = server.parseRequest(mockedRequest);

            Assert.IsNotNull(req);
            Assert.That(req.method, Is.EqualTo("GET"));
            Assert.That(req.resource, Is.EqualTo("/user"));
            Assert.That(req.httpVersion, Is.EqualTo("HTTP/1.1"));
            Assert.That(req.body["id"].ToString(), Is.EqualTo("20"));
        }

        [TestMethod]
        public void parseRequestTest_MalformedBody() {
            string mockedRequest = "GET /user HTTP/1.1\nHost: localhost\nContent-Type: text/json\n\n\"id\": \"20\"}";

            Assert.Throws<JsonReaderException>(() => server.parseRequest(mockedRequest));
        }
    }
}