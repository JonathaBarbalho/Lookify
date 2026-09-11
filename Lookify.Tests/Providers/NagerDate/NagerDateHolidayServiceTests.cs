using System.Net;
using System.Text;
using Lookify.Providers.NagerDate;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.NagerDate;

public class NagerDateHolidayServiceTests {

    private static NagerDateHolidayService CreateService() =>
        new() {
            ProviderName = "NagerDate",
            BaseAddress = "https://date.nager.at/"
        };

    [Fact]
    public async Task GetHolidaysAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
    {
        const string json = """
            [
                { "date": "2024-01-01", "name": "New Year's Day", "localName": "Confraternização Universal", "types": ["Public"] }
            ]
            """;
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.GetHolidaysAsync(2024, new FakeHttpClientFactory(handler));

        var holiday = Assert.Single(result);
        Assert.Equal(new DateOnly(2024, 1, 1), holiday.Date);
        Assert.Equal("New Year's Day", holiday.Name);
        Assert.Equal("Confraternização Universal", holiday.LocalName);
        Assert.Equal("Public", holiday.Type);
        Assert.Equal(
            "https://date.nager.at/api/v3/PublicHolidays/2024/BR",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Theory]
    [InlineData("""["Public","Bank"]""", "Public, Bank")]
    [InlineData("[]", null)]
    public async Task GetHolidaysAsync_QuandoMultiplosOuNenhumTipo_JuntaOsTiposOuRetornaNull(
        string typesJson,
        string? esperado)
    {
        var json = $$"""
            [
                { "date": "2024-01-01", "name": "New Year's Day", "localName": "Confraternização Universal", "types": {{typesJson}} }
            ]
            """;
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.GetHolidaysAsync(2024, new FakeHttpClientFactory(handler));

        Assert.Equal(esperado, result[0].Type);
    }

    [Fact]
    public async Task GetHolidaysAsync_QuandoApiRetorna404_LancaHttpRequestExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetHolidaysAsync(1500, new FakeHttpClientFactory(handler)));

        Assert.Contains("NagerDate", exception.Message);
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

        Assert.Contains("NagerDate", exception.Message);
        Assert.Contains("2024", exception.Message);
    }
}
