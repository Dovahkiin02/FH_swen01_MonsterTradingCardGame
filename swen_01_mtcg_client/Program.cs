while (true) {
    string command = Console.ReadLine();
    switch (command) {
        case "help":
            Console.WriteLine("Available commands: help, exit");
            break;
        case "exit":
            return;
        default:
            Console.WriteLine("Unknown command. Type 'help' for a list of commands.");
            break;
    }
}