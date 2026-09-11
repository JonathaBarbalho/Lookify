namespace Lookify.Cep;

public interface ICepLookifyService {

    Task<CepLookifyResultDto> ConsultAsync(
        string zipCode,
        CancellationToken cancellationToken = default);
}
