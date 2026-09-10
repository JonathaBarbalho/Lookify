using System.Globalization;
using System.Text.Json.Serialization;
using Lookify.Weather;

namespace Lookify.Providers.OpenMeteo;

internal static class OpenMeteoWeatherProviderRequest {

    public static string ProviderName => "OpenMeteo";

    public static string BaseAddress => "https://api.open-meteo.com/";

    public static string GeocodingBaseAddress => "https://geocoding-api.open-meteo.com/";

    public static async Task<List<WeatherForecastLookifyResultDto>> RequestByCoordinatesAsync(
        decimal latitude,
        decimal longitude,
        int? days,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var latitudeText = latitude.ToString(CultureInfo.InvariantCulture);
        var longitudeText = longitude.ToString(CultureInfo.InvariantCulture);
        var fullAddress =
            $"{BaseAddress}v1/forecast?latitude={latitudeText}&longitude={longitudeText}" +
            "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max,uv_index_max" +
            "&timezone=auto";
        if (days is not null) {
            fullAddress += $"&forecast_days={days}";
        }

        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Weather");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<OpenMeteoForecastProviderResponse>(
            providerName: ProviderName,
            identifier: $"{latitudeText},{longitudeText}",
            response,
            cancellationToken);

        return payload.ToResult(city: null, state: null);
    }

    public static async Task<List<WeatherForecastLookifyResultDto>> RequestByCityNameAsync(
        string cityName,
        string? state,
        int? days,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var resultCount = state is null ? 1 : 20;
        var geocodingAddress = $"{GeocodingBaseAddress}v1/search?name={Uri.EscapeDataString(cityName)}&count={resultCount}&language=pt&country=BR";
        var geocodingClient = httpFactory.CreateClient($"Lookify.{ProviderName}.Weather");
        var geocodingResponse = await geocodingClient.GetAsync(
            geocodingAddress,
            cancellationToken);
        var geocodingPayload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<OpenMeteoGeocodingProviderResponse>(
            providerName: ProviderName,
            identifier: cityName,
            geocodingResponse,
            cancellationToken);

        var location = state is null
            ? geocodingPayload.Results.FirstOrDefault()
            : geocodingPayload.Results.FirstOrDefault(
                candidate => string.Equals(candidate.Admin1, OpenMeteoBrazilianStates.GetName(state), StringComparison.OrdinalIgnoreCase));
        if (location is null || location.Latitude is null || location.Longitude is null) {
            var message = state is null
                ? $"{ProviderName} não encontrou a cidade {cityName}."
                : $"{ProviderName} não encontrou a cidade {cityName} no estado {state}.";
            throw new InvalidOperationException(message);
        }

        var fullAddress =
            $"{BaseAddress}v1/forecast?latitude={location.Latitude.Value.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={location.Longitude.Value.ToString(CultureInfo.InvariantCulture)}" +
            "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max,uv_index_max" +
            "&timezone=auto";
        if (days is not null) {
            fullAddress += $"&forecast_days={days}";
        }

        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Weather");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<OpenMeteoForecastProviderResponse>(
            providerName: ProviderName,
            identifier: cityName,
            response,
            cancellationToken);

        return payload.ToResult(city: location.Name, state: OpenMeteoBrazilianStates.GetUf(location.Admin1));
    }
}

internal sealed record class OpenMeteoForecastProviderResponse {

    [property: JsonPropertyName("daily")]
    public OpenMeteoDailyProviderResponse? Daily { get; init; }

    public List<WeatherForecastLookifyResultDto> ToResult(
        string? city,
        string? state)
    {
        if (Daily is null) {
            return [];
        }

        var result = new List<WeatherForecastLookifyResultDto>();
        for (var index = 0; index < Daily.Time.Count; index++) {
            var weatherCode = GetOrDefault(Daily.WeatherCode, index);
            result.Add(
                new WeatherForecastLookifyResultDto {
                    Date = Daily.Time[index],
                    MinTemperature = GetOrDefault(Daily.TemperatureMin, index),
                    MaxTemperature = GetOrDefault(Daily.TemperatureMax, index),
                    ConditionCode = weatherCode?.ToString(CultureInfo.InvariantCulture),
                    ConditionDescription = OpenMeteoWeatherCodeDescriptions.Describe(weatherCode),
                    PrecipitationProbability = GetOrDefault(Daily.PrecipitationProbabilityMax, index),
                    UvIndex = GetOrDefault(Daily.UvIndexMax, index),
                    City = city,
                    State = state
                });
        }

        return result;
    }

    private static T? GetOrDefault<T>(
        List<T?> values,
        int index) where T : struct =>
        index < values.Count ? values[index] : null;
}

internal sealed record class OpenMeteoDailyProviderResponse {

    [property: JsonPropertyName("time")]
    public List<DateOnly> Time { get; init; } = [];

    [property: JsonPropertyName("weather_code")]
    public List<int?> WeatherCode { get; init; } = [];

    [property: JsonPropertyName("temperature_2m_max")]
    public List<decimal?> TemperatureMax { get; init; } = [];

    [property: JsonPropertyName("temperature_2m_min")]
    public List<decimal?> TemperatureMin { get; init; } = [];

    [property: JsonPropertyName("precipitation_probability_max")]
    public List<int?> PrecipitationProbabilityMax { get; init; } = [];

    [property: JsonPropertyName("uv_index_max")]
    public List<decimal?> UvIndexMax { get; init; } = [];
}

internal sealed record class OpenMeteoGeocodingProviderResponse {

    [property: JsonPropertyName("results")]
    public List<OpenMeteoGeocodingResultResponse> Results { get; init; } = [];
}

internal sealed record class OpenMeteoGeocodingResultResponse {

    [property: JsonPropertyName("name")]
    public string? Name { get; init; }

    [property: JsonPropertyName("latitude")]
    public decimal? Latitude { get; init; }

    [property: JsonPropertyName("longitude")]
    public decimal? Longitude { get; init; }

    [property: JsonPropertyName("admin1")]
    public string? Admin1 { get; init; }
}
