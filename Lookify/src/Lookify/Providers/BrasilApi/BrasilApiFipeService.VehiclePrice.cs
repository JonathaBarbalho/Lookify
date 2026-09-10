using System.Text.Json.Serialization;
using Lookify.Fipe;
using Lookify.Providers.JsonConverters;

namespace Lookify.Providers.BrasilApi;

internal sealed partial class BrasilApiFipeService {

    public async Task<FipeVehiclePriceLookifyResultDto> GetVehiclePriceAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        string modelCode,
        string yearCode,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var vehicleTypeSegment = vehicleType.ToPathSegment();
        var fullAddress = $"{BaseAddress}api/fipe/detalhes/v1/{vehicleTypeSegment}/{brandCode}/{modelCode}/{yearCode}";
        if (referenceTable is not null) {
            fullAddress += $"?tabela_referencia={referenceTable}";
        }

        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Fipe");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<BrasilApiFipeVehiclePriceProviderResponse>(
            providerName: ProviderName,
            identifier: yearCode,
            response,
            cancellationToken);

        return payload.ToResult();
    }

    public async Task<List<FipeVehiclePriceLookifyResultDto>> GetPriceByFipeCodeAsync(
        string fipeCode,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/fipe/preco/v1/{fipeCode}";
        if (referenceTable is not null) {
            fullAddress += $"?tabela_referencia={referenceTable}";
        }

        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Fipe");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<BrasilApiFipeVehiclePriceProviderResponse>>(
            providerName: ProviderName,
            identifier: fipeCode,
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class BrasilApiFipeVehiclePriceProviderResponse {

    [property: JsonPropertyName("valor")]
    public string? Valor { get; init; }

    [property: JsonPropertyName("marca")]
    public string? Marca { get; init; }

    [property: JsonPropertyName("modelo")]
    public string? Modelo { get; init; }

    [property: JsonPropertyName("anoModelo")]
    [property: JsonConverter(typeof(FlexibleInt32JsonConverter))]
    public int? AnoModelo { get; init; }

    [property: JsonPropertyName("combustivel")]
    public string? Combustivel { get; init; }

    [property: JsonPropertyName("codigoFipe")]
    public string? CodigoFipe { get; init; }

    [property: JsonPropertyName("mesReferencia")]
    public string? MesReferencia { get; init; }

    [property: JsonPropertyName("tipoVeiculo")]
    [property: JsonConverter(typeof(FlexibleInt32JsonConverter))]
    public int? TipoVeiculo { get; init; }

    [property: JsonPropertyName("siglaCombustivel")]
    public string? SiglaCombustivel { get; init; }

    [property: JsonPropertyName("dataConsulta")]
    public string? DataConsulta { get; init; }

    public FipeVehiclePriceLookifyResultDto ToResult() =>
        new FipeVehiclePriceLookifyResultDto {
            FipeCode = CodigoFipe,
            Brand = Marca,
            Model = Modelo,
            ModelYear = AnoModelo,
            Fuel = Combustivel,
            FuelAcronym = SiglaCombustivel,
            Value = Valor,
            ReferenceMonth = MesReferencia,
            VehicleTypeCode = TipoVeiculo,
            RequestDate = DataConsulta
        };
}
