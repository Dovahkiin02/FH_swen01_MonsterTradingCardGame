import requests
import json
import random

class IntegrationTest:
    def __init__(self):
        self.url = "http://127.0.0.1:9999"
        self.headersDict = dict()
        self.cardListDict = dict()
        self.openCardsDict = dict()


    def login_admin(self):
        body = {
            "name": "admin",
            "password": "asdf"
        }
        # try:
        #     response = requests.post(self.url + "/login", data=json.dumps(body))
        #     if response.status_code == requests.codes.ok:
        #         self.headersDict["admin"] = dict(Authorization = response.json()["Token"])
        #         print("login successfully!")
        #     else:
        #         print("login failed.")
        # except:
        #     print("couldn't connect to server")
        response = requests.post(self.url + "/login", data=json.dumps(body))
        if response.status_code == requests.codes.ok:
            self.headersDict["admin"] = dict(Authorization = response.json()["Token"])
            print("login successfully!")
        else:
            print("login failed")

    def createCards(self, numberOfCards):
        types = {
            "Spell": 1,
            "Goblin": 2,
            "Ork": 3,
            "Dragon": 4,
            "Wizzard": 5,
            "Knight": 6,
            "Kraken": 7,
            "Elf": 8
        }

        elements = {
            "Normal": 1,
            "Fire": 2,
            "Water": 3,
            "Grass": 4
        }
        counter = 0
        for i in range(numberOfCards):
            typeIndex = random.randint(0, 7)
            elementIndex = random.randint(0, 3)
            damage = random.randint(1, 10)

            body = {
                "name": list(elements)[elementIndex] + list(types)[typeIndex],
                "element": list(elements.values())[elementIndex],
                "damage": damage,
                "type": list(types.values())[typeIndex]
            }

            response = requests.post(self.url + "/card", data=json.dumps(body), headers=self.headersDict["admin"])
            if response.status_code == requests.codes.created:
                counter += 1

        if counter == numberOfCards:
            print("cards created successfully")
            return True
        else:
            print("card creation failed")
            return False

    def createUser(self, name, pw):
        body = {
            "name": name,
            "password": pw,
            "role": 1
        }
        response = requests.post(self.url + "/user", data=json.dumps(body), headers=self.headersDict["admin"])
        if response.status_code == requests.codes.created:
            print("{} created successfully!".format(name))
        elif response.status_code == requests.codes.server_error:
            print("Internal Server error")
        else:
            print("--> error: " + response.json()["message"])

    def loginUser(self, name, pw):
        body = {
            "name": name,
            "password": pw
        } 

        response = requests.post(self.url + "/login", data=json.dumps(body), headers=self.headersDict["admin"])
        if response.status_code == requests.codes.ok:
            self.headersDict[name] = dict(Authorization = response.json()["Token"])
            print("login {} successful!".format(name))
            return True
        elif response.status_code == requests.codes.server_error:
            print("Internal Server error")
        else:
            print("--> error: " + response.json()["message"])


        return False

    def buyPackage(self, name):
        response = requests.post(self.url + "/transactions/package", headers=self.headersDict[name])
        if response.ok:
            print(json.dumps(response.json(), indent=2))
        else:
            print("--> error: " + response.json()["message"])

    def getStack(self, name):
        response = requests.get(self.url + "/stack", headers=self.headersDict[name])
        if response.ok:
            resJson = response.json()
            cardList = list()
            openCards = list()
            for entry in resJson[:4]:
                cardList.append(entry["id"])
            for entry in resJson[4:]:
                openCards.append(entry["id"])

            self.openCardsDict[name] = openCards
            self.cardListDict[name] = cardList
            print(json.dumps(resJson, indent=2))
        else:
            print("--> error: " + response.json()["message"])

    def getDeck(self, name):
        response = requests.get(self.url + "/deck", headers=self.headersDict[name])
        if response.ok:
            print(json.dumps(response.json(), indent=2))
        else:
            print("--> error: " + response.json()["message"])

    def setDeck(self, name, cardList):
        body = dict(cards = cardList)
        response = requests.post(self.url + "/deck", data=json.dumps(body), headers=self.headersDict[name])
        if response.ok:
            print(response.json()["message"])
        else:
            print("--> error: " + response.json()["message"])

    def battle(self, name):
        response = requests.post(self.url + "/game", headers=self.headersDict[name])
        if response.ok:
            resJson = response.json()
            if resJson["state"] == "FINISHED":
                counter = 0
                lines = resJson["protocol"].split("\r\n")
                for line in lines[:-2]:
                    print("{}: {}".format(counter, line))
                    counter += 1
                print(lines[-2])
            else:
                print(json.dumps(resJson["state"], indent=2))
        else:
            print("--> error: " + response.json()["message"])

    def getUser(self, name):
        response = requests.get(self.url + "/user", headers=self.headersDict[name])
        if response.ok:
            print(json.dumps(response.json(), indent=2))
        else:
            print("--> error: " + response.json()["message"])

    def getScoreboard(self, name):
        response = requests.get(self.url + "/scoreboard", headers=self.headersDict[name])
        if response.ok:
            # resJson = response.json()
            # for user in resJson:
            #     print("rank {}: {}".format(resJson["rank"], resJson["user"]))
            print(json.dumps(response.json(), indent=2))
        else:
            print("--> error: " + response.json()["message"])

    def getOffers(self, name):
        response = requests.get(self.url + "/store", headers=self.headersDict[name])
        if response.ok:
            print(json.dumps(response.json(), indent=2))
        else:
            print("--> error: " + response.json()["message"])

    def offerTrade(self, name, card, price):
        body = {
            "id": card,
            "price": price
        }
        response = requests.post(self.url + "/transactions/addOffer", data=json.dumps(body), headers=self.headersDict[name])
        if response.ok:
            print(response.json()["message"])
        else:
            print("--> error: " + response.json()["message"])

    def acceptTrade(self, name, card):
        body = {
            "id": card
        }
        response = requests.post(self.url + "/transactions/buyOffer", data=json.dumps(body), headers=self.headersDict[name])
        if response.ok:
            print(response.json()["message"])
        else:
            print("--> error: " + response.json()["message"])


