using Lookify.Providers.BrasilApi;
using Lookify.Providers.ViaCep;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Cep;

internal sealed class CepLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : ICepLookifyService {

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LookifyOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    public async Task<CepLookifyResultDto> ConsultAsync(
        string zipCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(zipCode)) {
            throw new ArgumentException("Zip code cannot be null or empty.", nameof(zipCode));
        }

        var sanitizedZipCode = SanitizeZipCode(zipCode);
        var enabledProviders = GetEnabledProviders();
        var failures = new List<Exception>();

        foreach (var provider in enabledProviders) {
            try {
                return provider switch {
                    CepLookifyProviderEnum.ViaCep =>
                        await ViaCepProviderRequest.RequestAsync(
                            sanitizedZipCode,
                            _httpFactory,
                            cancellationToken),
                    CepLookifyProviderEnum.BrasilApi =>
                        await BrasilApiCepProviderRequest.RequestAsync(
                            sanitizedZipCode,
                            _httpFactory,
                            cancellationToken),
                    _ => throw new NotSupportedException($"Provider {provider} not supported")
                };
            }
            catch (Exception exception) {
                failures.Add(exception);
                _logger.LogError(
                    exception,
                    "Falha ao consultar o CEP {ZipCode} no provedor {Provider}.",
                    sanitizedZipCode,
                    provider);
            }
        }

        throw new InvalidOperationException(
            $"Não foi possível consultar o CEP {sanitizedZipCode} em nenhum provedor configurado.",
            new AggregateException(failures));
    }

    private string SanitizeZipCode(string zipCode)
    {
        var satiziedZipCode = new string(zipCode.Where(char.IsDigit).ToArray());

        if (satiziedZipCode.Length != 8)
            throw new ArgumentException(
                "Zip code must contain exactly 8 digits after sanitization.",
                nameof(zipCode));

        return satiziedZipCode;
    }
        

    private List<CepLookifyProviderEnum> GetEnabledProviders() =>
        _options.CepProviders
            .Where(provider => _options.GetProviderOptions(provider).Enabled)
            .ToList();
}
