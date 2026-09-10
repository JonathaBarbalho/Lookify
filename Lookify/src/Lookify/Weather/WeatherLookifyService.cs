using Lookify.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Weather;

internal sealed class WeatherLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : IWeatherLookifyService {

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LookifyOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    public async Task<List<WeatherForecastLookifyResultDto>> GetForecastByCoordinatesAsync(
        decimal latitude,
        decimal longitude,
        int? days = null,
        CancellationToken cancellationToken = default)
    {
        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"previsão do tempo para {latitude},{longitude}",
            _logger,
            provider => GetService(provider).GetForecastByCoordinatesAsync(
                latitude,
                longitude,
                days,
                _httpFactory,
                cancellationToken));
    }

    public async Task<List<WeatherForecastLookifyResultDto>> GetForecastByCityNameAsync(
        string cityName,
        string? state = null,
        int? days = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(cityName)) {
            throw new ArgumentException("City name cannot be null or empty.", nameof(cityName));
        }

        var normalizedState = string.IsNullOrWhiteSpace(state) ? null : state.Trim().ToUpperInvariant();
        var operationDescription = normalizedState is null
            ? $"previsão do tempo de {cityName}"
            : $"previsão do tempo de {cityName}/{normalizedState}";

        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            operationDescription,
            _logger,
            provider => GetService(provider).GetForecastByCityNameAsync(
                cityName,
                normalizedState,
                days,
                _httpFactory,
                cancellationToken));
    }

    private List<WeatherLookifyProviderEnum> GetEnabledProviders() =>
        _options.WeatherProviders
            .Where(provider => _options.GetProviderOptions(provider).IsEnabled)
            .ToList();

    private static IProvider GetProvider(WeatherLookifyProviderEnum provider) =>
        provider switch {
            WeatherLookifyProviderEnum.OpenMeteo => ProviderRegistry.OpenMeteo,
            WeatherLookifyProviderEnum.Cptec => ProviderRegistry.Cptec,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };

    private static IWeatherProviderService GetService(WeatherLookifyProviderEnum provider) =>
        GetProvider(provider).Services
            .OfType<IWeatherProviderService>()
            .First();
}
