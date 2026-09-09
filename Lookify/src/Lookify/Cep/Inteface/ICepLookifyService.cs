using Lookify.Cep.Dto;

namespace Lookify.Cep.Inteface;

public interface ICepLookifyService {

    Task<CepLookifyResult> ConsultAsync(
        string zipCode,
        CancellationToken cancellationToken = default);
}
