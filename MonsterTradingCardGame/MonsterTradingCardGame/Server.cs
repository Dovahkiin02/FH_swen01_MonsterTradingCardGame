using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MonsterTradingCardGame {
    internal class Server {
        private readonly Dictionary<string, Dictionary<string, Action<TcpClient, JObject>>> routes;
        private readonly int port;
        private readonly Database db;
        private readonly GetRequestHandlers getRequestHandlers;
        private readonly PostRequestHandler postRequestHandlers;

        private void addRoutes() {
            routes.Add("GET", new Dictionary<string, Action<TcpClient, JObject>> {
                ["/stack"] = getRequestHandlers.getStack,
                ["/deck"] = getRequestHandlers.getDeck
            });
            routes.Add("POST", new Dictionary<string, Action<TcpClient, JObject>> {
                ["/login"] = postRequestHandlers.login
            });
        }

        public Server(int port, Database db) {
            this.port = port;
            this.db = db;
            getRequestHandlers = new(db);
            postRequestHandlers = new(db);
            routes = new();
            addRoutes();
        }

        public void Start() {
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Listening for incoming connections on port {0}...", port);

            // Accept incoming connections in a loop
            while (true) {
                var client = listener.AcceptTcpClient();
                Console.WriteLine("Received incoming connection from {0}", client.Client.RemoteEndPoint);
                // Start a new thread to handle the request
                var t = new Thread(() => ProcessRequest(client));
                t.Start();
            }
        }

        private void ProcessRequest(TcpClient client) {
            byte[] requestData = new byte[4096];
            int bytesRead = client.GetStream().Read(requestData, 0, requestData.Length);
            string requestString = Encoding.UTF8.GetString(requestData, 0, bytesRead);

            Console.WriteLine(requestString);
            HttpRequest request;
            try {
                request = parseRequest(requestString);
            } catch (Exception e) {
                string errMsg = "malformed request";
                RequestHandler.writeErr(client, HttpStatusCode.BadRequest, errMsg);

                return;
            }

            if (request.headers.TryGetValue("Authorization", out string token)) {

            } else if (request.method != "POST" || request.resource != "/login") {
                string errMsg = "unauthorized access";
                RequestHandler.writeErr(client, HttpStatusCode.Unauthorized, errMsg);
                return;
            }

            if (routes.TryGetValue(request.method, out var methodSpecificRoutes)) {
                if (methodSpecificRoutes.TryGetValue(request.resource, out var handler)) {
                    handler(client, request.body);
                } else {
                    string errMsg = "resource not found";
                    RequestHandler.writeErr(client, HttpStatusCode.NotFound, errMsg);
                    return;
                }
            } else {
                string errMsg = "resource not found";
                RequestHandler.writeErr(client, HttpStatusCode.NotFound, errMsg);
                return;
            }
        }

        private HttpRequest parseRequest(string request) {
            HttpRequest httpRequest = new();
            // Parse the request to determine the HTTP method and requested resource
            var requestLines = request.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            // Parse the request line
            string[] requestLine = requestLines[0].Split(' ');
            httpRequest.method = requestLine[0];
            httpRequest.resource = requestLine[1];
            httpRequest.httpVersion = requestLine[2];

            var headers = new Dictionary<string, string>();
            int i;
            for (i = 0; i < requestLines.Length; i++) {
                string line = requestLines[i];
                if (line == "") {
                    i++;
                    break;
                }

                string[] headerParts = line.Split(':');
                if (headerParts.Length == 2) {
                    headers[headerParts[0]] = headerParts[1];
                }
            }

            httpRequest.headers = headers;

            string body = string.Join("", requestLines.Skip(i).Take(requestLines.Length - i));

            try {
                httpRequest.body = JsonConvert.DeserializeObject<dynamic>(body) ?? new JObject();
            } catch (Exception e) {
                throw;
            }
            
            return httpRequest;
        }
    }

    internal class HttpRequest {
        public string method { get; set; }
        public string resource { get; set; }
        public string httpVersion { get; set; }
        public Dictionary<string, string> headers { get; set; }
        public JObject body { get; set; }
    }
}
