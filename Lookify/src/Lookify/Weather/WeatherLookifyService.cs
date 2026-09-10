using Lookify.Providers.Cptec;
using Lookify.Providers.OpenMeteo;
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
        return await ExecuteAsync(
            $"previsão do tempo para {latitude},{longitude}",
            provider => provider switch {
                WeatherLookifyProviderEnum.OpenMeteo =>
                    OpenMeteoWeatherProviderRequest.RequestByCoordinatesAsync(
                        latitude,
                        longitude,
                        days,
                        _httpFactory,
                        cancellationToken),
                WeatherLookifyProviderEnum.Cptec =>
                    throw new NotSupportedException(
                        $"Provider {WeatherLookifyProviderEnum.Cptec} não oferece consulta por coordenadas."),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
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

        return await ExecuteAsync(
            operationDescription,
            provider => provider switch {
                WeatherLookifyProviderEnum.OpenMeteo =>
                    OpenMeteoWeatherProviderRequest.RequestByCityNameAsync(
                        cityName,
                        normalizedState,
                        days,
                        _httpFactory,
                        cancellationToken),
                WeatherLookifyProviderEnum.Cptec =>
                    CptecWeatherProviderRequest.RequestByCityNameAsync(
                        cityName,
                        normalizedState,
                        days,
                        _httpFactory,
                        cancellationToken),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
    }

    private async Task<TResult> ExecuteAsync<TResult>(
        string operationDescription,
        Func<WeatherLookifyProviderEnum, Task<TResult>> request)
    {
        var enabledProviders = GetEnabledProviders();
        var failures = new List<Exception>();

        foreach (var provider in enabledProviders) {
            try {
                return await request(provider);
            }
            catch (Exception exception) {
                failures.Add(exception);
                _logger.LogError(
                    exception,
                    "Falha ao consultar {Operation} no provedor {Provider}.",
                    operationDescription,
                    provider);
            }
        }

        throw new InvalidOperationException(
            $"Não foi possível consultar {operationDescription} em nenhum provedor configurado.",
            new AggregateException(failures));
    }

    private List<WeatherLookifyProviderEnum> GetEnabledProviders() =>
        _options.WeatherProviders
            .Where(provider => _options.GetProviderOptions(provider).Enabled)
            .ToList();
}
