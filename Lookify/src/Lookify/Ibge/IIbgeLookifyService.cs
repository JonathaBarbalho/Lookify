namespace Lookify.Ibge;

public interface IIbgeLookifyService {

    Task<List<IbgeStateLookifyResultDto>> GetStatesAsync(
        CancellationToken cancellationToken = default);

    Task<IbgeStateLookifyResultDto> GetStateAsync(
        string uf,
        CancellationToken cancellationToken = default);

    Task<List<IbgeCityLookifyResultDto>> GetCitiesByStateAsync(
        string uf,
        CancellationToken cancellationToken = default);

    Task<List<IbgeCityLookifyResultDto>> GetAllCitiesAsync(
        CancellationToken cancellationToken = default);

    Task<List<IbgeRegionLookifyResultDto>> GetRegionsAsync(
        CancellationToken cancellationToken = default);
}
