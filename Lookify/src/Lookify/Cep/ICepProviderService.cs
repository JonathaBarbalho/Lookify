using Lookify.Providers;

namespace Lookify.Cep;

internal interface ICepProviderService : IProviderService {

    Task<CepLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
