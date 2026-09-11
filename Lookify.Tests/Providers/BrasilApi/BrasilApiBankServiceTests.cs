using System.Net;
using System.Text;
using Lookify.Providers.BrasilApi;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.BrasilApi;

public class BrasilApiBankServiceTests {

    private static BrasilApiBankService CreateService() =>
        new() {
            ProviderName = "BrasilApi",
            BaseAddress = "https://brasilapi.com.br/"
        };

    private const string BankWithAddressJson = """
        {
            "ispb": "00000000",
            "name": "BCO DO BRASIL",
            "code": 1,
            "fullName": "Banco do Brasil S.A.",
            "cnpj": "00000000000191",
            "headquarters_address": {
                "street": "SBS Quadra 4",
                "number": "100",
                "complement": "Bloco A",
                "district": "Asa Sul",
                "city": "Brasília",
                "state": "DF",
                "zipCode": "70074900"
            },
            "logo_url": "https://logo.example/bb.png"
        }
        """;

    private const string BankWithoutAddressJson = """
        {
            "ispb": "00000000",
            "name": "BCO DO BRASIL",
            "code": 1,
            "fullName": "Banco do Brasil S.A."
        }
        """;

    [Fact]
    public async Task GetAllBanksAsync_QuandoApiRespondeComSucesso_MapeiaListaEUrl()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent($"[{BankWithAddressJson}]", Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.GetAllBanksAsync(new FakeHttpClientFactory(handler));

        var bank = Assert.Single(result);
        Assert.Equal(1, bank.Code);
        Assert.Equal("BCO DO BRASIL", bank.Name);
        Assert.Equal("Banco do Brasil S.A.", bank.FullName);
        Assert.Equal("SBS Quadra 4", bank.Street);
        Assert.Equal("100", bank.Number);
        Assert.Equal("Bloco A", bank.Complement);
        Assert.Equal("Asa Sul", bank.District);
        Assert.Equal("Brasília", bank.City);
        Assert.Equal("DF", bank.State);
        Assert.Equal("70074900", bank.ZipCode);
        Assert.Equal("https://logo.example/bb.png", bank.LogoUrl);
        Assert.Equal(
            "https://brasilapi.com.br/api/banks/v1",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetAllBanksAsync_QuandoApiRetorna404_LancaHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetAllBanksAsync(new FakeHttpClientFactory(handler)));

        Assert.Contains("BrasilApi", exception.Message);
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetAllBanksAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("não é um json válido", Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetAllBanksAsync(new FakeHttpClientFactory(handler)));

        Assert.Contains("BrasilApi", exception.Message);
    }

    [Fact]
    public async Task GetBankByCodeAsync_QuandoApiRespondeComSucesso_MapeiaBancoEUrl()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(BankWithAddressJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.GetBankByCodeAsync(1, new FakeHttpClientFactory(handler));

        Assert.Equal("BCO DO BRASIL", result.Name);
        Assert.Equal(
            "https://brasilapi.com.br/api/banks/v1/1",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetBankByCodeAsync_QuandoApiRetorna404_LancaHttpRequestExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetBankByCodeAsync(999, new FakeHttpClientFactory(handler)));

        Assert.Contains("BrasilApi", exception.Message);
        Assert.Contains("999", exception.Message);
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetBankByCodeAsync_QuandoHeadquartersAddressAusente_CamposDeEnderecoFicamNulos()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(BankWithoutAddressJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.GetBankByCodeAsync(1, new FakeHttpClientFactory(handler));

        Assert.Null(result.Street);
        Assert.Null(result.Number);
        Assert.Null(result.Complement);
        Assert.Null(result.District);
        Assert.Null(result.City);
        Assert.Null(result.State);
        Assert.Null(result.ZipCode);
    }
}
