using Lookify.VehiclePlate;

namespace Lookify.Providers;

internal interface IVehiclePlateProviderService : IProviderService {

    Task<VehiclePlateLookifyResultDto> RequestAsync(
        string plate,
        string token,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
