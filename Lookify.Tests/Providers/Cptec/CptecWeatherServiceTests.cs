using System.Net;
using System.Text;
using Lookify.Providers.Cptec;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.Cptec;

public class CptecWeatherServiceTests {

    private static CptecWeatherService CreateService() =>
        new() {
            ProviderName = "Cptec",
            BaseAddress = "https://brasilapi.com.br/"
        };

    private const string ForecastJson = """
        {
            "cidade": "São Paulo",
            "estado": "SP",
            "clima": [
                { "data": "2024-01-01", "condicao": "c", "condicao_desc": "Chuva", "min": 18.0, "max": 25.0, "indice_uv": 7.0 }
            ]
        }
        """;

    [Fact]
    public async Task GetForecastByCoordinatesAsync_SempreLancaNotSupportedExceptionSemChamadaHttp()
    {
        var handler = new FakeHttpMessageHandler(
            _ => throw new InvalidOperationException("Não deveria ter feito requisição HTTP."));
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<NotSupportedException>(() =>
            service.GetForecastByCoordinatesAsync(-23.55m, -46.63m, null, new FakeHttpClientFactory(handler)));

        Assert.Contains("Cptec", exception.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task GetForecastByCityNameAsync_QuandoEstadoNulo_UsaPrimeiraCidadeEChamaPrevisao()
    {
        const string cityJson = """[{"nome":"São Paulo","id":244,"estado":"SP"}]""";
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath.Contains("/cidade/")
                ? new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(cityJson, Encoding.UTF8, "application/json")
                }
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(ForecastJson, Encoding.UTF8, "application/json")
                });
        var service = CreateService();

        var result = await service.GetForecastByCityNameAsync("São Paulo", null, null, new FakeHttpClientFactory(handler));

        Assert.Equal("São Paulo", result[0].City);
        Assert.Equal("Chuva", result[0].ConditionDescription);
        Assert.Equal(
            "https://brasilapi.com.br/api/cptec/v1/clima/previsao/244",
            handler.Requests[1].RequestUri!.ToString());
    }

    [Fact]
    public async Task GetForecastByCityNameAsync_QuandoEstadoInformado_FiltraPorEstado()
    {
        const string cityJson = """
            [
                {"nome":"Springfield","id":1,"estado":"RJ"},
                {"nome":"Springfield","id":2,"estado":"SP"}
            ]
            """;
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath.Contains("/cidade/")
                ? new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(cityJson, Encoding.UTF8, "application/json")
                }
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(ForecastJson, Encoding.UTF8, "application/json")
                });
        var service = CreateService();

        await service.GetForecastByCityNameAsync("Springfield", "SP", null, new FakeHttpClientFactory(handler));

        Assert.Equal(
            "https://brasilapi.com.br/api/cptec/v1/clima/previsao/2",
            handler.Requests[1].RequestUri!.ToString());
    }

    [Fact]
    public async Task GetForecastByCityNameAsync_QuandoDiasInformados_AdicionaSegmentoNaUrl()
    {
        const string cityJson = """[{"nome":"São Paulo","id":244,"estado":"SP"}]""";
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath.Contains("/cidade/")
                ? new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(cityJson, Encoding.UTF8, "application/json")
                }
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(ForecastJson, Encoding.UTF8, "application/json")
                });
        var service = CreateService();

        await service.GetForecastByCityNameAsync("São Paulo", null, 5, new FakeHttpClientFactory(handler));

        Assert.Equal(
            "https://brasilapi.com.br/api/cptec/v1/clima/previsao/244/5",
            handler.Requests[1].RequestUri!.ToString());
    }

    [Fact]
    public async Task GetForecastByCityNameAsync_QuandoCidadeNaoEncontrada_LancaInvalidOperationException()
    {
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath.Contains("/cidade/")
                ? new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("[]", Encoding.UTF8, "application/json")
                }
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(ForecastJson, Encoding.UTF8, "application/json")
                });
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetForecastByCityNameAsync("Cidade Inexistente", null, null, new FakeHttpClientFactory(handler)));

        Assert.Contains("Cidade Inexistente", exception.Message);
    }

    [Fact]
    public async Task GetForecastByCityNameAsync_QuandoCorpoDeCidadeInvalido_LancaInvalidOperationException()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("não é json", Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetForecastByCityNameAsync("São Paulo", null, null, new FakeHttpClientFactory(handler)));
    }

    [Fact]
    public async Task GetForecastByCityNameAsync_QuandoCorpoDePrevisaoInvalido_LancaInvalidOperationException()
    {
        const string cityJson = """[{"nome":"São Paulo","id":244,"estado":"SP"}]""";
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath.Contains("/cidade/")
                ? new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(cityJson, Encoding.UTF8, "application/json")
                }
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("não é json", Encoding.UTF8, "application/json")
                });
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetForecastByCityNameAsync("São Paulo", null, null, new FakeHttpClientFactory(handler)));
    }
}
