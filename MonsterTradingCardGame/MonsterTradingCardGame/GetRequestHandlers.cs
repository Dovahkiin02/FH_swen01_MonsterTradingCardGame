using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;

namespace MonsterTradingCardGame {
    internal class GetRequestHandlers : RequestHandler {
        public GetRequestHandlers(Database db) : base(db) {
        }

        public void getStack(TcpClient client, JObject body, User user) {
            List<Tuple<int, Card>>? stack = db.getStack(user.id);
            if (stack == null) {
                string msg = "unexpected error while trying to fetch stack";
                writeStructuredResponse(client, HttpStatusCode.InternalServerError, msg);
                return;
            }

            writeResponse(client, HttpStatusCode.OK, stack);            
        }

        public void getDeck(TcpClient client, JObject body, User user) {
            List<Tuple<int, Card>>? deck = db.getDeck(user.id);
            if (deck == null) {
                string msg = "unexpected error while trying to fetch stack";
                writeStructuredResponse(client, HttpStatusCode.InternalServerError, msg);
                return;
            }

            writeResponse(client, System.Net.HttpStatusCode.OK, deck);
        }

        public void getUser(TcpClient client, JObject body, User user) {
            writeResponse(client, HttpStatusCode.OK, user.toResponseObject());
        }
    }
}
