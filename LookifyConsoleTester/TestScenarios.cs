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
                Console.WriteLine("No CEP was provided.");
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

    public async Task TestCnpjLookup()
    {
        Console.WriteLine("Testing CNPJ Lookup...");

        Console.Write("Enter a CNPJ: ");
        var cnpj = Console.ReadLine();

        try {
            if (string.IsNullOrWhiteSpace(cnpj)) {
                Console.WriteLine("No CNPJ was provided.");
                return;
            }

            var result = await _lookifyService.Cnpj.ConsultAsync(cnpj);

            Console.WriteLine($"CNPJ: {result.Cnpj}");
            Console.WriteLine($"Company Name: {result.CompanyName}");
            Console.WriteLine($"Trade Name: {result.TradeName}");
            Console.WriteLine($"Registration Status: {result.RegistrationStatusDescription}");
            Console.WriteLine($"City: {result.City}");
            Console.WriteLine($"State: {result.State}");
        }
        catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task TestVehiclePlateLookup()
    {
        Console.WriteLine("Testing Vehicle Plate Lookup...");

        Console.Write("Enter a plate: ");
        var plate = Console.ReadLine();

        try {
            if (string.IsNullOrWhiteSpace(plate)) {
                Console.WriteLine("No plate was provided.");
                return;
            }

            var result = await _lookifyService.VehiclePlate.ConsultAsync(plate);

            Console.WriteLine($"Plate: {result.Plate}");
            Console.WriteLine($"Brand: {result.Brand}");
            Console.WriteLine($"Model: {result.Model}");
            Console.WriteLine($"Manufacture Year: {result.ManufactureYear}");
            Console.WriteLine($"Color: {result.Color}");
            Console.WriteLine($"City: {result.City}");
            Console.WriteLine($"State: {result.State}");
        }
        catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}