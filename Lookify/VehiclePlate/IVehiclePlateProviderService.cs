using Lookify.Providers;

namespace Lookify.VehiclePlate;

internal interface IVehiclePlateProviderService : IProviderService {

    Task<VehiclePlateLookifyResultDto> RequestAsync(
        string plate,
        string token,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
