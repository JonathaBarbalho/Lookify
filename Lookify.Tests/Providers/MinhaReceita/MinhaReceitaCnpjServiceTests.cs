using System.Net;
using System.Text;
using Lookify.Providers.MinhaReceita;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.MinhaReceita;

public class MinhaReceitaCnpjServiceTests {

    private static MinhaReceitaCnpjService CreateService() =>
        new() {
            ProviderName = "MinhaReceita",
            BaseAddress = "https://minhareceita.org/"
        };

    private const string FullJson = """
        {
            "cnpj": "11222333000181",
            "identificador_matriz_filial": 1,
            "descricao_identificador_matriz_filial": "Matriz",
            "razao_social": "Empresa Exemplo LTDA",
            "nome_fantasia": "Exemplo",
            "email": "contato@exemplo.com",
            "situacao_cadastral": 2,
            "descricao_situacao_cadastral": "Ativa",
            "data_situacao_cadastral": "2020-01-15",
            "motivo_situacao_cadastral": 0,
            "codigo_natureza_juridica": 2062,
            "natureza_juridica": "Sociedade Empresária Limitada",
            "data_inicio_atividade": "2010-05-20",
            "cnae_fiscal": 6201500,
            "cnae_fiscal_descricao": "Desenvolvimento de programas",
            "cnaes_secundarios": [ { "codigo": 6202300, "descricao": "Suporte técnico" } ],
            "logradouro": "das Flores",
            "numero": "100",
            "bairro": "Centro",
            "cep": "01001000",
            "uf": "SP",
            "municipio": "São Paulo",
            "ddd_telefone_1": "1130000000",
            "capital_social": 100000,
            "porte": "Demais",
            "codigo_porte": 5,
            "opcao_pelo_simples": true,
            "opcao_pelo_mei": false,
            "qsa": [
                {
                    "identificador_de_socio": 2,
                    "nome_socio": "Fulano de Tal",
                    "codigo_qualificacao_socio": 49,
                    "data_entrada_sociedade": "2010-05-20",
                    "codigo_qualificacao_representante_legal": 0
                }
            ]
        }
        """;

    [Fact]
    public async Task RequestAsync_QuandoApiRespondeComSucesso_MapeiaCamposEUrlSemPathSegment()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(FullJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler));

        Assert.Equal("Empresa Exemplo LTDA", result.CompanyName);
        Assert.Equal("contato@exemplo.com", result.Email);
        Assert.Equal("100000", result.ShareCapital);
        Assert.True(result.IsSimpleOptIn);
        Assert.False(result.IsMeiOptIn);
        Assert.Equal("5", result.CompanySizeCode);
        var secondary = Assert.Single(result.SecondaryCnaes);
        Assert.Equal("6202300", secondary.Code);
        var partner = Assert.Single(result.Partners);
        Assert.Equal(new DateOnly(2010, 5, 20), partner.EntryDate);
        Assert.Equal(
            "https://minhareceita.org/11222333000181",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Theory]
    [InlineData("2010-05-20", "2010-05-20")]
    [InlineData("20/05/2010", "2010-05-20")]
    [InlineData("data-invalida", null)]
    public async Task RequestAsync_QuandoDataDeEntradaDoSocioEmFormatosDiferentes_ConverteOuRetornaNull(
        string dataEntrada,
        string? dataEsperada)
    {
        var json = $$"""
            {
                "cnpj": "11222333000181",
                "razao_social": "Empresa",
                "identificador_matriz_filial": 1,
                "situacao_cadastral": 2,
                "motivo_situacao_cadastral": 0,
                "codigo_natureza_juridica": 2062,
                "cnae_fiscal": 6201500,
                "capital_social": 0,
                "opcao_pelo_simples": null,
                "opcao_pelo_mei": null,
                "qsa": [
                    {
                        "identificador_de_socio": 2,
                        "nome_socio": "Fulano",
                        "codigo_qualificacao_socio": 49,
                        "data_entrada_sociedade": "{{dataEntrada}}",
                        "codigo_qualificacao_representante_legal": 0
                    }
                ]
            }
            """;
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler));

        var esperado = dataEsperada is null ? (DateOnly?)null : DateOnly.Parse(dataEsperada);
        Assert.Equal(esperado, result.Partners[0].EntryDate);
    }

    [Fact]
    public async Task RequestAsync_QuandoBoolNulos_MapeiaComoNull()
    {
        const string json = """
            {
                "cnpj": "11222333000181",
                "razao_social": "Empresa",
                "identificador_matriz_filial": 1,
                "situacao_cadastral": 2,
                "motivo_situacao_cadastral": 0,
                "codigo_natureza_juridica": 2062,
                "cnae_fiscal": 6201500,
                "capital_social": 0,
                "opcao_pelo_simples": null,
                "opcao_pelo_mei": null,
                "qsa": []
            }
            """;
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler));

        Assert.Null(result.IsSimpleOptIn);
        Assert.Null(result.IsMeiOptIn);
    }

    [Fact]
    public async Task RequestAsync_QuandoApiRetorna404_LancaHttpRequestExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler)));

        Assert.Contains("MinhaReceita", exception.Message);
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

        Assert.Contains("MinhaReceita", exception.Message);
        Assert.Contains("11222333000181", exception.Message);
    }
}
