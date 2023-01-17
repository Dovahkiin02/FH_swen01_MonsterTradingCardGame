# Monster Trading Card Game 
## Design
The first step for me when I started designing this project was making an ERD for the database. That way I'm able to get a very fast understanding of the scope of the work. Here the ERD of my Database:
![ERD image](imgs/FH_swen01_MonsterTradingCardGame.png)

After designing and building the database, I built the methods in c# that will access the database.
Next I built a basic Server that can listen to and parse requests.
After that all that was missing were routes that the client could send requests to, which then implement the real functionality of the server and therefor connect the server with the interface Class to the Database.
Additionally I implemented a few helper Classes which help with readabilty and maintainability of the code.
## Solutions
### Routes
I opted to outsource the routes into different classes and then use delegates to call them via a dictionary. Here is how that looks in code:
```cs
private void addRoutes() {
    routes.Add("GET", new Dictionary<string, Action<TcpClient, JObject, User>> {
        ["/stack"] = getRequestHandlers.getStack,
        ["/deck"] = getRequestHandlers.getDeck,
        ["/user"] = getRequestHandlers.getUser,
        ["/store"] = getRequestHandlers.getStoreOffers,
        ["/scoreboard"] = getRequestHandlers.getScoreboard
    });
    routes.Add("POST", new Dictionary<string, Action<TcpClient, JObject, User>> {
        ["/login"] = postRequestHandlers.login,
        ["/user"] = postRequestHandlers.addUser,
        ["/card"] = postRequestHandlers.addCard,
        ["/deck"] = postRequestHandlers.updateDeck,
        ["/game"] = postRequestHandlers.startGame,
        ["/transactions/package"] = postRequestHandlers.buyPackage,
        ["/transactions/addOffer"] = postRequestHandlers.addOfferToStore,
        ["/transactions/buyOffer"] = postRequestHandlers.buyOfferFromStore
    });
    routes.Add("PUT", new Dictionary<string, Action<TcpClient, JObject, User>> {
        ["/user"] = putRequestHandler.updateUser
    });
}
```
Every route is an entry into a dictionary according to the method and for every method there is a seperate utility class to handle it. And this is how the delegate works:
```cs
if (routes.TryGetValue(request.method, out var methodSpecificRoutes)) {
    if (methodSpecificRoutes.TryGetValue(request.resource, out var handler)) {
        handler(client, request.body, currentUser); // call the method from the dict
    } else {
        errMsg = "resource not found";
        RequestHandler.writeStructuredResponse(client, HttpStatusCode.NotFound, errMsg);
        return;
    }
} else {
    errMsg = "resource not found";
    RequestHandler.writeStructuredResponse(client, HttpStatusCode.NotFound, errMsg);
    return;
}
```
### Tests
I used the Framework NUnit for testing and the framework Moq to mock dependencies. Here is an example of Moq from the ``TestMethod`` gameTest_02():
```cs
// simulating two random users
Guid user1 = Guid.NewGuid();
Guid user2 = Guid.NewGuid();

// creating the mocked object
Mock<Database> mockedDB = new();
// defining the behaviour of the member function getDeck(Guid)
mockedDB.Setup(x => x.getDeck(It.IsAny<Guid>())).Returns((Guid id) => {
    List<Tuple<int, Card>>? deck = new();
    if (id == user1) {
        for (int j = 0; j < 4; j++)
            deck.Add(Tuple.Create<int, Card>(j, new(1, "NormalKraken", Element.NORMAL, 12, Type.KRAKEN)));
        return deck;
    } else {
        for (int j = 0; j < 4; j++)
            deck.Add(Tuple.Create<int, Card>(j, new(1, "FireSpell", Element.FIRE, 12, Type.SPELL)));
        return deck;
    }
});
```

## Git
https://github.com/Dovahkiin02/FH_swen01_MonsterTradingCardGame
