using System.Net;
using System.Text;
using Lookify.Providers.BrasilApi;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.BrasilApi;

public class BrasilApiIbgeServiceTests {

    private static BrasilApiIbgeService CreateService() =>
        new() {
            ProviderName = "BrasilApi",
            BaseAddress = "https://brasilapi.com.br/"
        };

    public class States {

        [Fact]
        public async Task GetStatesAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
        {
            const string json = """[{"id":35,"sigla":"SP","nome":"São Paulo","regiao":{"id":3,"sigla":"SE","nome":"Sudeste"}}]""";
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetStatesAsync(new FakeHttpClientFactory(handler));

            Assert.Single(result);
            Assert.Equal(
                "https://brasilapi.com.br/api/ibge/uf/v1",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetStateAsync_QuandoApiRespondeComSucesso_MapeiaEUrl()
        {
            const string json = """{"id":35,"sigla":"SP","nome":"São Paulo","regiao":{"id":3,"sigla":"SE","nome":"Sudeste"}}""";
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetStateAsync("SP", new FakeHttpClientFactory(handler));

            Assert.Equal("São Paulo", result.Name);
            Assert.Equal(
                "https://brasilapi.com.br/api/ibge/uf/v1/SP",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetStatesAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetStatesAsync(new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetStatesAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("não é json", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetStatesAsync(new FakeHttpClientFactory(handler)));
        }
    }

    public class Cities {

        [Fact]
        public async Task GetCitiesByStateAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
        {
            const string json = """[{"nome":"São Paulo","codigo_ibge":"3550308"}]""";
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetCitiesByStateAsync("SP", new FakeHttpClientFactory(handler));

            var city = Assert.Single(result);
            Assert.Equal(3550308, city.Id);
            Assert.Equal("São Paulo", city.Name);
            Assert.Equal("SP", city.StateUf);
            Assert.Equal(
                "https://brasilapi.com.br/api/ibge/municipios/v1/SP",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetCitiesByStateAsync_QuandoCodigoIbgeNaoNumerico_IdFicaNulo()
        {
            const string json = """[{"nome":"São Paulo","codigo_ibge":"não-numerico"}]""";
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetCitiesByStateAsync("SP", new FakeHttpClientFactory(handler));

            Assert.Null(result[0].Id);
        }

        [Fact]
        public async Task GetAllCitiesAsync_SempreLancaNotSupportedExceptionSemChamadaHttp()
        {
            var handler = new FakeHttpMessageHandler(
                _ => throw new InvalidOperationException("Não deveria ter feito requisição HTTP."));
            var service = CreateService();

            var exception = await Assert.ThrowsAsync<NotSupportedException>(() =>
                service.GetAllCitiesAsync(new FakeHttpClientFactory(handler)));

            Assert.Contains("BrasilApi", exception.Message);
            Assert.Empty(handler.Requests);
        }

        [Fact]
        public async Task GetCitiesByStateAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetCitiesByStateAsync("SP", new FakeHttpClientFactory(handler)));
        }
    }

    public class Regions {

        [Fact]
        public async Task GetRegionsAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
        {
            const string json = """[{"id":3,"sigla":"SE","nome":"Sudeste"}]""";
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetRegionsAsync(new FakeHttpClientFactory(handler));

            Assert.Single(result);
            Assert.Equal(
                "https://brasilapi.com.br/api/ibge/regioes/v1",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetRegionsAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetRegionsAsync(new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetRegionsAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("não é json", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetRegionsAsync(new FakeHttpClientFactory(handler)));
        }
    }
}
