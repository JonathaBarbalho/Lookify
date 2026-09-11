using Lookify.Bank;
using Lookify.Cep;
using Lookify.Cnpj;
using Lookify.Fipe;
using Lookify.Holiday;
using Lookify.Ibge;
using Lookify.VehiclePlate;
using Lookify.Weather;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lookify.Tests;

public class LookifyServiceTests {

    [Fact]
    public void Constructor_QuandoInstanciado_ExpoeTodosOsSubServicosNaoNulos()
    {
        var service = new LookifyService(
            httpFactory: null!,
            Options.Create(new LookifyOptions()),
            NullLogger<LookifyService>.Instance);

        Assert.IsType<CepLookifyService>(service.Cep);
        Assert.IsType<CnpjLookifyService>(service.Cnpj);
        Assert.IsType<VehiclePlateLookifyService>(service.VehiclePlate);
        Assert.IsType<FipeLookifyService>(service.Fipe);
        Assert.IsType<IbgeLookifyService>(service.Ibge);
        Assert.IsType<BankLookifyService>(service.Bank);
        Assert.IsType<HolidayLookifyService>(service.Holiday);
        Assert.IsType<WeatherLookifyService>(service.Weather);
    }
}
