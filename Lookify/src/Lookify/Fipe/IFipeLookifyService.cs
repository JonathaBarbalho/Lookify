namespace Lookify.Fipe;

public interface IFipeLookifyService {

    Task<List<FipeReferenceTableLookifyResultDto>> GetReferenceTablesAsync(
        CancellationToken cancellationToken = default);

    Task<List<FipeBrandLookifyResultDto>> GetBrandsAsync(
        FipeVehicleType vehicleType,
        int? referenceTable = null,
        CancellationToken cancellationToken = default);

    Task<List<FipeModelLookifyResultDto>> GetModelsAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        int? referenceTable = null,
        CancellationToken cancellationToken = default);

    Task<List<FipeModelYearLookifyResultDto>> GetModelYearsAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        string modelCode,
        int? referenceTable = null,
        CancellationToken cancellationToken = default);

    Task<FipeVehiclePriceLookifyResultDto> GetVehiclePriceAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        string modelCode,
        string yearCode,
        int? referenceTable = null,
        CancellationToken cancellationToken = default);

    Task<List<FipeVehiclePriceLookifyResultDto>> GetPriceByFipeCodeAsync(
        string fipeCode,
        int? referenceTable = null,
        CancellationToken cancellationToken = default);
}
