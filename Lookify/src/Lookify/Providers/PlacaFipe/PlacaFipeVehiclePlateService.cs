using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Lookify.VehiclePlate;

namespace Lookify.Providers.PlacaFipe;

internal sealed class PlacaFipeVehiclePlateService : IVehiclePlateProviderService {

    private const int SuccessCode = 1;
    private const int DegradedSuccessCode = 22;

    public string ProviderName { get; init; } = string.Empty;

    public string BaseAddress { get; init; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public void UpdateEnabled(bool isEnabled) =>
        IsEnabled = isEnabled;

    public async Task<VehiclePlateLookifyResultDto> RequestAsync(
        string plate,
        string token,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.VehiclePlate");
        var response = await client.PostAsJsonAsync(
            $"{BaseAddress}getplacafipe",
            new PlacaFipeRequestBody(plate, token),
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<PlacaFipeProviderResponse>(
            providerName: ProviderName,
            plate,
            response,
            cancellationToken);

        if (payload.Codigo is not SuccessCode and not DegradedSuccessCode) {
            throw new InvalidOperationException(
                payload.Msg ?? $"{ProviderName} retornou o código {payload.Codigo} para a placa {plate}.");
        }

        return payload.ToResult();
    }

    private sealed record PlacaFipeRequestBody(
        [property: JsonPropertyName("placa")]
        string Placa,
        [property: JsonPropertyName("token")]
        string Token);
}
