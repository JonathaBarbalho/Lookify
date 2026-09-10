using Lookify.Providers;

namespace Lookify.Fipe;

internal interface IFipeProviderService : IProviderService {

    Task<List<FipeReferenceTableLookifyResultDto>> GetReferenceTablesAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);

    Task<List<FipeBrandLookifyResultDto>> GetBrandsAsync(
        FipeVehicleType vehicleType,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);

    Task<List<FipeModelLookifyResultDto>> GetModelsAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);

    Task<List<FipeModelYearLookifyResultDto>> GetModelYearsAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        string modelCode,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);

    Task<FipeVehiclePriceLookifyResultDto> GetVehiclePriceAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        string modelCode,
        string yearCode,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);

    Task<List<FipeVehiclePriceLookifyResultDto>> GetPriceByFipeCodeAsync(
        string fipeCode,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
