using System.Text.RegularExpressions;
using Lookify.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Ibge;

internal sealed class IbgeLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : IIbgeLookifyService {

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LookifyOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    private static readonly Regex UfFormatRegex = new(
        "^[A-Z]{2}$",
        RegexOptions.Compiled);

    public async Task<List<IbgeStateLookifyResultDto>> GetStatesAsync(
        CancellationToken cancellationToken = default)
    {
        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            "estados do IBGE",
            _logger,
            provider => GetService(provider).GetStatesAsync(
                _httpFactory,
                cancellationToken));
    }

    public async Task<IbgeStateLookifyResultDto> GetStateAsync(
        string uf,
        CancellationToken cancellationToken = default)
    {
        var sanitizedUf = SanitizeUf(uf);

        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"estado {sanitizedUf} do IBGE",
            _logger,
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

        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"municípios do estado {sanitizedUf} do IBGE",
            _logger,
            provider => GetService(provider).GetCitiesByStateAsync(
                sanitizedUf,
                _httpFactory,
                cancellationToken));
    }

    public async Task<List<IbgeCityLookifyResultDto>> GetAllCitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            "todos os municípios do IBGE",
            _logger,
            provider => GetService(provider).GetAllCitiesAsync(
                _httpFactory,
                cancellationToken));
    }

    public async Task<List<IbgeRegionLookifyResultDto>> GetRegionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            "regiões do IBGE",
            _logger,
            provider => GetService(provider).GetRegionsAsync(
                _httpFactory,
                cancellationToken));
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
            IbgeLookifyProviderEnum.Ibge => ProviderRegistry.Ibge,
            IbgeLookifyProviderEnum.BrasilApi => ProviderRegistry.BrasilApi,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };

    private static IIbgeProviderService GetService(IbgeLookifyProviderEnum provider) =>
        GetProvider(provider).Services
            .OfType<IIbgeProviderService>()
            .First();
}
