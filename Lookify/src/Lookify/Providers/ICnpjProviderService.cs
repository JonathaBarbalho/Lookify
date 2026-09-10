using Lookify.Cnpj;

namespace Lookify.Providers;

internal interface ICnpjProviderService : IProviderService {

    Task<CnpjLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