test = IntegrationTest()
print("1) login admin")
test.login_admin()
print()
print("2) create cards")
if not test.createCards(15):
    exit()
print()

print("3) create users")
test.createUser("user1", "asdf")
test.createUser("user2", "asdf")
print()

print("4) login users")
if not test.loginUser("user1", "asdf"):
    exit()

if not test.loginUser("user2", "asdf"):
    exit()
print()

print("5) buy package user1")
test.buyPackage("user1")
print()

print("6) buy package user2")
test.buyPackage("user2")
print()

print("7) get stack of user1")
test.getStack("user1")
print()

print("8) get stack of user2")
test.getStack("user2")
print()

print("9) get deck of user1")
test.getDeck("user1")
print()

print("10) get deck of user2")
test.getDeck("user2")
print()

print("11) configure deck of user1")
test.setDeck("user1", test.cardListDict["user1"])
print()

print("12) configure deck of user2")
test.setDeck("user2", test.cardListDict["user2"])
print()

print("13) get deck of user1")
test.getDeck("user1")
print()

print("14) get deck of user2")
test.getDeck("user2")
print()

print("15) battle user1")
test.battle("user1")
print()

print("16) battle user2")
test.battle("user2")
print()

print("16) get user1")
test.getUser("user1")
print()

print("17) get user2")
test.getUser("user2")
print()

print("18) scoreboard")
test.getScoreboard("user1")
print()

print("19) user1 offer trade")
test.offerTrade("user1", test.openCardsDict["user1"][0], 4)
print()

print("20) show store offers")
test.getOffers("user2")
print()

print("20) user2 accept trade")
test.acceptTrade("user2", test.openCardsDict["user1"][0])
print()

print("22) show store offers")
test.getOffers("user2")
print()

print("23) show stack user2")
test.getStack("user2")
print()