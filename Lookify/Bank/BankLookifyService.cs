using Lookify.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Bank;

internal sealed class BankLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : IBankLookifyService {

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LookifyOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    public async Task<List<BankLookifyResultDto>> GetAllBanksAsync(
        CancellationToken cancellationToken = default)
    {
        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            "bancos",
            _logger,
            provider => GetService(provider).GetAllBanksAsync(
                _httpFactory,
                cancellationToken));
    }

    public async Task<BankLookifyResultDto> GetBankByCodeAsync(
        int code,
        CancellationToken cancellationToken = default)
    {
        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"banco de código {code}",
            _logger,
            provider => GetService(provider).GetBankByCodeAsync(
                code,
                _httpFactory,
                cancellationToken));
    }

    private List<BankLookifyProviderEnum> GetEnabledProviders() =>
        _options.BankProviders
            .Where(provider => _options.GetProviderOptions(provider).IsEnabled)
            .ToList();

    private static IProvider GetProvider(BankLookifyProviderEnum provider) =>
        provider switch {
            BankLookifyProviderEnum.BrasilApi => ProviderRegistry.BrasilApi,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };

    private static IBankProviderService GetService(BankLookifyProviderEnum provider) =>
        GetProvider(provider).Services
            .OfType<IBankProviderService>()
            .First();
}
