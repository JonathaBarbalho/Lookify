using Lookify.Weather;

namespace Lookify.Providers;

internal interface IWeatherProviderService : IProviderService {

    Task<List<WeatherForecastLookifyResultDto>> GetForecastByCoordinatesAsync(
        decimal latitude,
        decimal longitude,
        int? days,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);

    Task<List<WeatherForecastLookifyResultDto>> GetForecastByCityNameAsync(
        string cityName,
        string? state,
        int? days,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
