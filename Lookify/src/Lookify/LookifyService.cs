using Lookify.Bank;
using Lookify.Cep;
using Lookify.Cnpj;
using Lookify.Fipe;
using Lookify.Holiday;
using Lookify.Ibge;
using Lookify.VehiclePlate;
using Lookify.Weather;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify;

public sealed class LookifyService {

    public ICepLookifyService Cep { get; }

    public ICnpjLookifyService Cnpj { get; }

    public IVehiclePlateLookifyService VehiclePlate { get; }

    public IFipeLookifyService Fipe { get; }

    public IIbgeLookifyService Ibge { get; }

    public IBankLookifyService Bank { get; }

    public IHolidayLookifyService Holiday { get; }

    public IWeatherLookifyService Weather { get; }

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

        Fipe = new FipeLookifyService(
            httpFactory,
            options,
            logger);

        Ibge = new IbgeLookifyService(
            httpFactory,
            options,
            logger);

        Bank = new BankLookifyService(
            httpFactory,
            options,
            logger);

        Holiday = new HolidayLookifyService(
            httpFactory,
            options,
            logger);

        Weather = new WeatherLookifyService(
            httpFactory,
            options,
            logger);
    }
}
