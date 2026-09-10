using Lookify.Providers;
using Lookify.Providers.BrasilApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Bank;

internal sealed class BankLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : IBankLookifyService {

    private static readonly BrasilApiProvider _brasilApi = new();

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LookifyOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    public async Task<List<BankLookifyResultDto>> GetAllBanksAsync(
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            "bancos",
            provider => GetService(provider).GetAllBanksAsync(
                _httpFactory,
                cancellationToken));
    }

    public async Task<BankLookifyResultDto> GetBankByCodeAsync(
        int code,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            $"banco de código {code}",
            provider => GetService(provider).GetBankByCodeAsync(
                code,
                _httpFactory,
                cancellationToken));
    }

    private async Task<TResult> ExecuteAsync<TResult>(
        string operationDescription,
        Func<BankLookifyProviderEnum, Task<TResult>> request)
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

    private List<BankLookifyProviderEnum> GetEnabledProviders() =>
        _options.BankProviders
            .Where(provider => _options.GetProviderOptions(provider).IsEnabled)
            .ToList();

    private static IProvider GetProvider(BankLookifyProviderEnum provider) =>
        provider switch {
            BankLookifyProviderEnum.BrasilApi => _brasilApi,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };

    private static IBankProviderService GetService(BankLookifyProviderEnum provider) =>
        GetProvider(provider).Services
            .OfType<IBankProviderService>()
            .First();
}
