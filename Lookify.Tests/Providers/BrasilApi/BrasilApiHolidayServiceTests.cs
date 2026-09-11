using System.Net;
using System.Text;
using Lookify.Providers.BrasilApi;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.BrasilApi;

public class BrasilApiHolidayServiceTests {

    private static BrasilApiHolidayService CreateService() =>
        new() {
            ProviderName = "BrasilApi",
            BaseAddress = "https://brasilapi.com.br/"
        };

    private const string Json = """
        [
            { "date": "2024-01-01", "name": "Confraternização mundial", "type": "national", "weekday": "Monday" }
        ]
        """;

    [Fact]
    public async Task GetHolidaysAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.GetHolidaysAsync(2024, new FakeHttpClientFactory(handler));

        var holiday = Assert.Single(result);
        Assert.Equal(new DateOnly(2024, 1, 1), holiday.Date);
        Assert.Equal("Confraternização mundial", holiday.LocalName);
        Assert.Null(holiday.Name);
        Assert.Equal("national", holiday.Type);
        Assert.Equal("Monday", holiday.Weekday);
        Assert.Equal(
            "https://brasilapi.com.br/api/feriados/v1/2024",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetHolidaysAsync_QuandoApiRetorna404_LancaHttpRequestExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetHolidaysAsync(1500, new FakeHttpClientFactory(handler)));

        Assert.Contains("BrasilApi", exception.Message);
        Assert.Contains("1500", exception.Message);
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetHolidaysAsync_QuandoCorpoInvalido_LancaInvalidOperationExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("não é um json válido", Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetHolidaysAsync(2024, new FakeHttpClientFactory(handler)));

        Assert.Contains("BrasilApi", exception.Message);
        Assert.Contains("2024", exception.Message);
    }
}
