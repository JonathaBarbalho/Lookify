using System.Net;
using System.Text;
using Lookify.Providers.OpenMeteo;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.OpenMeteo;

public class OpenMeteoWeatherServiceTests {

    private static OpenMeteoWeatherService CreateService() =>
        new() {
            ProviderName = "OpenMeteo",
            BaseAddress = "https://api.open-meteo.com/"
        };

    private const string ForecastJson = """
        {
            "daily": {
                "time": ["2024-01-01", "2024-01-02"],
                "weather_code": [0, 9999],
                "temperature_2m_max": [30.5, 28.0],
                "temperature_2m_min": [20.1, 19.0],
                "precipitation_probability_max": [10, 50],
                "uv_index_max": [8.5, 6.0]
            }
        }
        """;

    public class ByCoordinates {

        [Fact]
        public async Task GetForecastByCoordinatesAsync_QuandoApiRespondeComSucesso_MapeiaCamposEUrl()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(ForecastJson, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetForecastByCoordinatesAsync(-23.55m, -46.63m, null, new FakeHttpClientFactory(handler));

            Assert.Equal(2, result.Count);
            Assert.Equal(new DateOnly(2024, 1, 1), result[0].Date);
            Assert.Equal(30.5m, result[0].MaxTemperature);
            Assert.Equal("Céu limpo", result[0].ConditionDescription);
            Assert.Null(result[1].ConditionDescription);
            var url = handler.LastRequest!.RequestUri!.ToString();
            Assert.Contains("latitude=-23.55", url);
            Assert.Contains("longitude=-46.63", url);
            Assert.DoesNotContain("forecast_days", url);
        }

        [Fact]
        public async Task GetForecastByCoordinatesAsync_QuandoDiasInformados_AdicionaForecastDaysNaUrl()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(ForecastJson, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await service.GetForecastByCoordinatesAsync(-23.55m, -46.63m, 3, new FakeHttpClientFactory(handler));

            Assert.Contains("forecast_days=3", handler.LastRequest!.RequestUri!.ToString());
        }

        [Fact]
        public async Task GetForecastByCoordinatesAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetForecastByCoordinatesAsync(-23.55m, -46.63m, null, new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetForecastByCoordinatesAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("não é json", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetForecastByCoordinatesAsync(-23.55m, -46.63m, null, new FakeHttpClientFactory(handler)));
        }
    }

    public class ByCityName {

        private static HttpResponseMessage Route(
            HttpRequestMessage request,
            string geocodingJson,
            string forecastJson) =>
            request.RequestUri!.Host.Contains("geocoding")
                ? new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(geocodingJson, Encoding.UTF8, "application/json")
                }
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(forecastJson, Encoding.UTF8, "application/json")
                };

        [Fact]
        public async Task GetForecastByCityNameAsync_QuandoEstadoNulo_UsaPrimeiroResultadoDaGeocodificacao()
        {
            const string geocodingJson = """
                { "results": [ { "name": "São Paulo", "latitude": -23.55, "longitude": -46.63, "admin1": "São Paulo" } ] }
                """;
            var handler = new FakeHttpMessageHandler(request => Route(request, geocodingJson, ForecastJson));
            var service = CreateService();

            var result = await service.GetForecastByCityNameAsync("São Paulo", null, null, new FakeHttpClientFactory(handler));

            Assert.Equal("São Paulo", result[0].City);
            Assert.Equal("SP", result[0].State);
            Assert.Contains("count=1", handler.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task GetForecastByCityNameAsync_QuandoEstadoInformado_FiltraPorAdmin1()
        {
            const string geocodingJson = """
                {
                    "results": [
                        { "name": "Springfield", "latitude": 1, "longitude": 1, "admin1": "Rio de Janeiro" },
                        { "name": "Springfield", "latitude": -23.55, "longitude": -46.63, "admin1": "São Paulo" }
                    ]
                }
                """;
            var handler = new FakeHttpMessageHandler(request => Route(request, geocodingJson, ForecastJson));
            var service = CreateService();

            var result = await service.GetForecastByCityNameAsync("Springfield", "sp", null, new FakeHttpClientFactory(handler));

            Assert.Equal("SP", result[0].State);
            Assert.Contains("count=20", handler.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task GetForecastByCityNameAsync_QuandoCidadeNaoEncontrada_LancaInvalidOperationException()
        {
            const string geocodingJson = """{ "results": [] }""";
            var handler = new FakeHttpMessageHandler(request => Route(request, geocodingJson, ForecastJson));
            var service = CreateService();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetForecastByCityNameAsync("Cidade Inexistente", null, null, new FakeHttpClientFactory(handler)));

            Assert.Contains("Cidade Inexistente", exception.Message);
        }

        [Fact]
        public async Task GetForecastByCityNameAsync_QuandoEstadoNaoBateComNenhumResultado_LancaInvalidOperationExceptionComEstadoNaMensagem()
        {
            const string geocodingJson = """
                { "results": [ { "name": "Cidade", "latitude": 1, "longitude": 1, "admin1": "Bahia" } ] }
                """;
            var handler = new FakeHttpMessageHandler(request => Route(request, geocodingJson, ForecastJson));
            var service = CreateService();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetForecastByCityNameAsync("Cidade", "sp", null, new FakeHttpClientFactory(handler)));

            Assert.Contains("Cidade", exception.Message);
            Assert.Contains("sp", exception.Message);
        }

        [Fact]
        public async Task GetForecastByCityNameAsync_QuandoCorpoDeGeocodificacaoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(request =>
                request.RequestUri!.Host.Contains("geocoding")
                    ? new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent("não é json", Encoding.UTF8, "application/json")
                    }
                    : new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent(ForecastJson, Encoding.UTF8, "application/json")
                    });
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetForecastByCityNameAsync("São Paulo", null, null, new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetForecastByCityNameAsync_QuandoCorpoDePrevisaoInvalido_LancaInvalidOperationException()
        {
            const string geocodingJson = """
                { "results": [ { "name": "São Paulo", "latitude": -23.55, "longitude": -46.63, "admin1": "São Paulo" } ] }
                """;
            var handler = new FakeHttpMessageHandler(request => Route(request, geocodingJson, "não é json"));
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetForecastByCityNameAsync("São Paulo", null, null, new FakeHttpClientFactory(handler)));
        }
    }
}
