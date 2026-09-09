namespace LookifyConsoleTester;

internal sealed class Tester(
    TestScenarios scenarios) {

    private readonly TestScenarios _scenarios = scenarios;

    public async Task RunAsync()
    {
        while (true) {
            Console.WriteLine("Lookify Test Scenarios");
            Console.WriteLine("----------------------");
            Console.WriteLine("1. Test CEP Lookup");
            Console.WriteLine("2. Exit");
            Console.WriteLine();
            Console.Write("Select an option: ");

            var option = Console.ReadLine();
        
            switch (option) {
                case "1":
                    await _scenarios.TestCepLookup();
                    break;
                case "2":
                    Environment.Exit(0);
                    continue;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    continue;
            }
        }
    }
}