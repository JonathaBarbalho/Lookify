namespace Lookify.VehiclePlate;

public interface IVehiclePlateLookifyService {
    Task<VehiclePlateLookifyResultDto> ConsultAsync(
        string plate,
        CancellationToken cancellationToken = default);
}
