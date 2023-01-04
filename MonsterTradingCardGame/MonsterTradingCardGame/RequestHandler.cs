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
            [201] = "Created", 
            [400] = "Bad Request",
            [401] = "Unauthorized",
            [403] = "Forbidden",
            [404] = "Not Found",
            [500] = "Internal Server Error"
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

        public static void writeStructuredResponse(TcpClient client, HttpStatusCode status, string msg) {
            writeResponse(client, status, buildError(status, msg));
        }

        public static void writeUnauthorizedErr(TcpClient client) {
            string msg = "unauthorized access";
            HttpStatusCode status = HttpStatusCode.Unauthorized;
            writeResponse(client, status, buildError(status, msg));
        }

        public static void writeForbiddenErr(TcpClient client) {
            string msg = "unauthorized access";
            HttpStatusCode status = HttpStatusCode.Forbidden;
            writeResponse(client, status, buildError(status, msg));
        }

        public static void writeMalformedBodyErr(TcpClient client) {
            string msg = "malformed body";
            HttpStatusCode status = HttpStatusCode.BadRequest;
            writeResponse(client, status, buildError(status, msg));
        }

        public static void writeResponse(TcpClient client, HttpStatusCode status, object? data) {
            // Serialize the data object to a JSON string
            var body = JsonConvert.SerializeObject(data ?? "");
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
            client.Close();
        }

        public static void writeSse(StreamWriter writer, object? data) {
            //var body = JsonConvert.SerializeObject(data ?? "");
            //var jsonBytes = Encoding.UTF8.GetBytes(body);
            
            writer.WriteLine("HTTP/1.1 200 OK");
            writer.WriteLine("Content-Type: text/plain");
            writer.WriteLine();
            writer.WriteLine("data: Hello, world!\n");
            writer.Flush();
            Thread.Sleep(3000);
            writer.WriteLine("test: asdf");
            writer.Flush();
            
            //writer.Close();
            //var responseText = $"HTTP/1.1 200 OK\n" +
            //                  $"Content-Type: text/event-stream\n" +
            //                  $"Content-Length: {jsonBytes.Length}\n" +
            //                  "\n" +
            //                  body;
            //var responseBytes = Encoding.UTF8.GetBytes(responseText);
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
    }
}
