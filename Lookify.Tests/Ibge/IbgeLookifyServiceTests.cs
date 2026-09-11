using System.Net;
using System.Text;
using Lookify.Ibge;
using Lookify.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lookify.Tests.Ibge;

public class IbgeLookifyServiceTests {

    private static IbgeLookifyService CreateService() =>
        new(
            httpFactory: null!,
            options: Options.Create(new Lookify.LookifyOptions()),
            logger: NullLogger.Instance);

    private static IbgeLookifyService CreateService(IHttpClientFactory httpFactory, Lookify.LookifyOptions options) =>
        new(httpFactory, Options.Create(options), NullLogger.Instance);

    [Theory]
    [InlineData("")]
    [InlineData("S")]
    [InlineData("SPP")]
    [InlineData("12")]
    public async Task GetStateAsync_QuandoUfInvalida_LancaArgumentException(string uf)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetStateAsync(uf));
    }

    [Fact]
    public async Task GetStateAsync_QuandoUfMinuscula_NormalizaParaMaiuscula()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("""{"id":35,"sigla":"SP","nome":"São Paulo"}""", Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        await service.GetStateAsync("sp");

        Assert.Contains("/SP", handler.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetCitiesByStateAsync_QuandoUfInvalida_LancaArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetCitiesByStateAsync(""));
    }

    [Fact]
    public async Task GetStatesAsync_QuandoPrimeiroProviderFalhaESegundoSucede_RetornaResultadoDoSegundoProvider()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.Host.Contains("servicodados")
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("""[{"id":35,"sigla":"SP","nome":"São Paulo"}]""", Encoding.UTF8, "application/json")
                });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.GetStatesAsync();

        Assert.Single(result);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task GetRegionsAsync_QuandoProviderDesabilitado_NuncaERequisitado()
    {
        var options = new Lookify.LookifyOptions();
        options.UpdateEnableIbgeProvider(false, IbgeLookifyProviderEnum.Ibge);
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        await service.GetRegionsAsync();

        Assert.Single(handler.Requests);
        Assert.Contains("brasilapi", handler.Requests[0].RequestUri!.Host);
    }

    [Fact]
    public async Task GetRegionsAsync_QuandoTodosProvidersHabilitadosFalham_LancaInvalidOperationExceptionComAggregateException()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetRegionsAsync());

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Equal(2, aggregate.InnerExceptions.Count);
    }
}
