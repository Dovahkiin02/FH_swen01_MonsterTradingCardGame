using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using System.Collections.ObjectModel;

namespace MonsterTradingCardGame {
    internal abstract class RequestHandler {
        protected Database db;
        public RequestHandler(Database db) {
            this.db = db;
        }
    
        private static readonly Dictionary<int, string> reasonPhrases = new Dictionary<int, string> {
            [200] = "OK",
            [400] = "Bad Request",
            [404] = "Not Found",
        };

        private static string getReasonPhrase(int status) {
            if (reasonPhrases.TryGetValue(status, out var phrase)) {
                return phrase;
            } else {
                return "";
            }
        }

        private static JObject buildError(HttpStatusCode status, string msg) {
            var error = new JObject();
            error["status"] = (int)status;
            error["message"] = msg;
            return error;
        }

        public static void writeErr(TcpClient client, HttpStatusCode status, string msg) {
            writeResponse(client, status, buildError(status, msg));
        }

        public static void writeResponse(TcpClient client, HttpStatusCode status, object data) {
            // Serialize the data object to a JSON string
            var body = JsonConvert.SerializeObject(data);
            var jsonBytes = Encoding.UTF8.GetBytes(body);

            // Get the reason phrase for the status code
            var reasonPhrase = getReasonPhrase((int)status);

            // Generate the response text
            var responseText = $"HTTP/1.1 {(int)status} {reasonPhrase}\n" +
                              $"Content-Type: application/json\n" +
                              $"Content-Length: {jsonBytes.Length}\n" +
                              "\n" +
                              body;
            var responseBytes = Encoding.UTF8.GetBytes(responseText);

            // Send the response back to the client
            var stream = client.GetStream();
            stream.Write(responseBytes, 0, responseBytes.Length);
            //client.Close();
        }

        protected bool checkKeys(JObject body, string[] keys) {
            if (body.Count == 0) 
                return false;

            foreach (string key in keys) {
                if (!body.ContainsKey(key)) {
                    return false;
                } 
            }

            return true;
        }

        protected bool checkBody(TcpClient client, JObject body, string[] keys) {
            if (!checkKeys(body, keys)) {
                string errMsg = "malformed request body";
                writeErr(client, System.Net.HttpStatusCode.BadRequest, errMsg);
                return false;
            }

            return true;
        }
    }
}
