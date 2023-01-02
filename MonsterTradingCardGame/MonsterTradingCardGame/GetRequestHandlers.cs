using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MonsterTradingCardGame {
    internal class GetRequestHandlers : RequestHandler {
        public GetRequestHandlers(Database db) : base(db) {
        }

        public void getStack(TcpClient client, JObject body, User user) {
            List<Tuple<int, Card>>? stack = db.getStack(user.id);
            if (stack == null) {
                string msg = "unexpected error while trying to fetch stack";
                writeErr(client, System.Net.HttpStatusCode.InternalServerError, msg);
                return;
            }

            writeResponse(client, System.Net.HttpStatusCode.OK, stack);            
        }

        public void getDeck(TcpClient client, JObject body, User user) {
            List<Tuple<int, Card>>? deck = db.getDeck(user.id);
            if (deck == null) {
                string msg = "unexpected error while trying to fetch stack";
                writeErr(client, System.Net.HttpStatusCode.InternalServerError, msg);
                return;
            }

            writeResponse(client, System.Net.HttpStatusCode.OK, deck);
        }

        public void getStats(TcpClient client, JObject body, User user) { }
    }
}
