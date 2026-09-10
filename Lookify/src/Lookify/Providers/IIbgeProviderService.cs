using Lookify.Ibge;

namespace Lookify.Providers;

internal interface IIbgeProviderService : IProviderService {

    Task<List<IbgeStateLookifyResultDto>> GetStatesAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);

    Task<IbgeStateLookifyResultDto> GetStateAsync(
        string uf,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);

    Task<List<IbgeCityLookifyResultDto>> GetCitiesByStateAsync(
        string uf,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);

    Task<List<IbgeCityLookifyResultDto>> GetAllCitiesAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);

    Task<List<IbgeRegionLookifyResultDto>> GetRegionsAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
