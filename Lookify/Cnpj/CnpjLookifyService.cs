using System.Text.RegularExpressions;
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

    private static readonly Regex CnpjFormatRegex = new(
        "^[A-Z0-9]{12}\\d{2}$",
        RegexOptions.Compiled);

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
            _options.TimeOut,
            _logger,
            (provider, token) => GetService(provider).RequestAsync(
                sanitizedCnpj,
                _httpFactory,
                token),
            cancellationToken);
    }

    private static string SanitizeCnpj(string cnpj)
    {
        var sanitizedCnpj = new string(cnpj.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        if (!CnpjFormatRegex.IsMatch(sanitizedCnpj))
            throw new ArgumentException(
                "Cnpj must contain 14 characters after sanitization: 12 alphanumeric characters followed by 2 numeric check digits.",
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
