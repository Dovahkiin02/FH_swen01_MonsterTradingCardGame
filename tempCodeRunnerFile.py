("3) create users")
test.createUser("user1", "asdf")
test.createUser("user2", "asdf")

print("4) login users")
if not test.loginUser("user1", "asdf"):
    exit()

if not test.loginUser("user2", "asdf"):
    exit()

print("5) buy package user1")

