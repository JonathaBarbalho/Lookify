using System.Net;
using System.Text;
using Lookify.Cnpj;
using Lookify.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lookify.Tests.Cnpj;

public class CnpjLookifyServiceTests {

    private static CnpjLookifyService CreateService() =>
        new(
            httpFactory: null!,
            options: Options.Create(new Lookify.LookifyOptions()),
            logger: NullLogger.Instance);

    private static CnpjLookifyService CreateService(IHttpClientFactory httpFactory, Lookify.LookifyOptions options) =>
        new(httpFactory, Options.Create(options), NullLogger.Instance);

    private const string BrasilApiJson = """
        {
            "cnpj": "11222333000181",
            "razao_social": "Empresa Exemplo LTDA"
        }
        """;

    [Fact]
    public async Task ConsultAsync_QuandoCnpjVazio_LancaArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.ConsultAsync(""));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1122233300018")]
    public async Task ConsultAsync_QuandoCnpjComMenosDe14Digitos_LancaArgumentException(string cnpj)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.ConsultAsync(cnpj));
    }

    [Fact]
    public async Task ConsultAsync_QuandoCnpjComFormatacao_SanitizaEConsultaApenasDigitos()
    {
        var options = new Lookify.LookifyOptions();
        options.UpdateEnableCnpjProvider(
            false,
            CnpjLookifyProviderEnum.ReceitaWs,
            CnpjLookifyProviderEnum.Publica,
            CnpjLookifyProviderEnum.MinhaReceita);
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(BrasilApiJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        await service.ConsultAsync("11.222.333/0001-81");

        Assert.Equal(
            "https://brasilapi.com.br/api/cnpj/v1/11222333000181",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task ConsultAsync_QuandoPrimeiroProviderFalhaESegundoSucede_RetornaResultadoDoSegundoProvider()
    {
        var options = new Lookify.LookifyOptions();
        options.UpdateEnableCnpjProvider(
            false,
            CnpjLookifyProviderEnum.Publica,
            CnpjLookifyProviderEnum.MinhaReceita);
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.Host.Contains("brasilapi")
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("""{"cnpj":"11222333000181","nome":"Empresa Exemplo LTDA"}""", Encoding.UTF8, "application/json")
                });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.ConsultAsync("11222333000181");

        Assert.Equal("Empresa Exemplo LTDA", result.CompanyName);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task ConsultAsync_QuandoProviderDesabilitado_NuncaERequisitado()
    {
        var options = new Lookify.LookifyOptions();
        options.UpdateEnableCnpjProvider(false, CnpjLookifyProviderEnum.BrasilApi);
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("""{"cnpj":"11222333000181","nome":"Empresa"}""", Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        await service.ConsultAsync("11222333000181");

        Assert.Single(handler.Requests);
        Assert.Equal("receitaws.com.br", handler.Requests[0].RequestUri!.Host);
    }

    [Fact]
    public async Task ConsultAsync_QuandoTodosProvidersHabilitadosFalham_LancaInvalidOperationExceptionComAggregateException()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConsultAsync("11222333000181"));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Equal(4, aggregate.InnerExceptions.Count);
        Assert.Equal(4, handler.Requests.Count);
    }
}
