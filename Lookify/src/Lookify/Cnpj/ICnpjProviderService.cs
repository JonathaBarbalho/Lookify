using Lookify.Providers;

namespace Lookify.Cnpj;

internal interface ICnpjProviderService : IProviderService {

    Task<CnpjLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
