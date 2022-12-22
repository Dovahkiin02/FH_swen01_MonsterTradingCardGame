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

        public void getStack(TcpClient client, JObject body) {
            if (body.Count == 0) {
                Console.WriteLine("body is empty");
            }
            
            writeResponse(client, System.Net.HttpStatusCode.OK, body);
        }

        public void getDeck(TcpClient client, JObject body) { }

        public void getStats(TcpClient client, JObject body) { }
    }
}
