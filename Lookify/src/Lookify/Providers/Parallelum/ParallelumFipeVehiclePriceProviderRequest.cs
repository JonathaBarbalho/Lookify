using System.Text.Json.Serialization;
using Lookify.Fipe;
using Lookify.Providers.JsonConverters;

namespace Lookify.Providers.Parallelum;

internal static class ParallelumFipeVehiclePriceProviderRequest {

    public static string ProviderName => "Parallelum";

    public static string BaseAddress => "https://fipe.parallelum.com.br/";

    public static async Task<FipeVehiclePriceLookifyResultDto> RequestDetailsAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        string modelCode,
        string yearCode,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var vehicleTypeSegment = vehicleType.ToParallelumPathSegment();
        var fullAddress = $"{BaseAddress}api/v2/{vehicleTypeSegment}/brands/{brandCode}/models/{modelCode}/years/{yearCode}";
        if (referenceTable is not null) {
            fullAddress += $"?reference={referenceTable}";
        }

        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Fipe");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<ParallelumFipeVehiclePriceProviderResponse>(
            providerName: ProviderName,
            identifier: yearCode,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}

internal sealed record class ParallelumFipeVehiclePriceProviderResponse {

    [property: JsonPropertyName("price")]
    public string? Price { get; init; }

    [property: JsonPropertyName("brand")]
    public string? Brand { get; init; }

    [property: JsonPropertyName("model")]
    public string? Model { get; init; }

    [property: JsonPropertyName("modelYear")]
    [property: JsonConverter(typeof(FlexibleInt32JsonConverter))]
    public int? ModelYear { get; init; }

    [property: JsonPropertyName("fuel")]
    public string? Fuel { get; init; }

    [property: JsonPropertyName("codeFipe")]
    public string? CodeFipe { get; init; }

    [property: JsonPropertyName("referenceMonth")]
    public string? ReferenceMonth { get; init; }

    [property: JsonPropertyName("vehicleType")]
    [property: JsonConverter(typeof(FlexibleInt32JsonConverter))]
    public int? VehicleType { get; init; }

    [property: JsonPropertyName("fuelAcronym")]
    public string? FuelAcronym { get; init; }

    public FipeVehiclePriceLookifyResultDto ToResult() =>
        new FipeVehiclePriceLookifyResultDto {
            FipeCode = CodeFipe,
            Brand = Brand,
            Model = Model,
            ModelYear = ModelYear,
            Fuel = Fuel,
            FuelAcronym = FuelAcronym,
            Value = Price,
            ReferenceMonth = ReferenceMonth,
            VehicleTypeCode = VehicleType,
            RequestDate = null
        };
}
