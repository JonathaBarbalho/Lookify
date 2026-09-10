using Lookify.Cep;
using Lookify.Cnpj;
using Lookify.VehiclePlate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify;

public sealed class LookifyService {

    public ICepLookifyService Cep { get; }

    public ICnpjLookifyService Cnpj { get; }

    public IVehiclePlateLookifyService VehiclePlate { get; }

    public LookifyService(
        IHttpClientFactory httpFactory,
        IOptions<LookifyOptions> options,
        ILogger<LookifyService> logger)
    {
        Cep = new CepLookifyService(
            httpFactory,
            options,
            logger);

        Cnpj = new CnpjLookifyService(
            httpFactory,
            options,
            logger);

        VehiclePlate = new VehiclePlateLookifyService(
            httpFactory,
            options,
            logger);
    }
}
