using System.Text.RegularExpressions;
using Lookify.Providers.BrasilApi;
using Lookify.Providers.Ibge;
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
        return await ExecuteAsync(
            "estados do IBGE",
            provider => provider switch {
                IbgeLookifyProviderEnum.Ibge =>
                    IbgeStateProviderRequest.RequestAllAsync(
                        _httpFactory,
                        cancellationToken),
                IbgeLookifyProviderEnum.BrasilApi =>
                    BrasilApiIbgeStateProviderRequest.RequestAllAsync(
                        _httpFactory,
                        cancellationToken),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
    }

    public async Task<IbgeStateLookifyResultDto> GetStateAsync(
        string uf,
        CancellationToken cancellationToken = default)
    {
        var sanitizedUf = SanitizeUf(uf);

        return await ExecuteAsync(
            $"estado {sanitizedUf} do IBGE",
            provider => provider switch {
                IbgeLookifyProviderEnum.Ibge =>
                    IbgeStateProviderRequest.RequestByUfAsync(
                        sanitizedUf,
                        _httpFactory,
                        cancellationToken),
                IbgeLookifyProviderEnum.BrasilApi =>
                    BrasilApiIbgeStateProviderRequest.RequestByUfAsync(
                        sanitizedUf,
                        _httpFactory,
                        cancellationToken),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
    }

    public async Task<List<IbgeCityLookifyResultDto>> GetCitiesByStateAsync(
        string uf,
        CancellationToken cancellationToken = default)
    {
        var sanitizedUf = SanitizeUf(uf);

        return await ExecuteAsync(
            $"municípios do estado {sanitizedUf} do IBGE",
            provider => provider switch {
                IbgeLookifyProviderEnum.Ibge =>
                    IbgeCityProviderRequest.RequestByStateAsync(
                        sanitizedUf,
                        _httpFactory,
                        cancellationToken),
                IbgeLookifyProviderEnum.BrasilApi =>
                    BrasilApiIbgeCityProviderRequest.RequestByStateAsync(
                        sanitizedUf,
                        _httpFactory,
                        cancellationToken),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
    }

    public async Task<List<IbgeCityLookifyResultDto>> GetAllCitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            "todos os municípios do IBGE",
            provider => provider switch {
                IbgeLookifyProviderEnum.Ibge =>
                    IbgeCityProviderRequest.RequestAllAsync(
                        _httpFactory,
                        cancellationToken),
                IbgeLookifyProviderEnum.BrasilApi =>
                    throw new NotSupportedException(
                        $"Provider {IbgeLookifyProviderEnum.BrasilApi} não oferece listagem de todos os municípios de uma vez."),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
    }

    public async Task<List<IbgeRegionLookifyResultDto>> GetRegionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            "regiões do IBGE",
            provider => provider switch {
                IbgeLookifyProviderEnum.Ibge =>
                    IbgeRegionProviderRequest.RequestAllAsync(
                        _httpFactory,
                        cancellationToken),
                IbgeLookifyProviderEnum.BrasilApi =>
                    BrasilApiIbgeRegionProviderRequest.RequestAllAsync(
                        _httpFactory,
                        cancellationToken),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
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
            .Where(provider => _options.GetProviderOptions(provider).Enabled)
            .ToList();
}
