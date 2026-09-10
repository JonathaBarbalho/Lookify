namespace Lookify.Weather;

public interface IWeatherLookifyService {

    Task<List<WeatherForecastLookifyResultDto>> GetForecastByCoordinatesAsync(
        decimal latitude,
        decimal longitude,
        int? days = null,
        CancellationToken cancellationToken = default);

    Task<List<WeatherForecastLookifyResultDto>> GetForecastByCityNameAsync(
        string cityName,
        string? state = null,
        int? days = null,
        CancellationToken cancellationToken = default);
}
