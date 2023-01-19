import requests
import json
from prompt_toolkit import prompt
from prompt_toolkit.completion import WordCompleter

class Connection:
    def __init__(self):
        self.url = "http://127.0.0.1:9999"

    def test(self):
        r = requests.get(self.url)
        print(json.dumps(r.json(), indent = 2))

    def login(self):
        name = prompt("username: ")
        password = prompt("password: ", is_password=True)
        print()
        body = {
            "name": name,
            "password": password
        }
        try:
            response = requests.post(self.url + "/login", data=json.dumps(body))
            if response.ok:
                self.token = response.json()["Token"]
                self.headers = dict(Authorization = self.token)
                print("  login successful!")
            else:
                print("  login failed.")
        except:
            print("connection refused")
            exit()

    def logout(self):
        self.__dict__.pop("token", None)

        print()
        print("  logged out successfully.")

    def createUser(self):
        if not hasattr(self, 'token'):
            print("  not logged in.")
            return

        roles = {
            "admin": 0,
            "player": 1
        }
        commandCompleter = WordCompleter(list(roles), ignore_case=False, match_middle=False)

        name = prompt("username: ")
        password = prompt("password: ", is_password=True)
        role = prompt("role: ", completer=commandCompleter, complete_in_thread=True)
        if role not in roles:
            print("'{}' is not recognised as a valid role.".format(role))
            return

        print()

        body = {
            "name": name,
            "password": password,
            "role": roles[role]
        }

        response = requests.post(self.url + "/user", data=json.dumps(body), headers=self.headers)
        if response.ok:
            print(response.json()["message"])
        else:
            print("error: " + response.json()["message"])

    def createCard(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        elements = {
            "normal": 1,
            "fire": 2,
            "water": 3,
            "grass": 4
        }
        commandCompleterElements = WordCompleter(list(elements), ignore_case=False, match_middle=False)

        types = {
            "spell": 1,
            "goblin": 2,
            "ork": 3,
            "dragon": 4,
            "wizzard": 5,
            "knight": 6,
            "kraken": 7,
            "elf": 8
        }
        commandCompleterTypes = WordCompleter(list(types), ignore_case=False, match_middle=False)

        name = prompt("name: ")
        element = prompt("element: ", completer=commandCompleterElements, complete_in_thread=True)
        if element not in elements:
            print("'{}' is not recognised as a valid element.".format(element))
            return
        damage = prompt("damage: ")
        cardType = prompt("type: ", completer=commandCompleterTypes, complete_in_thread=True)
        if cardType not in types:
            print("'{}' is not recognised as a valid type.".format(cardType))
            return
        
        body = {
            "name": name,
            "element": elements[element],
            "damage": int(damage),
            "type": types[cardType]
        }

        response = requests.post(self.url + "/card", data=json.dumps(body), headers=self.headers)
        if response.ok:
            print(response.json()["message"])
        else:
            print("error: " + response.json()["message"])

    def buyPackage(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.post(self.url + "/transactions/package", headers=self.headers)
        if response.ok:
            print(json.dumps(response.json(), indent=2))
        else:
            print("error: " + response.json()["message"])

    def getStack(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.get(self.url + "/stack", headers=self.headers)
        if response.ok:
            print(json.dumps(response.json(), indent=2))
        else:
            print("error: " + response.json()["message"])

    def getDeck(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.get(self.url + "/deck", headers=self.headers)
        if response.ok:
            print(json.dumps(response.json(), indent=2))
        else:
            print("error: " + response.json()["message"])

    def getUser(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.get(self.url + "/user", headers=self.headers)
        if response.ok:
            print(json.dumps(response.json(), indent=2))
        else:
            print("error: " + response.json()["message"])

    def setDeck(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        print("define deck in format x, x, x, x")
        userInput = prompt("Deck: ")
        cards = userInput.replace(" ", "").split(",")

        response = requests.post(self.url + "/deck", data=json.dumps(dict(cards = cards)), headers=self.headers)
        if response.ok:
            print(response.json()["message"])
        else:
            print("error: " + response.json()["message"])

    def getOffers(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.get(self.url + "/store", headers=self.headers)
        if response.ok:
            print(json.dumps(response.json(), indent=2))
        else:
            print("error: " + response.json()["message"])

    def getScoreboard(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.get(self.url + "/scoreboard", headers=self.headers)
        if response.ok:
            print(json.dumps(response.json(), indent=2))
        else:
            print("error: " + response.json()["message"])

    def battle(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.post(self.url + "/game", headers=self.headers)
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
            print("error: " + response.json()["message"])

    def offerCard(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        cardId = prompt("card: ")
        price = prompt("price: ")

        body = {
            "id": cardId,
            "price": price
        }

        response = requests.post(self.url + "/transactions/addOffer", data=json.dumps(body), headers=self.headers)
        if response.ok:
            print(response.json()["message"])
        else:
            print("error: " + response.json()["message"])

    def buyCard(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        cardId = prompt("card: ")

        response = requests.post(self.url + "/transactions/buyOffer", data=json.dumps(dict(id = cardId)), headers=self.headers)
        if response.ok:
            print(response.json()["message"])
        else:
            print("error: " + response.json()["message"])
        
