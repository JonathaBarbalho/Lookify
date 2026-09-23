using System.Net;
using System.Text;
using Lookify.Tests.TestSupport;
using Lookify.Weather;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lookify.Tests.Weather;

public class WeatherLookifyServiceTests {

    private static WeatherLookifyService CreateService() =>
        new(
            httpFactory: null!,
            options: Options.Create(new Lookify.LookifyOptions()),
            logger: NullLogger.Instance);

    private static WeatherLookifyService CreateService(IHttpClientFactory httpFactory, Lookify.LookifyOptions options) =>
        new(httpFactory, Options.Create(options), NullLogger.Instance);

    [Fact]
    public async Task GetForecastByCityNameAsync_QuandoCityNameVazio_LancaArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetForecastByCityNameAsync(""));
    }

    [Fact]
    public async Task GetForecastByCoordinatesAsync_QuandoPrimeiroProviderFalhaESegundoSucede_RetornaResultadoDoSegundoProvider()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.Host.Contains("open-meteo")
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : throw new NotSupportedException("Cptec não deveria receber essa chamada, ele lança NotSupportedException antes."));
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetForecastByCoordinatesAsync(-23.55m, -46.63m));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Equal(2, aggregate.InnerExceptions.Count);
        Assert.IsType<HttpRequestException>(aggregate.InnerExceptions[0]);
        Assert.IsType<NotSupportedException>(aggregate.InnerExceptions[1]);
    }

    [Fact]
    public async Task GetForecastByCoordinatesAsync_QuandoProviderDesabilitado_NuncaERequisitado()
    {
        var options = new Lookify.LookifyOptions();
        options.UpdateEnableWeatherProvider(false, WeatherLookifyProviderEnum.OpenMeteo);
        var handler = new FakeHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetForecastByCoordinatesAsync(-23.55m, -46.63m));

        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task GetForecastByCoordinatesAsync_QuandoTodosProvidersHabilitadosFalham_LancaInvalidOperationExceptionComAggregateException()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetForecastByCoordinatesAsync(-23.55m, -46.63m));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Equal(2, aggregate.InnerExceptions.Count);
    }

    [Fact]
    public async Task GetForecastByCityNameAsync_QuandoOpenMeteoFalhaECptecSucede_RetornaResultadoDoCptec()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(request => {
            if (request.RequestUri!.Host.Contains("open-meteo")) {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            if (request.RequestUri.AbsolutePath.Contains("/cidade/")) {
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("""[{"nome":"São Paulo","id":244,"estado":"SP"}]""", Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("""{"cidade":"São Paulo","estado":"SP","clima":[{"data":"2026-09-21","condicao":"c","condicao_desc":"Céu limpo","min":18,"max":27,"indice_uv":6}]}""", Encoding.UTF8, "application/json")
            };
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.GetForecastByCityNameAsync("São Paulo");

        Assert.Single(result);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task GetForecastByCityNameAsync_QuandoOpenMeteoDesabilitado_CptecAtendeSozinho()
    {
        var options = new Lookify.LookifyOptions();
        options.UpdateEnableWeatherProvider(false, WeatherLookifyProviderEnum.OpenMeteo);
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath.Contains("/cidade/")
                ? new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("""[{"nome":"São Paulo","id":244,"estado":"SP"}]""", Encoding.UTF8, "application/json")
                }
                : new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("""{"cidade":"São Paulo","estado":"SP","clima":[{"data":"2026-09-21","condicao":"c","condicao_desc":"Céu limpo","min":18,"max":27,"indice_uv":6}]}""", Encoding.UTF8, "application/json")
                });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.GetForecastByCityNameAsync("São Paulo");

        Assert.Single(result);
        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, request => Assert.Contains("brasilapi", request.RequestUri!.Host));
    }

    [Fact]
    public async Task GetForecastByCityNameAsync_QuandoTodosProvidersHabilitadosFalham_LancaInvalidOperationExceptionComAggregateException()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetForecastByCityNameAsync("São Paulo"));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Equal(2, aggregate.InnerExceptions.Count);
        Assert.IsType<HttpRequestException>(aggregate.InnerExceptions[0]);
        Assert.IsType<HttpRequestException>(aggregate.InnerExceptions[1]);
    }

    [Fact]
    public async Task GetForecastByCityNameAsync_QuandoPrevisaoDoOpenMeteoEstouraTimeOut_RetornaResultadoDoCptec()
    {
        var options = new Lookify.LookifyOptions {
            TimeOut = TimeSpan.FromMilliseconds(100)
        };
        var handler = new AsyncFakeHttpMessageHandler((request, token) => {
            if (request.RequestUri!.Host.Contains("geocoding")) {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("""{"results":[{"name":"São Paulo","latitude":-23.55,"longitude":-46.63,"admin1":"São Paulo"}]}""", Encoding.UTF8, "application/json")
                });
            }

            if (request.RequestUri.Host.Contains("open-meteo")) {
                return AsyncFakeHttpMessageHandler.HangUntilCanceledAsync(token);
            }

            if (request.RequestUri.AbsolutePath.Contains("/cidade/")) {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("""[{"nome":"São Paulo","id":244,"estado":"SP"}]""", Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("""{"cidade":"São Paulo","estado":"SP","clima":[{"data":"2026-09-21","condicao":"c","condicao_desc":"Céu limpo","min":18,"max":27,"indice_uv":6}]}""", Encoding.UTF8, "application/json")
            });
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.GetForecastByCityNameAsync("São Paulo");

        Assert.Single(result);
        Assert.Equal(4, handler.Requests.Count);
        Assert.Equal("geocoding-api.open-meteo.com", handler.Requests[0].RequestUri!.Host);
        Assert.Equal("api.open-meteo.com", handler.Requests[1].RequestUri!.Host);
        Assert.All(handler.Requests.Skip(2), request => Assert.Contains("brasilapi", request.RequestUri!.Host));
    }
}
