using System.Net;
using System.Text;
using Lookify.Tests.TestSupport;
using Lookify.VehiclePlate;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lookify.Tests.VehiclePlate;

public class VehiclePlateLookifyServiceTests {

    private static VehiclePlateLookifyService CreateService() =>
        new(
            httpFactory: null!,
            options: Options.Create(new Lookify.LookifyOptions()),
            logger: NullLogger.Instance);

    private static VehiclePlateLookifyService CreateService(IHttpClientFactory httpFactory, Lookify.LookifyOptions options) =>
        new(httpFactory, Options.Create(options), NullLogger.Instance);

    private const string SuccessJson = """
        { "codigo": 1, "placa": "ABC1234", "informacoes_veiculo": { "marca": "Fiat" }, "fipe": [] }
        """;

    [Fact]
    public async Task ConsultAsync_QuandoPlacaVazia_LancaArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.ConsultAsync(""));
    }

    [Theory]
    [InlineData("ABC1234")]
    [InlineData("ABC1D23")]
    public async Task ConsultAsync_QuandoFormatoValido_ConsultaComSucesso(string plate)
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(SuccessJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.ConsultAsync(plate);

        Assert.Equal("Fiat", result.Brand);
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("ABCD1234")]
    [InlineData("123ABCD")]
    public async Task ConsultAsync_QuandoFormatoInvalido_LancaArgumentException(string plate)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.ConsultAsync(plate));
    }

    [Fact]
    public async Task ConsultAsync_QuandoPlacaComMinusculasETraco_SanitizaAntesDeConsultar()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(SuccessJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        await service.ConsultAsync("abc-1234");

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        Assert.Contains("\"placa\":\"ABC1234\"", body);
    }

    [Fact]
    public async Task ConsultAsync_QuandoTokenConfigurado_RepassaTokenNoCorpoDaRequisicao()
    {
        var options = new Lookify.LookifyOptions();
        options.SetPlacaFipeToken("meu-token");
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(SuccessJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        await service.ConsultAsync("ABC1234");

        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        Assert.Contains("\"token\":\"meu-token\"", body);
    }

    [Fact]
    public async Task ConsultAsync_QuandoTodosProvidersHabilitadosFalham_LancaInvalidOperationExceptionComAggregateException()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConsultAsync("ABC1234"));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Single(aggregate.InnerExceptions);
    }
}
