using System.Net;
using System.Text;
using Lookify.Providers.ReceitaWs;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.ReceitaWs;

public class ReceitaWsCnpjServiceTests {

    private static ReceitaWsCnpjService CreateService() =>
        new() {
            ProviderName = "ReceitaWs",
            BaseAddress = "https://receitaws.com.br/"
        };

    private const string FullJson = """
        {
            "cnpj": "11222333000181",
            "tipo": "MATRIZ",
            "abertura": "20/05/2010",
            "nome": "Empresa Exemplo LTDA",
            "fantasia": "Exemplo",
            "atividade_principal": [ { "code": "62.01-5-01", "text": "Desenvolvimento de programas" } ],
            "atividades_secundarias": [ { "code": "62.02-3-00", "text": "Suporte técnico" } ],
            "natureza_juridica": "206-2 - Sociedade Empresária Limitada",
            "logradouro": "das Flores",
            "numero": "100",
            "complemento": "Sala 1",
            "bairro": "Centro",
            "municipio": "São Paulo",
            "uf": "SP",
            "cep": "01001000",
            "email": "contato@exemplo.com",
            "telefone": "(11) 3000-0000",
            "situacao": "ATIVA",
            "data_situacao": "15/01/2020",
            "motivo_situacao": "SEM MOTIVO",
            "capital_social": 100000,
            "qsa": [
                {
                    "nome": "Fulano de Tal",
                    "qual": "Sócio-Administrador",
                    "data_entrada": "20/05/2010",
                    "cpf_cnpj": "***123456**",
                    "faixa_etaria": "31 a 40 anos"
                }
            ]
        }
        """;

    [Fact]
    public async Task RequestAsync_QuandoApiRespondeComSucesso_MapeiaCamposEUrl()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(FullJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler));

        Assert.Equal("Empresa Exemplo LTDA", result.CompanyName);
        Assert.Equal("Exemplo", result.TradeName);
        Assert.Equal("(11) 3000-0000", result.Phone);
        Assert.Equal("contato@exemplo.com", result.Email);
        Assert.Equal("62.01-5-01", result.PrimaryCnaeCode);
        Assert.Equal("Desenvolvimento de programas", result.PrimaryCnaeDescription);
        var secondary = Assert.Single(result.SecondaryCnaes);
        Assert.Equal("62.02-3-00", secondary.Code);
        Assert.Equal("100000", result.ShareCapital);
        Assert.Equal(new DateOnly(2010, 5, 20), result.ActivityStartDate);
        Assert.Equal(new DateOnly(2020, 1, 15), result.RegistrationStatusDate);
        var partner = Assert.Single(result.Partners);
        Assert.Equal("Fulano de Tal", partner.Name);
        Assert.Equal(new DateOnly(2010, 5, 20), partner.EntryDate);
        Assert.Equal(
            "https://receitaws.com.br/v1/cnpj/11222333000181",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Theory]
    [InlineData("20/05/2010", "2010-05-20")]
    [InlineData("2010-05-20", "2010-05-20")]
    [InlineData("data-invalida", null)]
    public async Task RequestAsync_QuandoDataDeAberturaEmFormatosDiferentes_ConverteOuRetornaNull(
        string abertura,
        string? dataEsperada)
    {
        var json = $$"""
            {
                "cnpj": "11222333000181",
                "nome": "Empresa",
                "abertura": "{{abertura}}",
                "capital_social": 0,
                "qsa": []
            }
            """;
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler));

        var esperado = dataEsperada is null ? (DateOnly?)null : DateOnly.Parse(dataEsperada);
        Assert.Equal(esperado, result.ActivityStartDate);
    }

    [Fact]
    public async Task RequestAsync_QuandoApiRetorna404_LancaHttpRequestExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler)));

        Assert.Contains("ReceitaWs", exception.Message);
        Assert.Contains("11222333000181", exception.Message);
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task RequestAsync_QuandoCorpoInvalido_LancaInvalidOperationExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("não é um json válido", Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler)));

        Assert.Contains("ReceitaWs", exception.Message);
        Assert.Contains("11222333000181", exception.Message);
    }
}
