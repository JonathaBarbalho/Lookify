using Lookify.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Cnpj;

internal sealed class CnpjLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : ICnpjLookifyService {

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

        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"o CNPJ {sanitizedCnpj}",
            _logger,
            provider => GetService(provider).RequestAsync(
                sanitizedCnpj,
                _httpFactory,
                cancellationToken));
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
            CnpjLookifyProviderEnum.BrasilApi => ProviderRegistry.BrasilApi,
            CnpjLookifyProviderEnum.ReceitaWs => ProviderRegistry.ReceitaWs,
            CnpjLookifyProviderEnum.Publica => ProviderRegistry.Publica,
            CnpjLookifyProviderEnum.MinhaReceita => ProviderRegistry.MinhaReceita,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };

    private static ICnpjProviderService GetService(CnpjLookifyProviderEnum provider) =>
        GetProvider(provider).Services
            .OfType<ICnpjProviderService>()
            .First();
}
