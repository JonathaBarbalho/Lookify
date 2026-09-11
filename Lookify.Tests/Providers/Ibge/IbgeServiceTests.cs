using System.Net;
using System.Text;
using Lookify.Providers.Ibge;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.Ibge;

public class IbgeServiceTests {

    private static IbgeService CreateService() =>
        new() {
            ProviderName = "Ibge",
            BaseAddress = "https://servicodados.ibge.gov.br/"
        };

    public class States {

        [Fact]
        public async Task GetStatesAsync_QuandoApiRespondeComSucesso_MapeiaListaComRegiaoEUrl()
        {
            const string json = """[{"id":35,"sigla":"SP","nome":"São Paulo","regiao":{"id":3,"sigla":"SE","nome":"Sudeste"}}]""";
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetStatesAsync(new FakeHttpClientFactory(handler));

            var state = Assert.Single(result);
            Assert.Equal("SP", state.Uf);
            Assert.Equal("Sudeste", state.RegionName);
            Assert.Equal(
                "https://servicodados.ibge.gov.br/api/v1/localidades/estados",
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
                "https://servicodados.ibge.gov.br/api/v1/localidades/estados/SP",
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

        private const string Json = """
            [
                {
                    "id": 3550308,
                    "nome": "São Paulo",
                    "microrregiao": {
                        "mesorregiao": {
                            "UF": {
                                "id": 35,
                                "sigla": "SP",
                                "nome": "São Paulo",
                                "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                            }
                        }
                    }
                }
            ]
            """;

        [Fact]
        public async Task GetCitiesByStateAsync_QuandoApiRespondeComSucesso_MapeiaAninhamentoProfundoEUrl()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetCitiesByStateAsync("SP", new FakeHttpClientFactory(handler));

            var city = Assert.Single(result);
            Assert.Equal("São Paulo", city.Name);
            Assert.Equal(35, city.StateId);
            Assert.Equal("SP", city.StateUf);
            Assert.Equal(3, city.RegionId);
            Assert.Equal("Sudeste", city.RegionName);
            Assert.Equal(
                "https://servicodados.ibge.gov.br/api/v1/localidades/estados/SP/municipios",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetAllCitiesAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetAllCitiesAsync(new FakeHttpClientFactory(handler));

            Assert.Single(result);
            Assert.Equal(
                "https://servicodados.ibge.gov.br/api/v1/localidades/municipios",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetCitiesByStateAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetCitiesByStateAsync("SP", new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetCitiesByStateAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("não é json", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
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

            var region = Assert.Single(result);
            Assert.Equal("Sudeste", region.Name);
            Assert.Equal("SE", region.Acronym);
            Assert.Equal(
                "https://servicodados.ibge.gov.br/api/v1/localidades/regioes",
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
