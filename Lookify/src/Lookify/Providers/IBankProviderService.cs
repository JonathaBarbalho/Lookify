using Lookify.Bank;

namespace Lookify.Providers;

internal interface IBankProviderService : IProviderService {

    Task<List<BankLookifyResultDto>> GetAllBanksAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);

    Task<BankLookifyResultDto> GetBankByCodeAsync(
        int code,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
