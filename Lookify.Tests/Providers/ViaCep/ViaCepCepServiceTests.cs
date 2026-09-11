using System.Net;
using System.Text;
using Lookify.Providers.ViaCep;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.ViaCep;

public class ViaCepCepServiceTests {

    private static ViaCepCepService CreateService() =>
        new() {
            ProviderName = "ViaCep",
            BaseAddress = "https://viacep.com.br/"
        };

    [Fact]
    public async Task RequestAsync_QuandoApiRespondeComSucesso_MapeiaJsonParaResultDto()
    {
        const string json = """
            {
                "cep": "01001-000",
                "logradouro": "Praça da Sé",
                "complemento": "lado ímpar",
                "bairro": "Sé",
                "localidade": "São Paulo",
                "uf": "SP",
                "ibge": "3550308",
                "erro": false
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
        Assert.Equal("lado ímpar", result.Complement);
        Assert.Equal("Sé", result.Neighborhood);
        Assert.Equal("São Paulo", result.City);
        Assert.Equal("SP", result.State);
        Assert.Equal("3550308", result.IbgeCityCode);
        Assert.Equal(
            "https://viacep.com.br/ws/01001000/json/",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task RequestAsync_QuandoApiRetorna404_LancaHttpRequestExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound) {
            Content = new StringContent("não encontrado", Encoding.UTF8, "text/plain")
        });
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.RequestAsync("00000000", new FakeHttpClientFactory(handler)));

        Assert.Contains("ViaCep", exception.Message);
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

        Assert.Contains("ViaCep", exception.Message);
        Assert.Contains("01001000", exception.Message);
    }

    [Fact]
    public async Task RequestAsync_QuandoErroTrueNoCorpo_LancaInvalidOperationExceptionComProviderEIdentificador()
    {
        const string json = """{ "erro": true }""";
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RequestAsync("00000000", new FakeHttpClientFactory(handler)));

        Assert.Equal("ViaCep não encontrou o CEP 00000000.", exception.Message);
    }
}
