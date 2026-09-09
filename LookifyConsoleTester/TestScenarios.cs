using Lookify;

namespace LookifyConsoleTester;

internal sealed class TestScenarios(
    LookifyService lookifyService) {
    private readonly LookifyService _lookifyService = lookifyService;
    public async Task TestCepLookup()
    {
        Console.WriteLine("Testing CEP Lookup...");

        Console.Write("Enter a CEP (zip code): ");
        var zipCode = Console.ReadLine();

        try {
            if (string.IsNullOrWhiteSpace(zipCode)) {
                Console.WriteLine("Invalid CEP format.");
                return;
            }

            var result = await _lookifyService.Cep.ConsultAsync(zipCode);

            Console.WriteLine($"CEP: {result.ZipCode}");
            Console.WriteLine($"Street: {result.Street}");
            Console.WriteLine($"Neighborhood: {result.Neighborhood}");
            Console.WriteLine($"City: {result.City}");
            Console.WriteLine($"State: {result.State}");
        }
        catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}