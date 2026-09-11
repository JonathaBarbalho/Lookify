using System.Net;
using System.Text;
using Lookify.Fipe;
using Lookify.Providers.BrasilApi;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.BrasilApi;

public class BrasilApiFipeServiceTests {

    private static BrasilApiFipeService CreateService() =>
        new() {
            ProviderName = "BrasilApi",
            BaseAddress = "https://brasilapi.com.br/"
        };

    public class ReferenceTables {

        [Fact]
        public async Task GetReferenceTablesAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("""[{"codigo":322,"mes":"janeiro/2024"}]""", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetReferenceTablesAsync(new FakeHttpClientFactory(handler));

            var table = Assert.Single(result);
            Assert.Equal(322, table.Code);
            Assert.Equal("janeiro/2024", table.Month);
            Assert.Equal(
                "https://brasilapi.com.br/api/fipe/tabelas/v1",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetReferenceTablesAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetReferenceTablesAsync(new FakeHttpClientFactory(handler)));

            Assert.Contains("BrasilApi", exception.Message);
        }

        [Fact]
        public async Task GetReferenceTablesAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("não é json", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetReferenceTablesAsync(new FakeHttpClientFactory(handler)));

            Assert.Contains("BrasilApi", exception.Message);
        }
    }

    public class Brands {

        [Theory]
        [InlineData(FipeVehicleType.Cars, "carros")]
        [InlineData(FipeVehicleType.Motorcycles, "motos")]
        [InlineData(FipeVehicleType.Trucks, "caminhoes")]
        public async Task GetBrandsAsync_QuandoTipoDeVeiculo_UsaSegmentoCorretoNaUrl(
            FipeVehicleType vehicleType,
            string segmentoEsperado)
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("""[{"nome":"Fiat","valor":"21"}]""", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetBrandsAsync(vehicleType, null, new FakeHttpClientFactory(handler));

            Assert.Equal("21", result[0].Code);
            Assert.Equal("Fiat", result[0].Name);
            Assert.Equal(
                $"https://brasilapi.com.br/api/fipe/marcas/v1/{segmentoEsperado}",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetBrandsAsync_QuandoReferenceTableInformada_AdicionaQueryString()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await service.GetBrandsAsync(FipeVehicleType.Cars, 322, new FakeHttpClientFactory(handler));

            Assert.Equal(
                "https://brasilapi.com.br/api/fipe/marcas/v1/carros?tabela_referencia=322",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetBrandsAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetBrandsAsync(FipeVehicleType.Cars, null, new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetBrandsAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("não é json", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetBrandsAsync(FipeVehicleType.Cars, null, new FakeHttpClientFactory(handler)));
        }
    }

    public class Models {

        [Fact]
        public async Task GetModelsAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("""[{"modelo":"Uno","valor":"5940"}]""", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetModelsAsync(FipeVehicleType.Cars, "21", null, new FakeHttpClientFactory(handler));

            Assert.Equal("5940", result[0].Code);
            Assert.Equal("Uno", result[0].Name);
            Assert.Equal(
                "https://brasilapi.com.br/api/fipe/veiculos/v1/carros/21",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetModelsAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetModelsAsync(FipeVehicleType.Cars, "21", null, new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetModelsAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("não é json", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetModelsAsync(FipeVehicleType.Cars, "21", null, new FakeHttpClientFactory(handler)));
        }
    }

    public class ModelYears {

        [Fact]
        public async Task GetModelYearsAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("""[{"nome":"2015 Flex","valor":"2015-1"}]""", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetModelYearsAsync(FipeVehicleType.Cars, "21", "5940", null, new FakeHttpClientFactory(handler));

            Assert.Equal("2015-1", result[0].Code);
            Assert.Equal("2015 Flex", result[0].Label);
            Assert.Equal(
                "https://brasilapi.com.br/api/fipe/anos/v1/carros/21/5940",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetModelYearsAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetModelYearsAsync(FipeVehicleType.Cars, "21", "5940", null, new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetModelYearsAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("não é json", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetModelYearsAsync(FipeVehicleType.Cars, "21", "5940", null, new FakeHttpClientFactory(handler)));
        }
    }

    public class VehiclePrice {

        private const string Json = """
            {
                "valor": "R$ 30.000,00",
                "marca": "Fiat",
                "modelo": "Uno",
                "anoModelo": "2015",
                "combustivel": "Flex",
                "codigoFipe": "001004-9",
                "mesReferencia": "janeiro de 2024",
                "tipoVeiculo": 1,
                "siglaCombustivel": "F",
                "dataConsulta": "quarta-feira, 10 de janeiro de 2024"
            }
            """;

        [Fact]
        public async Task GetVehiclePriceAsync_QuandoApiRespondeComSucesso_MapeiaCamposEUrl()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetVehiclePriceAsync(
                FipeVehicleType.Cars, "21", "5940", "2015-1", null, new FakeHttpClientFactory(handler));

            Assert.Equal("001004-9", result.FipeCode);
            Assert.Equal(2015, result.ModelYear);
            Assert.Equal(1, result.VehicleTypeCode);
            Assert.Equal(
                "https://brasilapi.com.br/api/fipe/detalhes/v1/carros/21/5940/2015-1",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetVehiclePriceAsync_QuandoAnoModeloETipoVeiculoNumericos_ConverteMesmoAssim()
        {
            const string json = """
                {
                    "valor": "R$ 30.000,00",
                    "anoModelo": 2015,
                    "tipoVeiculo": 1
                }
                """;
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetVehiclePriceAsync(
                FipeVehicleType.Cars, "21", "5940", "2015-1", null, new FakeHttpClientFactory(handler));

            Assert.Equal(2015, result.ModelYear);
            Assert.Equal(1, result.VehicleTypeCode);
        }

        [Fact]
        public async Task GetVehiclePriceAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetVehiclePriceAsync(FipeVehicleType.Cars, "21", "5940", "2015-1", null, new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetVehiclePriceAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("não é json", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetVehiclePriceAsync(FipeVehicleType.Cars, "21", "5940", "2015-1", null, new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetPriceByFipeCodeAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent($"[{Json}]", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetPriceByFipeCodeAsync("001004-9", null, new FakeHttpClientFactory(handler));

            Assert.Single(result);
            Assert.Equal(
                "https://brasilapi.com.br/api/fipe/preco/v1/001004-9",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetPriceByFipeCodeAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetPriceByFipeCodeAsync("001004-9", null, new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetPriceByFipeCodeAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("não é json", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetPriceByFipeCodeAsync("001004-9", null, new FakeHttpClientFactory(handler)));
        }
    }
}
