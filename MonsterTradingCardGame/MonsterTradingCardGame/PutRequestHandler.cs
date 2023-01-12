using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;

namespace MonsterTradingCardGame {
    internal class PutRequestHandler : RequestHandler {

        public PutRequestHandler(Database db) : base(db) { }

        private Dictionary<string, object> convertToDict(JObject body) {
            var dict = new Dictionary<string, object>();
            
            foreach (var item in body) {
                dict.Add(item.Key, Enum.TryParse<Role>(item.Value.ToString(), out Role res) ? 
                                                            (int)res : item.Value.ToString());
            }
            return dict;
        }

        public void updateUser(TcpClient client, JObject body, User user) {
            if (body.Count == 0 || 
                (!body.ContainsKey("name") && 
                 !body.ContainsKey("password") && 
                 !body.ContainsKey("role"))) {
                writeMalformedBodyErr(client);
                return;
            }
            if (body.ContainsKey("role")) {
                try {
                    if (Enum.Parse<Role>(body["role"].ToString()) < user.role) {
                        writeForbiddenErr(client);
                        return;
                    }
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    writeMalformedBodyErr(client);
                    return;
                } 
            }

            string responseMsg;
            if (!db.updateUser(user.id, convertToDict(body))) {
                responseMsg = "unexpected error while trying to update user";
                writeStructuredResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                return;
            }
            responseMsg = "user changed successfully";
            writeStructuredResponse(client, HttpStatusCode.OK, responseMsg);
        }
    }
}
