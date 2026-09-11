using System.Net;
using System.Text;
using Lookify.Holiday;
using Lookify.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lookify.Tests.Holiday;

public class HolidayLookifyServiceTests {

    private static HolidayLookifyService CreateService(IHttpClientFactory httpFactory, Lookify.LookifyOptions options) =>
        new(httpFactory, Options.Create(options), NullLogger.Instance);

    private const string BrasilApiJson = """
        [
            { "date": "2024-01-01", "name": "Confraternização mundial", "type": "national" }
        ]
        """;

    private const string NagerDateJson = """
        [
            { "date": "2024-01-01", "name": "New Year's Day", "localName": "Confraternização Universal", "types": ["Public"] }
        ]
        """;

    [Fact]
    public async Task GetHolidaysAsync_QuandoPrimeiroProviderSucede_RetornaResultadoDoPrimeiro()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(BrasilApiJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.GetHolidaysAsync(2024);

        Assert.Single(result);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task GetHolidaysAsync_QuandoPrimeiroProviderFalhaESegundoSucede_RetornaResultadoDoSegundoProvider()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath.Contains("feriados")
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(NagerDateJson, Encoding.UTF8, "application/json")
                });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.GetHolidaysAsync(2024);

        Assert.Single(result);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task GetHolidaysAsync_QuandoProviderDesabilitado_NuncaERequisitado()
    {
        var options = new Lookify.LookifyOptions();
        options.UpdateEnableHolidayProvider(false, HolidayLookifyProviderEnum.BrasilApi);
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(NagerDateJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        await service.GetHolidaysAsync(2024);

        Assert.Single(handler.Requests);
        Assert.Contains("date.nager.at", handler.Requests[0].RequestUri!.Host);
    }

    [Fact]
    public async Task GetHolidaysAsync_QuandoTodosProvidersHabilitadosFalham_LancaInvalidOperationExceptionComAggregateException()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetHolidaysAsync(2024));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Equal(2, aggregate.InnerExceptions.Count);
        Assert.Equal(2, handler.Requests.Count);
    }
}
