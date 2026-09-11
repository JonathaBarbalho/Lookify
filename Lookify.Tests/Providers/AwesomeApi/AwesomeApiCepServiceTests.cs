using System.Net;
using System.Text;
using Lookify.Providers.AwesomeApi;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.AwesomeApi;

public class AwesomeApiCepServiceTests {

    private static AwesomeApiCepService CreateService() =>
        new() {
            ProviderName = "AwesomeApi",
            BaseAddress = "https://cep.awesomeapi.com.br/"
        };

    [Fact]
    public async Task RequestAsync_QuandoApiRespondeComSucesso_MapeiaJsonParaResultDto()
    {
        const string json = """
            {
                "cep": "01001-000",
                "address": "Praça da Sé",
                "district": "Sé",
                "city": "São Paulo",
                "state": "SP",
                "city_ibge": "3550308",
                "ddd": "11",
                "lat": -23.55,
                "lng": -46.63
            }
            """;
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync(
            "01001000",
            new FakeHttpClientFactory(handler));

        Assert.Equal("01001-000", result.ZipCode);
        Assert.Equal("Praça da Sé", result.Street);
        Assert.Null(result.Complement);
        Assert.Equal("Sé", result.Neighborhood);
        Assert.Equal("São Paulo", result.City);
        Assert.Equal("SP", result.State);
        Assert.Equal("3550308", result.IbgeCityCode);
        Assert.Equal("11", result.Ddd);
        Assert.Equal(-23.55m, result.Latitude);
        Assert.Equal(-46.63m, result.Longitude);
        Assert.Equal(
            "https://cep.awesomeapi.com.br/json/01001000",
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

        Assert.Contains("AwesomeApi", exception.Message);
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

        Assert.Contains("AwesomeApi", exception.Message);
        Assert.Contains("01001000", exception.Message);
    }

    [Theory]
    [InlineData("-23.55", "-46.63")]
    [InlineData("\"-23.55\"", "\"-46.63\"")]
    public async Task RequestAsync_QuandoLatLngVariamFormato_MapeiaComoDecimal(string latJson, string lngJson)
    {
        var json = $$"""
            {
                "cep": "01001000",
                "address": "Praça da Sé",
                "district": "Sé",
                "city": "São Paulo",
                "state": "SP",
                "city_ibge": "3550308",
                "ddd": "11",
                "lat": {{latJson}},
                "lng": {{lngJson}}
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
