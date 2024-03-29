﻿using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;

namespace MonsterTradingCardGame {
    internal class PostRequestHandler : RequestHandler {
        private GameHandler gameHandler;
        public PostRequestHandler(Database db) : base(db) {
            this.gameHandler = new (db);
        }
        private const int defaultPlayerCoins = 20;
        private const int defaultPlayerElo = 100;
        private const int defaultPackageCost = 5;
        private const int deckSize = 4;

        public void login(TcpClient client, JObject body, User _user) {
            if (!checkKeys(body, new[] { "name", "password" })) {
                writeMalformedBodyErr(client);
                return;
            }
            if (!db.isConnected()) {
                string errMsg = "unexpected internal error";
                writeStructuredResponse(client, HttpStatusCode.InternalServerError, errMsg);
                return;
            }
            Guid? userId = db.verifyUser(body["name"].ToString(), body["password"].ToString());
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
            if (!checkKeys(body, new[] { "name", "password", "role" })) {
                writeMalformedBodyErr(client);
                return;
            }
            if (user.role != Role.ADMIN) {
                writeForbiddenErr(client);
                return;
            }
            string responseMsg;
            try {
                bool result = db.addUser(
                        body["name"].ToString(),
                        body["password"].ToString(),
                        defaultPlayerCoins,
                        Enum.Parse<Role>(body["role"].ToString()),
                        defaultPlayerElo,
                        out string errMsg
                        );

                if (!result) {
                    if (errMsg == "user already exists") {
                        writeStructuredResponse(client, HttpStatusCode.Conflict, errMsg);
                        return;
                    }
                    responseMsg = "unexpected error while creating new user";
                    writeStructuredResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                    return;
                } else {
                    responseMsg = "user created successfully";
                    writeStructuredResponse(client, HttpStatusCode.Created, responseMsg);
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
                    int.Parse(body["damage"].ToString()),
                    Enum.Parse<Type>(body["type"].ToString())
                    );
                if (!result) {
                    string msg = "unexpected error while creating new card";
                    writeStructuredResponse(client, HttpStatusCode.InternalServerError, msg);
                    return;
                } else {
                    string msg = "card created successfully";
                    writeStructuredResponse(client, HttpStatusCode.Created, msg);
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
                writeStructuredResponse(client, HttpStatusCode.BadRequest, msg);
                return;
            }

            List<Card>? package = db.buyPackage(user.id, defaultPackageCost);
            if (package == null) {
                string msg = "unexpected error while trying to get package";
                writeStructuredResponse(client, HttpStatusCode.InternalServerError, msg);
                return;
            }

            writeResponse(client, HttpStatusCode.OK, package);
        }

        public void updateDeck(TcpClient client, JObject body, User user) {
            if (!checkKeys(body, new[] { "cards" })) {
                writeMalformedBodyErr(client);
                return;
            }
            List<int> cards = new();
            try {
                cards = body["cards"].Select(e => int.Parse((string)e)).ToList();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                writeMalformedBodyErr(client);
                return;
            }
            
            string responseMsg;
            if (cards.Count != deckSize) {
                responseMsg = $"wrong number of cards provided.\nA deck may only contain {deckSize} number of cards";
                writeStructuredResponse(client, HttpStatusCode.BadRequest, responseMsg);
                return;
            }

            if (!db.checkCardsInStack(user.id, cards)) {
                responseMsg = "provided cards can't be used in deck";
                writeStructuredResponse(client, HttpStatusCode.BadRequest, responseMsg);
                return;
            }

            if (!db.setDeck(user.id, cards)) {
                responseMsg = "unexpected error while adding cards to deck";
                writeStructuredResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                return;
            }

            responseMsg = "deck updated successfully";
            writeStructuredResponse(client, HttpStatusCode.OK, responseMsg);
        }

        public void startGame(TcpClient client, JObject body, User user) {
            List<Tuple<int, Card>>? deck = db.getDeck(user.id);
            string responseMsg;
            if (deck == null || deck.Count != deckSize) {
                responseMsg = "no valid deck defined";
                writeStructuredResponse(client, HttpStatusCode.BadRequest, responseMsg);
                return;
            }
            JObject response = new();
            Status? status = gameHandler.getStatus(user.id);
            if (status == null) {
                status = gameHandler.joinGame(user.id);
            }

            if (status == Status.WAITING) {
                response["state"] = Enum.GetName(typeof(Status), status);
                writeResponse(client, HttpStatusCode.Accepted, response);
                return;
            } else if (status == Status.FAILED) {
                responseMsg = "unexpected error while finishing game";
                writeResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                return;
            } else if (status == Status.FINISHED) {
                Tuple<FightResult, string, string, string>? protocol = gameHandler.getProtocol(user.id);
                if (protocol == null) {
                    responseMsg = "unexpected error while finishing game";
                    writeStructuredResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                    return;
                }
                response["state"] = Enum.GetName(typeof(Status), status);
                response["result"] = Enum.GetName(typeof(FightResult), protocol.Item1);
                response["player1"] = protocol.Item2;
                response["player2"] = protocol.Item3;
                response["protocol"] = protocol.Item4;
                writeResponse(client, HttpStatusCode.OK, response);
                return;
            } else { 
                Thread.Sleep(300);
                status = gameHandler.getStatus(user.id);
                if (status == Status.FINISHED) {
                    Tuple<FightResult, string, string, string>? protocol = gameHandler.getProtocol(user.id);
                    if (protocol == null) {
                        responseMsg = "unexpected error while finishing game";
                        writeStructuredResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                        return;
                    }
                    response["state"] = Enum.GetName(typeof(Status), status);
                    response["result"] = Enum.GetName(typeof(FightResult), protocol.Item1);
                    response["player1"] = protocol.Item2;
                    response["player2"] = protocol.Item3;
                    response["protocol"] = protocol.Item4;
                    writeResponse(client, HttpStatusCode.OK, response);
                    return;
                } else if (status == Status.FAILED) {
                    responseMsg = "game failed";
                    writeStructuredResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                    return;
                } else if (status == null) {
                    responseMsg = "unexpected error";
                    writeStructuredResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                } else {
                    response["state"] = Enum.GetName(typeof(Status), status);
                    writeResponse(client, HttpStatusCode.Accepted, response);
                    return;
                }
            }
        }

        public void addOfferToStore(TcpClient client, JObject body, User user) {
            if (!checkKeys(body, new[] { "id", "price" })) {
                writeMalformedBodyErr(client);
                return;
            }
            int stackId = int.Parse(body["id"].ToString());
            string responseMsg;
            if (!db.checkCardAvailability(user.id, stackId)) {
                responseMsg = "card isn't available in stack of user";
                writeStructuredResponse(client, HttpStatusCode.BadRequest, responseMsg);
                return;
            }
            if (db.checkDuplicateOfferInStore(stackId)) {
                responseMsg = "offer is already in store";
                writeStructuredResponse(client, HttpStatusCode.BadRequest, responseMsg);
                return;
            }
            if (!db.addOfferToStore(stackId, int.Parse(body["price"].ToString()))) {
                responseMsg = "unexpected error while adding offer to store";
                writeStructuredResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                return;
            }

            responseMsg = "offer added to store";
            writeStructuredResponse(client, HttpStatusCode.OK, responseMsg);
            return;
        }

        public void buyOfferFromStore(TcpClient client, JObject body, User user) {
            if (!checkKeys(body, new[] {"id"})) {
                writeMalformedBodyErr(client);
                return;
            }
            int stackId = int.Parse(body["id"].ToString());
            string responseMsg;
            if (!db.checkOffer(user.id, stackId)) {
                responseMsg = "offer not available for this user";
                writeStructuredResponse(client, HttpStatusCode.BadRequest, responseMsg);
                return;
            }

            if (user.coins < db.getPriceOfOffer(stackId)) {
                responseMsg = "user doesn't have enough coins to buy this offer";
                writeStructuredResponse(client, HttpStatusCode.BadRequest, responseMsg);
                return;
            }
            
            if (!db.buyCardFromStore(user.id, stackId)) {
                responseMsg = "unexpected error while buying offer from store";
                writeStructuredResponse(client, HttpStatusCode.InternalServerError, responseMsg);
                return;
            }

            responseMsg = "offer bought from store";
            writeStructuredResponse(client, HttpStatusCode.OK, responseMsg);
            return;
        }
    }
}
