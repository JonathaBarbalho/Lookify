namespace Lookify.Bank;

public interface IBankLookifyService {

    Task<List<BankLookifyResultDto>> GetAllBanksAsync(
        CancellationToken cancellationToken = default);

    Task<BankLookifyResultDto> GetBankByCodeAsync(
        int code,
        CancellationToken cancellationToken = default);
}
