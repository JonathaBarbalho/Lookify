using System.Net;
using System.Text;
using Lookify.Bank;
using Lookify.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Lookify.Tests.Bank;

public class BankLookifyServiceTests {

    private static BankLookifyService CreateService(IHttpClientFactory httpFactory, Lookify.LookifyOptions options) =>
        new(httpFactory, Options.Create(options), NullLogger.Instance);

    private const string BrasilApiBanksJson = """
        [
            {
                "ispb": "00000000",
                "name": "BCO DO BRASIL",
                "code": 1,
                "fullName": "Banco do Brasil S.A."
            }
        ]
        """;

    private const string BrasilApiBankJson = """
        {
            "ispb": "00000000",
            "name": "BCO DO BRASIL",
            "code": 1,
            "fullName": "Banco do Brasil S.A."
        }
        """;

    [Fact]
    public async Task GetAllBanksAsync_QuandoProviderSucede_RetornaListaDeBancos()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(BrasilApiBanksJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.GetAllBanksAsync();

        Assert.Single(result);
        Assert.Equal("BCO DO BRASIL", result[0].Name);
    }

    [Fact]
    public async Task GetAllBanksAsync_QuandoNenhumProviderHabilitado_LancaInvalidOperationException()
    {
        var options = new Lookify.LookifyOptions();
        options.UpdateEnableBankProvider(false, BankLookifyProviderEnum.BrasilApi);
        var handler = new FakeHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetAllBanksAsync());

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Empty(aggregate.InnerExceptions);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task GetBankByCodeAsync_QuandoProviderSucede_RetornaBancoEUrlCorreta()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(BrasilApiBankJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var result = await service.GetBankByCodeAsync(1);

        Assert.Equal("BCO DO BRASIL", result.Name);
        Assert.Equal(
            "https://brasilapi.com.br/api/banks/v1/1",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetBankByCodeAsync_QuandoProviderFalha_LancaInvalidOperationExceptionComAggregateException()
    {
        var options = new Lookify.LookifyOptions();
        var handler = new FakeHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(new FakeHttpClientFactory(handler), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetBankByCodeAsync(1));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Single(aggregate.InnerExceptions);
    }
}
