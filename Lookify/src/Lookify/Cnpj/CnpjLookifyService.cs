using Lookify.Providers;
using Lookify.Providers.BrasilApi;
using Lookify.Providers.MinhaReceita;
using Lookify.Providers.Publica;
using Lookify.Providers.ReceitaWs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Cnpj;

internal sealed class CnpjLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : ICnpjLookifyService {

    private static readonly BrasilApiProvider _brasilApi = new();
    private static readonly ReceitaWsProvider _receitaWs = new();
    private static readonly PublicaProvider _publica = new();
    private static readonly MinhaReceitaProvider _minhaReceita = new();

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LookifyOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    public async Task<CnpjLookifyResultDto> ConsultAsync(
        string cnpj,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(cnpj)) {
            throw new ArgumentException("Cnpj cannot be null or empty.", nameof(cnpj));
        }

        var sanitizedCnpj = SanitizeCnpj(cnpj);
        var enabledProviders = GetEnabledProviders();
        var failures = new List<Exception>();

        foreach (var provider in enabledProviders) {
            try {
                var service = GetProvider(provider).Services
                    .OfType<ICnpjProviderService>()
                    .First();

                return await service.RequestAsync(
                    sanitizedCnpj,
                    _httpFactory,
                    cancellationToken);
            }
            catch (Exception exception) {
                failures.Add(exception);
                _logger.LogError(
                    exception,
                    "Falha ao consultar o CNPJ {Cnpj} no provedor {Provider}.",
                    sanitizedCnpj,
                    provider);
            }
        }

        throw new InvalidOperationException(
            $"Não foi possível consultar o CNPJ {sanitizedCnpj} em nenhum provedor configurado.",
            new AggregateException(failures));
    }

    private string SanitizeCnpj(string cnpj)
    {
        var sanitizedCnpj = new string(cnpj.Where(char.IsDigit).ToArray());

        if (sanitizedCnpj.Length != 14)
            throw new ArgumentException(
                "Cnpj must contain exactly 14 digits after sanitization.",
                nameof(cnpj));

        return sanitizedCnpj;
    }

    private List<CnpjLookifyProviderEnum> GetEnabledProviders() =>
        _options.CnpjProviders
            .Where(provider => _options.GetProviderOptions(provider).IsEnabled)
            .ToList();

    private static IProvider GetProvider(CnpjLookifyProviderEnum provider) =>
        provider switch {
            CnpjLookifyProviderEnum.BrasilApi => _brasilApi,
            CnpjLookifyProviderEnum.ReceitaWs => _receitaWs,
            CnpjLookifyProviderEnum.Publica => _publica,
            CnpjLookifyProviderEnum.MinhaReceita => _minhaReceita,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
}
