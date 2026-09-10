using System.Text.Json.Serialization;
using Lookify.Weather;

namespace Lookify.Providers.Cptec;

internal sealed class CptecWeatherService : IWeatherProviderService {

    public string ProviderName { get; init; } = string.Empty;

    public string BaseAddress { get; init; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public void UpdateEnabled(bool isEnabled) =>
        IsEnabled = isEnabled;

    public Task<List<WeatherForecastLookifyResultDto>> GetForecastByCoordinatesAsync(
        decimal latitude,
        decimal longitude,
        int? days,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(
            $"Provider {ProviderName} não oferece consulta por coordenadas.");

    public async Task<List<WeatherForecastLookifyResultDto>> GetForecastByCityNameAsync(
        string cityName,
        string? state,
        int? days,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var cityAddress = $"{BaseAddress}api/cptec/v1/cidade/{Uri.EscapeDataString(cityName)}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Weather");
        var cityResponse = await client.GetAsync(
            cityAddress,
            cancellationToken);
        var cityPayload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<CptecCityProviderResponse>>(
            providerName: ProviderName,
            identifier: cityName,
            cityResponse,
            cancellationToken);

        var city = state is null
            ? cityPayload.FirstOrDefault()
            : cityPayload.FirstOrDefault(candidate => string.Equals(candidate.Estado, state, StringComparison.OrdinalIgnoreCase));
        if (city?.Id is null) {
            var message = state is null
                ? $"{ProviderName} não encontrou a cidade {cityName}."
                : $"{ProviderName} não encontrou a cidade {cityName} no estado {state}.";
            throw new InvalidOperationException(message);
        }

        var forecastAddress = $"{BaseAddress}api/cptec/v1/clima/previsao/{city.Id}";
        if (days is not null) {
            forecastAddress += $"/{days}";
        }

        var forecastResponse = await client.GetAsync(
            forecastAddress,
            cancellationToken);
        var forecastPayload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<CptecForecastProviderResponse>(
            providerName: ProviderName,
            identifier: cityName,
            forecastResponse,
            cancellationToken);

        return forecastPayload.ToResult();
    }
}

internal sealed record class CptecCityProviderResponse {

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }

    [property: JsonPropertyName("id")]
    public int? Id { get; init; }

    [property: JsonPropertyName("estado")]
    public string? Estado { get; init; }
}

internal sealed record class CptecForecastProviderResponse {

    [property: JsonPropertyName("cidade")]
    public string? Cidade { get; init; }

    [property: JsonPropertyName("estado")]
    public string? Estado { get; init; }

    [property: JsonPropertyName("clima")]
    public List<CptecForecastDayResponse> Clima { get; init; } = [];

    public List<WeatherForecastLookifyResultDto> ToResult() =>
        Clima.Select(
            day => new WeatherForecastLookifyResultDto {
                Date = day.Data,
                MinTemperature = day.Min,
                MaxTemperature = day.Max,
                ConditionCode = day.Condicao,
                ConditionDescription = day.CondicaoDesc,
                UvIndex = day.IndiceUv,
                City = Cidade,
                State = Estado
            }).ToList();
}

internal sealed record class CptecForecastDayResponse {

    [property: JsonPropertyName("data")]
    public DateOnly? Data { get; init; }

    [property: JsonPropertyName("condicao")]
    public string? Condicao { get; init; }

    [property: JsonPropertyName("condicao_desc")]
    public string? CondicaoDesc { get; init; }

    [property: JsonPropertyName("min")]
    public decimal? Min { get; init; }

    [property: JsonPropertyName("max")]
    public decimal? Max { get; init; }

    [property: JsonPropertyName("indice_uv")]
    public decimal? IndiceUv { get; init; }
}
