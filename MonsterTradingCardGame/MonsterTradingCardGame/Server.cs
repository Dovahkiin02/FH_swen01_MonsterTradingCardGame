using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MonsterTradingCardGame {
    internal class Server {
        private readonly Dictionary<string, Dictionary<string, Action<TcpClient, JObject, User>>> routes = new();
        private readonly int port;
        private readonly Database db;
        private readonly GetRequestHandlers getRequestHandlers;
        private readonly PostRequestHandler postRequestHandlers;

        private void addRoutes() {
            routes.Add("GET", new Dictionary<string, Action<TcpClient, JObject, User>> {
                ["/stack"] = getRequestHandlers.getStack,
                ["/deck"] = getRequestHandlers.getDeck
            });
            routes.Add("POST", new Dictionary<string, Action<TcpClient, JObject, User>> {
                ["/login"] = postRequestHandlers.login,
                ["/user"] = postRequestHandlers.addUser,
                ["/card"] = postRequestHandlers.addCard,
                ["/deck"] = postRequestHandlers.updateDeck,
                ["/game"] = postRequestHandlers.startGame,
                ["/transactions/package"] = postRequestHandlers.buyPackage
            });
        }

        public Server(int port, Database db) {
            this.port = port;
            this.db = db;
            getRequestHandlers = new(db);
            postRequestHandlers = new(db, new GameHandler(db));
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
            //RequestHandler.writeUnauthorizedErr(client);
            byte[] requestData = new byte[4096];
            int bytesRead = client.GetStream().Read(requestData, 0, requestData.Length);
            string requestString = Encoding.UTF8.GetString(requestData, 0, bytesRead);
            string errMsg;

            
            HttpRequest request;
            try {
                request = parseRequest(requestString);
            } catch (Exception e) {
                errMsg = "malformed request";
                RequestHandler.writeErr(client, HttpStatusCode.BadRequest, errMsg);

                return;
            }
            User? currentUser = null;
            if (request.headers.TryGetValue("Authorization", out string? token)) {
                Guid? userId = JwtHandler.validateJwt(token);
                if (userId == null) {
                    RequestHandler.writeUnauthorizedErr(client);
                    return;
                }
                currentUser = db.getUser(userId.Value);
                if (currentUser == null) {
                    RequestHandler.writeUnauthorizedErr(client);
                    return;
                }
                //currentUser = new User(Guid.NewGuid(), "asdf", Role.ADMIN, 20);
            } else if (request.method != "POST" || request.resource != "/login") {
                RequestHandler.writeUnauthorizedErr(client);
                return;
            }

            if (routes.TryGetValue(request.method, out var methodSpecificRoutes)) {
                if (methodSpecificRoutes.TryGetValue(request.resource, out var handler)) {
                    handler(client, request.body, currentUser);
                } else {
                    errMsg = "resource not found";
                    RequestHandler.writeErr(client, HttpStatusCode.NotFound, errMsg);
                    return;
                }
            } else {
                errMsg = "resource not found";
                RequestHandler.writeErr(client, HttpStatusCode.NotFound, errMsg);
                return;
            }
        }

        public HttpRequest parseRequest(string request) {
            HttpRequest httpRequest = new();
            // Parse the request to determine the HTTP method and requested resource
            var requestLines = request.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            // Parse the request line
            string[] requestLine = requestLines[0].Split(' ');
            httpRequest.method = requestLine[0];
            string[] parameters = requestLine[1].Split('?');
            httpRequest.resource = parameters[0];
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
                    headers[headerParts[0]] = headerParts[1].Trim();
                }
            }

            httpRequest.headers = headers;

            string body = string.Join("", requestLines.Skip(i).Take(requestLines.Length - i));
            JObject jbody = JsonConvert.DeserializeObject<dynamic>(body) ?? new JObject();
            if (parameters.Length > 1) {
                if (parameters.Length > 2) {
                    throw new Exception("malformed parameters");
                }
                jbody["parameter"] = parseParameters(parameters[1]);
            }
            try {
                httpRequest.body = jbody; 
            } catch (Exception e) {
                throw;
            }
            
            return httpRequest;
        }

        private JObject parseParameters(string parameter) {
            string[] queryParamsArray = parameter.Split('&');

            JObject queryParams = new ();
            foreach (string param in queryParamsArray) {
                string[] keyValue = param.Split('=');
                queryParams.Add(keyValue[0], keyValue[1]);
            }

            return queryParams;
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
