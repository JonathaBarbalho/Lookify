using Lookify;
using Lookify.Fipe;

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
            Console.WriteLine($"Ibge City Code: {result.IbgeCityCode}");
            Console.WriteLine($"Ddd: {result.Ddd}");
            Console.WriteLine($"Latitude: {result.Latitude}");
            Console.WriteLine($"Longitude: {result.Longitude}");
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

    public async Task TestFipeLookup()
    {
        Console.WriteLine("Testing FIPE Lookup...");

        try {
            Console.WriteLine("Vehicle type: 1. Cars  2. Motorcycles  3. Trucks");
            Console.Write("Select an option (default 1): ");
            var vehicleType = Console.ReadLine() switch {
                "2" => FipeVehicleType.Motorcycles,
                "3" => FipeVehicleType.Trucks,
                _ => FipeVehicleType.Cars
            };

            var brands = await _lookifyService.Fipe.GetBrandsAsync(vehicleType);
            Console.WriteLine($"{brands.Count} brands found. Showing the first 10:");
            foreach (var brand in brands.Take(10)) {
                Console.WriteLine($"  {brand.Code} - {brand.Name}");
            }

            Console.Write("Enter a brand code: ");
            var brandCode = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(brandCode)) {
                Console.WriteLine("No brand code was provided.");
                return;
            }

            var models = await _lookifyService.Fipe.GetModelsAsync(vehicleType, brandCode);
            Console.WriteLine($"{models.Count} models found. Showing the first 10:");
            foreach (var model in models.Take(10)) {
                Console.WriteLine($"  {model.Code} - {model.Name}");
            }

            Console.Write("Enter a model code: ");
            var modelCode = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(modelCode)) {
                Console.WriteLine("No model code was provided.");
                return;
            }

            var years = await _lookifyService.Fipe.GetModelYearsAsync(vehicleType, brandCode, modelCode);
            Console.WriteLine($"{years.Count} model years found:");
            foreach (var year in years) {
                Console.WriteLine($"  {year.Code} - {year.Label}");
            }

            Console.Write("Enter a year code: ");
            var yearCode = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(yearCode)) {
                Console.WriteLine("No year code was provided.");
                return;
            }

            var price = await _lookifyService.Fipe.GetVehiclePriceAsync(vehicleType, brandCode, modelCode, yearCode);
            Console.WriteLine($"FIPE Code: {price.FipeCode}");
            Console.WriteLine($"Brand: {price.Brand}");
            Console.WriteLine($"Model: {price.Model}");
            Console.WriteLine($"Model Year: {price.ModelYear}");
            Console.WriteLine($"Fuel: {price.Fuel}");
            Console.WriteLine($"Value: {price.Value}");
            Console.WriteLine($"Reference Month: {price.ReferenceMonth}");
        }
        catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task TestIbgeLookup()
    {
        Console.WriteLine("Testing IBGE Lookup...");

        try {
            var states = await _lookifyService.Ibge.GetStatesAsync();
            Console.WriteLine($"{states.Count} states found. Showing the first 5:");
            foreach (var state in states.Take(5)) {
                Console.WriteLine($"  {state.Uf} - {state.Name} ({state.RegionName})");
            }

            Console.Write("Enter a UF (e.g. SP): ");
            var uf = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(uf)) {
                Console.WriteLine("No UF was provided.");
                return;
            }

            var selectedState = await _lookifyService.Ibge.GetStateAsync(uf);
            Console.WriteLine($"State: {selectedState.Name} ({selectedState.Uf}) - Region: {selectedState.RegionName}");

            var cities = await _lookifyService.Ibge.GetCitiesByStateAsync(uf);
            Console.WriteLine($"{cities.Count} cities found. Showing the first 10:");
            foreach (var city in cities.Take(10)) {
                Console.WriteLine($"  {city.Id} - {city.Name}");
            }

            var regions = await _lookifyService.Ibge.GetRegionsAsync();
            Console.WriteLine($"{regions.Count} regions found:");
            foreach (var region in regions) {
                Console.WriteLine($"  {region.Acronym} - {region.Name}");
            }

            var allCities = await _lookifyService.Ibge.GetAllCitiesAsync();
            Console.WriteLine($"{allCities.Count} cities found in Brazil (all states).");
        }
        catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task TestBankLookup()
    {
        Console.WriteLine("Testing Bank Lookup...");

        try {
            var banks = await _lookifyService.Bank.GetAllBanksAsync();
            Console.WriteLine($"{banks.Count} banks found. Showing the first 5:");
            foreach (var bank in banks.Take(5)) {
                Console.WriteLine($"  {bank.Code} - {bank.Name}");
            }

            Console.Write("Enter a bank code (e.g. 1 for Banco do Brasil): ");
            var codeInput = Console.ReadLine();
            if (!int.TryParse(codeInput, out var code)) {
                Console.WriteLine("No valid bank code was provided.");
                return;
            }

            var selectedBank = await _lookifyService.Bank.GetBankByCodeAsync(code);
            Console.WriteLine($"Code: {selectedBank.Code}");
            Console.WriteLine($"Ispb: {selectedBank.Ispb}");
            Console.WriteLine($"Name: {selectedBank.Name}");
            Console.WriteLine($"Full Name: {selectedBank.FullName}");
            Console.WriteLine($"Cnpj: {selectedBank.Cnpj}");
            Console.WriteLine($"Address: {selectedBank.Street}, {selectedBank.District}, {selectedBank.City} - {selectedBank.State}");
        }
        catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}