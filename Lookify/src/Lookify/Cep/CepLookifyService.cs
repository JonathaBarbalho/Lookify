using Lookify.Providers;
using Lookify.Providers.AwesomeApi;
using Lookify.Providers.BrasilApi;
using Lookify.Providers.OpenCep;
using Lookify.Providers.ViaCep;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Cep;

internal sealed class CepLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : ICepLookifyService {

    private static readonly ViaCepProvider _viaCep = new();
    private static readonly BrasilApiProvider _brasilApi = new();
    private static readonly OpenCepProvider _openCep = new();
    private static readonly AwesomeApiProvider _awesomeApi = new();

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

        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"o CEP {sanitizedZipCode}",
            _logger,
            provider => GetService(provider).RequestAsync(
                sanitizedZipCode,
                _httpFactory,
                cancellationToken));
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
            .Where(provider => _options.GetProviderOptions(provider).IsEnabled)
            .ToList();

    private static IProvider GetProvider(CepLookifyProviderEnum provider) =>
        provider switch {
            CepLookifyProviderEnum.ViaCep => _viaCep,
            CepLookifyProviderEnum.BrasilApi => _brasilApi,
            CepLookifyProviderEnum.OpenCep => _openCep,
            CepLookifyProviderEnum.AwesomeApi => _awesomeApi,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };

    private static ICepProviderService GetService(CepLookifyProviderEnum provider) =>
        GetProvider(provider).Services
            .OfType<ICepProviderService>()
            .First();
}
