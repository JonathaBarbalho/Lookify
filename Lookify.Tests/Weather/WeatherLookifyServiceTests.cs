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
}
