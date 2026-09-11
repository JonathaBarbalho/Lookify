using System.Net;
using System.Text;
using Lookify.Cep;
using Lookify.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lookify.Tests.Cep;

public class CepLookifyServiceTests {

    private static CepLookifyService CreateService() =>
        new(
            httpFactory: null!,
            options: Options.Create(new LookifyOptions()),
            logger: NullLogger.Instance);

    private static CepLookifyService CreateService(IHttpClientFactory httpFactory, LookifyOptions options) =>
        new(httpFactory, Options.Create(options), NullLogger.Instance);

    private const string ViaCepJson = """
        {
            "cep": "01001-000",
            "logradouro": "Praça da Sé",
            "bairro": "Sé",
            "localidade": "São Paulo",
            "uf": "SP",
            "ibge": "3550308",
            "erro": false
        }
        """;

    private const string BrasilApiJson = """
        {
            "cep": "01001000",
            "state": "SP",
            "city": "São Paulo",
            "neighborhood": "Sé",
            "street": "Praça da Sé",
            "ibge": { "city": "3550308", "state": "35" },
            "location": { "coordinates": { "longitude": "-46.63", "latitude": "-23.55" } }
        }
        """;

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public async Task ConsultAsync_QuandoCepInvalido_LancaArgumentException(string zipCode)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.ConsultAsync(zipCode));
    }

    [Fact]
    public async Task ConsultAsync_QuandoZipCodeComFormatacao_SanitizaEConsultaApenasDigitos()
    {
        var options = new LookifyOptions();
        options.UpdateEnableCepProvider(
            false,
            CepLookifyProviderEnum.BrasilApi,
            CepLookifyProviderEnum.OpenCep,
            CepLookifyProviderEnum.AwesomeApi);
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(ViaCepJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        await service.ConsultAsync("01001-000");

        Assert.Equal(
            "https://viacep.com.br/ws/01001000/json/",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task ConsultAsync_QuandoPrimeiroProviderFalhaESegundoSucede_RetornaResultadoDoSegundoProvider()
    {
        var options = new LookifyOptions();
        options.UpdateEnableCepProvider(
            false,
            CepLookifyProviderEnum.OpenCep,
            CepLookifyProviderEnum.AwesomeApi);
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.Host.Contains("viacep")
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(BrasilApiJson, Encoding.UTF8, "application/json")
                });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.ConsultAsync("01001000");

        Assert.Equal("São Paulo", result.City);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task ConsultAsync_QuandoProviderDesabilitado_NuncaERequisitado()
    {
        var options = new LookifyOptions();
        options.UpdateEnableCepProvider(false, CepLookifyProviderEnum.ViaCep);
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(BrasilApiJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        await service.ConsultAsync("01001000");

        Assert.Single(handler.Requests);
        Assert.Equal("brasilapi.com.br", handler.Requests[0].RequestUri!.Host);
    }

    [Fact]
    public async Task ConsultAsync_QuandoTodosProvidersHabilitadosFalham_LancaInvalidOperationExceptionComAggregateException()
    {
        var options = new LookifyOptions();
        var handler = new FakeHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConsultAsync("01001000"));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Equal(4, aggregate.InnerExceptions.Count);
        Assert.Equal(4, handler.Requests.Count);
    }
}
