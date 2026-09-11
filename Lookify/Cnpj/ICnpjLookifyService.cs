namespace Lookify.Cnpj;

public interface ICnpjLookifyService {
    Task<CnpjLookifyResultDto> ConsultAsync(
        string cnpj,
        CancellationToken cancellationToken = default);
}
