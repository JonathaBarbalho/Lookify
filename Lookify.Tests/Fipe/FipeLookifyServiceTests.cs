using System.Net;
using System.Text;
using Lookify.Fipe;
using Lookify.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lookify.Tests.Fipe;

public class FipeLookifyServiceTests {

    private static FipeLookifyService CreateService() =>
        new(
            httpFactory: null!,
            options: Options.Create(new Lookify.LookifyOptions()),
            logger: NullLogger.Instance);

    private static FipeLookifyService CreateService(IHttpClientFactory httpFactory, Lookify.LookifyOptions options) =>
        new(httpFactory, Options.Create(options), NullLogger.Instance);

    [Fact]
    public async Task GetModelsAsync_QuandoBrandCodeVazio_LancaArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetModelsAsync(FipeVehicleType.Cars, ""));
    }

    [Theory]
    [InlineData("", "modelo")]
    [InlineData("marca", "")]
    public async Task GetModelYearsAsync_QuandoBrandOuModelVazio_LancaArgumentException(
        string brandCode,
        string modelCode)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetModelYearsAsync(FipeVehicleType.Cars, brandCode, modelCode));
    }

    [Theory]
    [InlineData("", "modelo", "ano")]
    [InlineData("marca", "", "ano")]
    [InlineData("marca", "modelo", "")]
    public async Task GetVehiclePriceAsync_QuandoQualquerCodigoVazio_LancaArgumentException(
        string brandCode,
        string modelCode,
        string yearCode)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetVehiclePriceAsync(FipeVehicleType.Cars, brandCode, modelCode, yearCode));
    }

    [Fact]
    public async Task GetPriceByFipeCodeAsync_QuandoFipeCodeVazio_LancaArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetPriceByFipeCodeAsync(""));
    }

    [Fact]
    public async Task GetReferenceTablesAsync_QuandoPrimeiroProviderFalhaESegundoSucede_RetornaResultadoDoSegundoProvider()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.Host.Contains("brasilapi")
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("""[{"code":202401,"month":"janeiro/2024"}]""", Encoding.UTF8, "application/json")
                });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.GetReferenceTablesAsync();

        Assert.Single(result);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task GetReferenceTablesAsync_QuandoProviderDesabilitado_NuncaERequisitado()
    {
        var options = new Lookify.LookifyOptions();
        options.UpdateEnableFipeProvider(false, FipeLookifyProviderEnum.BrasilApi);
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        await service.GetReferenceTablesAsync();

        Assert.Single(handler.Requests);
        Assert.Contains("parallelum", handler.Requests[0].RequestUri!.Host);
    }

    [Fact]
    public async Task GetReferenceTablesAsync_QuandoTodosProvidersHabilitadosFalham_LancaInvalidOperationExceptionComAggregateException()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetReferenceTablesAsync());

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Equal(2, aggregate.InnerExceptions.Count);
    }
}
