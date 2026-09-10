using System.Text.RegularExpressions;
using Lookify.Providers;
using Lookify.Providers.BrasilApi;
using Lookify.Providers.Ibge;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Ibge;

internal sealed class IbgeLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : IIbgeLookifyService {

    private static readonly IbgeProvider _ibge = new();
    private static readonly BrasilApiProvider _brasilApi = new();

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LookifyOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    private static readonly Regex UfFormatRegex = new(
        "^[A-Z]{2}$",
        RegexOptions.Compiled);

    public async Task<List<IbgeStateLookifyResultDto>> GetStatesAsync(
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            "estados do IBGE",
            provider => GetService(provider).GetStatesAsync(
                _httpFactory,
                cancellationToken));
    }

    public async Task<IbgeStateLookifyResultDto> GetStateAsync(
        string uf,
        CancellationToken cancellationToken = default)
    {
        var sanitizedUf = SanitizeUf(uf);

        return await ExecuteAsync(
            $"estado {sanitizedUf} do IBGE",
            provider => GetService(provider).GetStateAsync(
                sanitizedUf,
                _httpFactory,
                cancellationToken));
    }

    public async Task<List<IbgeCityLookifyResultDto>> GetCitiesByStateAsync(
        string uf,
        CancellationToken cancellationToken = default)
    {
        var sanitizedUf = SanitizeUf(uf);

        return await ExecuteAsync(
            $"municípios do estado {sanitizedUf} do IBGE",
            provider => GetService(provider).GetCitiesByStateAsync(
                sanitizedUf,
                _httpFactory,
                cancellationToken));
    }

    public async Task<List<IbgeCityLookifyResultDto>> GetAllCitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            "todos os municípios do IBGE",
            provider => GetService(provider).GetAllCitiesAsync(
                _httpFactory,
                cancellationToken));
    }

    public async Task<List<IbgeRegionLookifyResultDto>> GetRegionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            "regiões do IBGE",
            provider => GetService(provider).GetRegionsAsync(
                _httpFactory,
                cancellationToken));
    }

    private async Task<TResult> ExecuteAsync<TResult>(
        string operationDescription,
        Func<IbgeLookifyProviderEnum, Task<TResult>> request)
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

    private static string SanitizeUf(
        string uf)
    {
        if (string.IsNullOrEmpty(uf)) {
            throw new ArgumentException("Uf cannot be null or empty.", nameof(uf));
        }

        var sanitizedUf = uf.Trim().ToUpperInvariant();

        if (!UfFormatRegex.IsMatch(sanitizedUf)) {
            throw new ArgumentException("Uf must contain exactly 2 letters.", nameof(uf));
        }

        return sanitizedUf;
    }

    private List<IbgeLookifyProviderEnum> GetEnabledProviders() =>
        _options.IbgeProviders
            .Where(provider => _options.GetProviderOptions(provider).IsEnabled)
            .ToList();

    private static IProvider GetProvider(IbgeLookifyProviderEnum provider) =>
        provider switch {
            IbgeLookifyProviderEnum.Ibge => _ibge,
            IbgeLookifyProviderEnum.BrasilApi => _brasilApi,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };

    private static IIbgeProviderService GetService(IbgeLookifyProviderEnum provider) =>
        GetProvider(provider).Services
            .OfType<IIbgeProviderService>()
            .First();
}
