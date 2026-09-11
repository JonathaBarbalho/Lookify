using System.Net;
using System.Text;
using Lookify.Fipe;
using Lookify.Providers.Parallelum;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.Parallelum;

public class ParallelumFipeServiceTests {

    private static ParallelumFipeService CreateService() =>
        new() {
            ProviderName = "Parallelum",
            BaseAddress = "https://fipe.parallelum.com.br/"
        };

    public class ReferenceTables {

        [Fact]
        public async Task GetReferenceTablesAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("""[{"code":322,"month":"janeiro/2024"}]""", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetReferenceTablesAsync(new FakeHttpClientFactory(handler));

            var table = Assert.Single(result);
            Assert.Equal(322, table.Code);
            Assert.Equal(
                "https://fipe.parallelum.com.br/api/v2/references",
                handler.LastRequest?.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetReferenceTablesAsync_QuandoApiRetorna404_LancaHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.GetReferenceTablesAsync(new FakeHttpClientFactory(handler)));
        }

        [Fact]
        public async Task GetReferenceTablesAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("não é json", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetReferenceTablesAsync(new FakeHttpClientFactory(handler)));
        }
    }

    public class Brands {

        [Theory]
        [InlineData(FipeVehicleType.Cars, "cars")]
        [InlineData(FipeVehicleType.Motorcycles, "motorcycles")]
        [InlineData(FipeVehicleType.Trucks, "trucks")]
        public async Task GetBrandsAsync_QuandoTipoDeVeiculo_UsaSegmentoCorretoNaUrl(
            FipeVehicleType vehicleType,
            string segmentoEsperado)
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("""[{"code":"21","name":"Fiat"}]""", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetBrandsAsync(vehicleType, null, new FakeHttpClientFactory(handler));

            Assert.Equal("21", result[0].Code);
            Assert.Equal("Fiat", result[0].Name);
            Assert.Equal(
                $"https://fipe.parallelum.com.br/api/v2/{segmentoEsperado}/brands",
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
                "https://fipe.parallelum.com.br/api/v2/cars/brands?reference=322",
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
                Content = new StringContent("""[{"code":"5940","name":"Uno"}]""", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetModelsAsync(FipeVehicleType.Cars, "21", null, new FakeHttpClientFactory(handler));

            Assert.Equal("5940", result[0].Code);
            Assert.Equal("Uno", result[0].Name);
            Assert.Equal(
                "https://fipe.parallelum.com.br/api/v2/cars/brands/21/models",
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
                Content = new StringContent("""[{"code":"2015-1","name":"2015 Gasoline"}]""", Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetModelYearsAsync(FipeVehicleType.Cars, "21", "5940", null, new FakeHttpClientFactory(handler));

            Assert.Equal("2015-1", result[0].Code);
            Assert.Equal("2015 Gasoline", result[0].Label);
            Assert.Equal(
                "https://fipe.parallelum.com.br/api/v2/cars/brands/21/models/5940/years",
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

        [Fact]
        public async Task GetVehiclePriceAsync_QuandoApiRespondeComSucesso_MapeiaCamposEUrl()
        {
            const string json = """
                {
                    "price": "R$ 30.000,00",
                    "brand": "Fiat",
                    "model": "Uno",
                    "modelYear": "2015",
                    "fuel": "Gasoline",
                    "codeFipe": "001004-9",
                    "referenceMonth": "january de 2024",
                    "vehicleType": 1,
                    "fuelAcronym": "G"
                }
                """;
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var service = CreateService();

            var result = await service.GetVehiclePriceAsync(
                FipeVehicleType.Cars, "21", "5940", "2015-1", null, new FakeHttpClientFactory(handler));

            Assert.Equal("001004-9", result.FipeCode);
            Assert.Equal(2015, result.ModelYear);
            Assert.Equal(1, result.VehicleTypeCode);
            Assert.Null(result.RequestDate);
            Assert.Equal(
                "https://fipe.parallelum.com.br/api/v2/cars/brands/21/models/5940/years/2015-1",
                handler.LastRequest?.RequestUri?.ToString());
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
        public async Task GetPriceByFipeCodeAsync_SempreLancaNotSupportedExceptionSemChamadaHttp()
        {
            var handler = new FakeHttpMessageHandler(
                _ => throw new InvalidOperationException("Não deveria ter feito requisição HTTP."));
            var service = CreateService();

            var exception = await Assert.ThrowsAsync<NotSupportedException>(() =>
                service.GetPriceByFipeCodeAsync("001004-9", null, new FakeHttpClientFactory(handler)));

            Assert.Contains("Parallelum", exception.Message);
            Assert.Empty(handler.Requests);
        }
    }
}
