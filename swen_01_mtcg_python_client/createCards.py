import random 

def main():
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
    for i in range(15):
        typeIndex = random.randint(0, 7)
        elementIndex = random.randint(0, 3)
        damage = random.randint(1, 10)


        print(",   (default, '{}', {}, {}, {})".format(list(elements)[elementIndex] + list(types)[typeIndex], list(elements.values())[elementIndex], damage, list(types.values())[typeIndex]))


main()