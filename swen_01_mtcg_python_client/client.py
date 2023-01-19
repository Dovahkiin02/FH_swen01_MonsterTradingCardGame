from Connection import Connection

from prompt_toolkit import PromptSession
from prompt_toolkit.completion import WordCompleter
import os


def exitCommand():
    quit()

def clear():
    os.system('cls' if os.name == 'nt' else 'clear')

def main():
    conn = Connection()
    commands = {
        "login": conn.login,
        "createCard": conn.createCard,
        "createUser": conn.createUser,
        "showStack": conn.getStack,
        "showDeck": conn.getDeck,
        "showUser": conn.getUser,
        "showOffers": conn.getOffers,
        "showScoreboard": conn.getScoreboard,
        "buyPackage": conn.buyPackage,
        "setDeck": conn.setDeck,
        "battle": conn.battle,
        "offerCard": conn.offerCard,
        "buyCard": conn.buyCard,
        "logout": conn.logout,
        "exit": exitCommand,
        "quit": exitCommand,
        "clear": clear,
        "cls": clear
    }

    session = PromptSession()
    command_completer = WordCompleter(list(commands), ignore_case=False, match_middle=False)
    while True:
        userInput = session.prompt("> ", completer=command_completer, complete_in_thread=True)
        if userInput in commands:
            commands[userInput]()
        else:
            print("'{}' not recognised as a command".format(userInput))
        print()

main()
