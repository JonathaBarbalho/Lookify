namespace LookifyConsoleTester;

internal sealed class Tester(
    TestScenarios scenarios) {

    private readonly TestScenarios _scenarios = scenarios;

    public async Task RunAsync()
    {
        while (true) {
            Console.WriteLine("----------------------");
            Console.WriteLine("Lookify Test Scenarios");
            Console.WriteLine("----------------------");
            Console.WriteLine("1. Test CEP Lookup");
            Console.WriteLine("2. Test CNPJ Lookup");
            Console.WriteLine("3. Test Vehicle Plate Lookup");
            Console.WriteLine("4. Test FIPE Lookup");
            Console.WriteLine("5. Test IBGE Lookup");
            Console.WriteLine("6. Test Bank Lookup");
            Console.WriteLine("7. Exit");
            Console.WriteLine();
            Console.Write("Select an option: ");

            var option = Console.ReadLine();

            switch (option) {
                case "1":
                    await _scenarios.TestCepLookup();
                    break;
                case "2":
                    await _scenarios.TestCnpjLookup();
                    break;
                case "3":
                    await _scenarios.TestVehiclePlateLookup();
                    break;
                case "4":
                    await _scenarios.TestFipeLookup();
                    break;
                case "5":
                    await _scenarios.TestIbgeLookup();
                    break;
                case "6":
                    await _scenarios.TestBankLookup();
                    break;
                case "7":
                    Environment.Exit(0);
                    continue;
                default:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Invalid option. Please try again.");
                    Console.ForegroundColor = ConsoleColor.White;
                    continue;
            }
            Console.WriteLine();
        }
    }
}