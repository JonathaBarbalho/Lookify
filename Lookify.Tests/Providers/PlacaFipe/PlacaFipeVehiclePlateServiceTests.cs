using System.Net;
using System.Text;
using Lookify.Providers.PlacaFipe;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.PlacaFipe;

public class PlacaFipeVehiclePlateServiceTests {

    private static PlacaFipeVehiclePlateService CreateService() =>
        new() {
            ProviderName = "PlacaFipe",
            BaseAddress = "https://api.placafipe.com.br/"
        };

    private const string FullJson = """
        {
            "codigo": 1,
            "placa": "ABC1234",
            "informacoes_veiculo": {
                "marca": "Fiat",
                "modelo": "Uno",
                "ano": "2015",
                "ano_modelo": 2015,
                "cor": "Branco",
                "chassi": "9BD1111",
                "motor": "1.0",
                "municipio": "São Paulo",
                "uf": "SP",
                "segmento": "Automóvel",
                "sub_segmento": "Hatch",
                "cilindradas": "1.0",
                "combustivel": "Flex"
            },
            "fipe": [
                {
                    "similaridade": "0.95",
                    "correspondencia": 0.9,
                    "marca": "Fiat",
                    "modelo": "Uno",
                    "ano_modelo": 2015,
                    "codigo_fipe": "001004-9",
                    "codigo_marca": "21",
                    "codigo_modelo": "5940",
                    "mes_referencia": "janeiro/2024",
                    "combustivel": "Flex",
                    "valor": "R$ 30.000,00",
                    "unidade_valor": "R$"
                }
            ]
        }
        """;

    [Fact]
    public async Task RequestAsync_QuandoCodigoDeSucesso_MapeiaCamposCompletos()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(FullJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync("ABC1234", "meu-token", new FakeHttpClientFactory(handler));

        Assert.Equal("Fiat", result.Brand);
        Assert.Equal(2015, result.ManufactureYear);
        Assert.Equal(2015, result.ModelYear);
        var match = Assert.Single(result.FipeMatches);
        Assert.Equal(0.95m, match.Similarity);
        Assert.Equal(0.9m, match.Correspondence);
        Assert.Equal("2015", match.ModelYear);
        Assert.Equal(
            "https://api.placafipe.com.br/getplacafipe",
            handler.LastRequest?.RequestUri?.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        var body = await handler.LastRequest!.Content!.ReadAsStringAsync();
        Assert.Contains("\"placa\":\"ABC1234\"", body);
        Assert.Contains("\"token\":\"meu-token\"", body);
    }

    [Fact]
    public async Task RequestAsync_QuandoCodigoDeSucessoDegradado_RetornaResultadoMesmoAssim()
    {
        const string json = """{ "codigo": 22, "placa": "ABC1234" }""";
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync("ABC1234", "token", new FakeHttpClientFactory(handler));

        Assert.Equal("ABC1234", result.Plate);
    }

    [Fact]
    public async Task RequestAsync_QuandoCodigoDeFalhaComMensagem_LancaInvalidOperationExceptionComMensagemDaApi()
    {
        const string json = """{ "codigo": 2, "msg": "Placa não encontrada" }""";
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RequestAsync("ABC1234", "token", new FakeHttpClientFactory(handler)));

        Assert.Equal("Placa não encontrada", exception.Message);
    }

    [Fact]
    public async Task RequestAsync_QuandoCodigoDeFalhaSemMensagem_LancaInvalidOperationExceptionComMensagemPadrao()
    {
        const string json = """{ "codigo": 2 }""";
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RequestAsync("ABC1234", "token", new FakeHttpClientFactory(handler)));

        Assert.Contains("PlacaFipe", exception.Message);
        Assert.Contains("ABC1234", exception.Message);
        Assert.Contains("2", exception.Message);
    }

    [Fact]
    public async Task RequestAsync_QuandoApiRetorna404_LancaHttpRequestExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.RequestAsync("ABC1234", "token", new FakeHttpClientFactory(handler)));

        Assert.Contains("PlacaFipe", exception.Message);
        Assert.Contains("ABC1234", exception.Message);
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task RequestAsync_QuandoCorpoInvalido_LancaInvalidOperationExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("não é um json válido", Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RequestAsync("ABC1234", "token", new FakeHttpClientFactory(handler)));

        Assert.Contains("PlacaFipe", exception.Message);
        Assert.Contains("ABC1234", exception.Message);
    }
}
