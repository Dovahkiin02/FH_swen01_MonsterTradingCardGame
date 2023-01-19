using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;

namespace MonsterTradingCardGame {
    internal class GetRequestHandlers : RequestHandler {
        public GetRequestHandlers(Database db) : base(db) {}

        public void getStack(TcpClient client, JObject body, User user) {
            List<Tuple<int, Card>>? stack = db.getStack(user.id);
            string responseMsg;
            if (stack == null) {
                responseMsg = "unexpected error while trying to fetch stack";
                writeStructuredResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                return;
            } else if (stack.Count == 0) {
                responseMsg = "no stack defined";
                writeStructuredResponse(client, HttpStatusCode.NotFound, responseMsg);
                return;
            }
            JArray responseBody = new();
            stack.Select(e => new JObject() {
                ["id"] = e.Item1,
                ["card"] = e.Item2.toResponseObject()
            }).ToList().ForEach(e => responseBody.Add(e));

            writeResponse(client, HttpStatusCode.OK, responseBody);            
        }

        public void getDeck(TcpClient client, JObject body, User user) {
            List<Tuple<int, Card>>? deck = db.getDeck(user.id);
            string responseMsg;
            if (deck == null) {
                responseMsg = "unexpected error while trying to fetch stack";
                writeStructuredResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                return;
            } else if (deck.Count == 0) {
                responseMsg = "no deck defined";
                writeStructuredResponse(client, HttpStatusCode.NotFound, responseMsg);
                return;
            }
            JArray responseBody = new();
            deck.Select(e => new JObject() {
                ["id"] = e.Item1,
                ["card"] = e.Item2.toResponseObject()
            }).ToList().ForEach(e => responseBody.Add(e));

            writeResponse(client, HttpStatusCode.OK, responseBody);
        }

        public void getUser(TcpClient client, JObject body, User user) {
            writeResponse(client, HttpStatusCode.OK, user.toResponseObject());
        }

        public void getStoreOffers(TcpClient client, JObject body, User user) {
            List<ValueTuple<int, int, string, Card>>? offers = db.getStoreOffers();
            
            if (offers == null || offers.Count == 0) {
                string msg = "no offers found in store";
                writeStructuredResponse(client, HttpStatusCode.NotFound, msg);
                return;
            }

            JArray responseBody = new();
            offers.Select(e => new JObject() {
                ["id"] = e.Item1,
                ["price"] = e.Item2,
                ["seller"] = e.Item3,
                ["card"] = e.Item4.toResponseObject()
            }).ToList().ForEach(e => responseBody.Add(e));

            writeResponse(client, HttpStatusCode.OK, responseBody);
        }

        public void getScoreboard(TcpClient client, JObject body, User user) {
            List<ValueTuple<int, User>>? scoreboard = db.getScoreboard();
            if (scoreboard == null || scoreboard.Count == 0) {
                string msg = "no scoreboard found";
                writeStructuredResponse(client, HttpStatusCode.NoContent, msg);
                return;
            }

            JArray responseBody = new();
            scoreboard.Select(e => new JObject() {
                ["rank"] = e.Item1,
                ["user"] = e.Item2.toScoreBoardResponseObject()
            }).ToList().ForEach(e => responseBody.Add(e));
            writeResponse(client, HttpStatusCode.OK, responseBody);
        }
    }
}
