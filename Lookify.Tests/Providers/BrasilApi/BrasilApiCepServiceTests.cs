using System.Net;
using System.Text;
using Lookify.Providers.BrasilApi;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.BrasilApi;

public class BrasilApiCepServiceTests {

    private static BrasilApiCepService CreateService() =>
        new() {
            ProviderName = "BrasilApi",
            BaseAddress = "https://brasilapi.com.br/"
        };

    [Fact]
    public async Task RequestAsync_QuandoApiRespondeComSucesso_MapeiaJsonParaResultDto()
    {
        const string json = """
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
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync(
            "01001000",
            new FakeHttpClientFactory(handler));

        Assert.Equal("01001000", result.ZipCode);
        Assert.Equal("São Paulo", result.City);
        Assert.Equal("SP", result.State);
        Assert.Equal("Praça da Sé", result.Street);
        Assert.Equal("3550308", result.IbgeCityCode);
        Assert.Equal(-23.55m, result.Latitude);
        Assert.Equal(-46.63m, result.Longitude);
        Assert.Equal(
            "https://brasilapi.com.br/api/cep/v2/01001000",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task RequestAsync_QuandoApiRetorna404_LancaHttpRequestExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound) {
            Content = new StringContent("""{"message":"CEP não encontrado"}""", Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.RequestAsync("00000000", new FakeHttpClientFactory(handler)));

        Assert.Contains("BrasilApi", exception.Message);
        Assert.Contains("00000000", exception.Message);
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
            service.RequestAsync("01001000", new FakeHttpClientFactory(handler)));

        Assert.Contains("BrasilApi", exception.Message);
        Assert.Contains("01001000", exception.Message);
    }

    [Fact]
    public async Task RequestAsync_QuandoCoordenadasSaoNumericas_MapeiaLatLongComoDecimal()
    {
        const string json = """
            {
                "cep": "01001000",
                "state": "SP",
                "city": "São Paulo",
                "neighborhood": "Sé",
                "street": "Praça da Sé",
                "ibge": { "city": "3550308", "state": "35" },
                "location": { "coordinates": { "longitude": -46.63, "latitude": -23.55 } }
            }
            """;
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync(
            "01001000",
            new FakeHttpClientFactory(handler));

        Assert.Equal(-23.55m, result.Latitude);
        Assert.Equal(-46.63m, result.Longitude);
    }
}
