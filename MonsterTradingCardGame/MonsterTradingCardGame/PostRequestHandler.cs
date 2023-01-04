using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;

namespace MonsterTradingCardGame {
    internal class PostRequestHandler : RequestHandler {
        private GameHandler gameHandler;
        public PostRequestHandler(Database db, GameHandler gameHandler) : base(db) {
            this.gameHandler = gameHandler;
        }
        private const int defaultPlayerCoins = 20;
        private const int defaultPackageCost = 5;
        private const int deckSize = 4;

        public void login(TcpClient client, JObject body, User _user) {
            if (!checkKeys(body, new[] { "username", "password" })) {
                writeMalformedBodyErr(client);
                return;
            }
            if (!db.isConnected()) {
                string errMsg = "unexpected internal error";
                writeErr(client, HttpStatusCode.InternalServerError, errMsg);
                return;
            }
            Guid? userId = db.verifyUser(body["username"].ToString(), body["password"].ToString());
            if (userId != null) {
                JObject response = new() {
                    ["Token"] = JwtHandler.getJwt(userId.Value)
                };
                writeResponse(client, HttpStatusCode.OK, response);
                return;
            } else {
                writeUnauthorizedErr(client);
                return;
            }
        }

        public void addUser(TcpClient client, JObject body, User user) {
            if (!checkKeys(body, new[] { "username", "password", "role" })) {
                writeMalformedBodyErr(client);
                return;
            }
            if (user.role != Role.ADMIN) {
                writeForbiddenErr(client);
                return;
            }
            try {
                bool result = db.addUser(
                        body["username"].ToString(),
                        body["password"].ToString(),
                        defaultPlayerCoins,
                        Enum.Parse<Role>(body["role"].ToString())
                        );

                if (!result) {
                    string msg = "unexpected error while creating new user";
                    writeErr(client, HttpStatusCode.InternalServerError, msg);
                    return;
                } else {
                    writeResponse(client, HttpStatusCode.Created, null);
                    return;
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                writeMalformedBodyErr(client);
                return;
            }
            
        }

        public void addCard(TcpClient client, JObject body, User user) {
            if (!checkKeys(body, new[] { "name", "element", "damage", "type" })) {
                writeMalformedBodyErr(client);
                return;
            }
            if (user.role != Role.ADMIN) {
                writeForbiddenErr(client);
                return;
            }

            try {
                bool result = db.addCard(
                    body["name"].ToString(), 
                    Enum.Parse<Element>(body["element"].ToString()), 
                    int.Parse(body["element"].ToString()),
                    Enum.Parse<Type>(body["type"].ToString())
                    );
                if (!result) {
                    string msg = "unexpected error while creating new card";
                    writeErr(client, HttpStatusCode.InternalServerError, msg);
                    return;
                } else {
                    writeResponse(client, HttpStatusCode.Created, null);
                    return;
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                writeMalformedBodyErr(client);
                return;
            }
        }

        public void buyPackage(TcpClient client, JObject body, User user) {
            if (user.coins < defaultPackageCost) {
                string msg = "not enough coins to buy package";
                writeErr(client, HttpStatusCode.BadRequest, msg);
                return;
            }

            List<Card>? package = db.buyPackage(user.id, defaultPackageCost);
            if (package == null) {
                string msg = "unexpected error while trying to get package";
                writeErr(client, HttpStatusCode.InternalServerError, msg);
                return;
            }

            writeResponse(client, HttpStatusCode.OK, package);
        }

        public void updateDeck(TcpClient client, JObject body, User user) {
            if (!checkKeys(body, new[] { "cards" })) {
                writeMalformedBodyErr(client);
                return;
            }
            List<int> cards = body["cards"].Select(e => int.Parse((string)e)).ToList();
            if (cards.Count != deckSize) {
                string msg = $"wrong number of cards provided.\nA deck may only contain {deckSize} number of cards";
                writeErr(client, HttpStatusCode.BadRequest, msg);
                return;
            }

            if (!db.checkCardsInStack(user.id, cards)) {
                string msg = "provided cards can't be used in deck";
                writeErr(client, HttpStatusCode.BadRequest, msg);
                return;
            }

            if (!db.addCardsToDeck(user.id, cards)) {
                string msg = "unexpected error while adding cards to deck";
                writeErr(client, HttpStatusCode.InternalServerError, msg);
                return;
            }

            writeResponse(client, HttpStatusCode.OK, null);
        }

        public void startGame(TcpClient client, JObject body, User user) {
            List<Tuple<int, Card>>? deck = db.getDeck(user.id);
            if (deck == null || deck.Count != deckSize) {
                string msg = "no valid deck defined";
                writeErr(client, HttpStatusCode.BadRequest, msg);
                return;
            }
            JObject response = new();
            Status? status = gameHandler.getStatus(user.id);
            if (status == null) {
                status = gameHandler.joinGame(user.id);
            }

            if (status == Status.WAITING) {
                response["status"] = Enum.GetName(typeof(Status), status);
                writeResponse(client, HttpStatusCode.Accepted, response);
                return;
            } else if (status == Status.FAILED) {
                string msg = "unexpected error while finishing game";
                writeResponse(client, HttpStatusCode.InternalServerError, msg);
                return;
            } else { 
                Thread.Sleep(400);
                status = gameHandler.getStatus(user.id);
                if (status == Status.FINISHED) {
                    string? protocol = gameHandler.getProtocol(user.id);
                    if (protocol == null) {
                        string msg = "unexpected error while finishing game";
                        writeErr(client, HttpStatusCode.InternalServerError, msg);
                        return;
                    }
                    response["status"] = Enum.GetName(typeof(Status), status);
                    response["protocol"] = protocol;
                    writeResponse(client, HttpStatusCode.OK, response);
                    return;
                } else if (status == Status.FAILED) {
                    string msg = "game failed";
                    writeErr(client, HttpStatusCode.InternalServerError, msg);
                    return;
                } else {
                    response["status"] = Enum.GetName(typeof(Status), status);
                    writeResponse(client, HttpStatusCode.Accepted, response);
                    return;
                }
            }
        }
    }
}
