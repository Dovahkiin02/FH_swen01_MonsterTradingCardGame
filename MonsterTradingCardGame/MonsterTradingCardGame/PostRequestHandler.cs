using Newtonsoft.Json.Linq;
using Npgsql.Replication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame {
    internal class PostRequestHandler : RequestHandler {
        public PostRequestHandler(Database db) : base(db) { }

        public void login(TcpClient client, JObject body) {
            if (!checkBody(client, body, new[] { "username", "password" })) {
                return;
            }
            if (!db.isConnected()) {
                string errMsg = "unexpected internal error";
                writeErr(client, HttpStatusCode.InternalServerError, errMsg);
            }
            if (db.verifyUser(body["username"].ToString(), body["password"].ToString())) {
                JObject response = new();
                response["Token"] = "Token string";
                writeResponse(client, HttpStatusCode.OK, response);
            } else {
                string errMsg = "unauthorized access";
                RequestHandler.writeErr(client, HttpStatusCode.Unauthorized, errMsg);
                return;
            }
        }
    }
}
