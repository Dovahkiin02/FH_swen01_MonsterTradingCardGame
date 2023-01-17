from Connection import Connection

from prompt_toolkit import PromptSession
from prompt_toolkit.completion import WordCompleter

def exitCommand():
    quit()

def hello():
    print("hello World!")


def main():
    conn = Connection()
    commands = {
        "login": conn.login,
        "createUser": conn.createUser,
        "getStack": conn.getStack,
        "getDeck": conn.getDeck,
        "getUser": conn.getUser,
        "logout": conn.logout,
        "exit": exitCommand,
        "quit": exitCommand
    }

    session = PromptSession()
    command_completer = WordCompleter(list(commands), ignore_case=False, match_middle=False)
    while True:
        user_input = session.prompt("> ", completer=command_completer, complete_in_thread=True)
        if user_input in commands:
            commands[user_input]()
        else:
            print("'{}' not recognised as a command".format(user_input))

main()
