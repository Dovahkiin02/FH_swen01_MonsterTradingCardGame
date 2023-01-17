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
        response = requests.post(self.url + "/login", data=json.dumps(body))
        if response.status_code == requests.codes.ok:
            self.token = response.json()["Token"]
            self.headers = dict(Authorization = self.token)
            print("  login successfully!")
        else:
            print("  login failed.")

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
        command_completer = WordCompleter(list(roles), ignore_case=True, match_middle=False)

        name = prompt("username: ")
        password = prompt("password: ", is_password=True)
        role = prompt("role: ", completer=command_completer, complete_in_thread=True)
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
        if response.status_code == requests.codes.created:
            print("  User created successfully!")
        elif response.status_code == requests.codes.server_error:
            print("  Internal Server error")
        else:
            print("error: " + response.json()["message"])

    def buyPackage(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.post(self.url + "/transactions/package", headers=self.headers)
        if response.status_code == requests.codes.ok:
            print("  ok")
        else:
            print("error: " + response.json()["message"])

    def getStack(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.get(self.url + "/stack", headers=self.headers)
        if response.status_code == requests.codes.ok:
            print("  ok")
        else:
            print("error: " + response.json()["message"])

    def getDeck(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.get(self.url + "/deck", headers=self.headers)
        if response.status_code == requests.codes.ok:
            print("  ok")
        else:
            print("error: " + response.json()["message"])

    def getUser(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.get(self.url + "/user", headers=self.headers)
        if response.status_code == requests.codes.ok:
            print("  ok")
        else:
            print("error: " + response.json()["message"])

    def startGame(self):
        if not hasattr(self, "token"):
            print("  not logged in.")
            return

        response = requests.get(self.url + "/game", headers=self.headers)
        if response.status_code == requests.codes.ok:
            print("  ok")
        else:
            print("error: " + response.json()["message"])