using System.Net;
using System.Text;
using Lookify.Providers.BrasilApi;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.BrasilApi;

public class BrasilApiCnpjServiceTests {

    private static BrasilApiCnpjService CreateService() =>
        new() {
            ProviderName = "BrasilApi",
            BaseAddress = "https://brasilapi.com.br/"
        };

    private const string FullJson = """
        {
            "cnpj": "11222333000181",
            "identificador_matriz_filial": 1,
            "descricao_matriz_filial": "Matriz",
            "razao_social": "Empresa Exemplo LTDA",
            "nome_fantasia": "Exemplo",
            "situacao_cadastral": 2,
            "descricao_situacao_cadastral": "Ativa",
            "data_situacao_cadastral": "2020-01-15",
            "motivo_situacao_cadastral": 0,
            "codigo_natureza_juridica": 2062,
            "data_inicio_atividade": "2010-05-20",
            "cnae_fiscal": 6201500,
            "cnae_fiscal_descricao": "Desenvolvimento de programas",
            "descricao_tipo_de_logradouro": "Rua",
            "logradouro": "das Flores",
            "numero": "100",
            "complemento": "Sala 1",
            "bairro": "Centro",
            "cep": "01001000",
            "uf": "SP",
            "municipio": "São Paulo",
            "ddd_telefone_1": "1130000000",
            "ddd_telefone_2": "",
            "ddd_fax": "",
            "capital_social": 100000,
            "porte": "05",
            "descricao_porte": "Demais",
            "opcao_pelo_simples": true,
            "data_opcao_pelo_simples": "2015-01-01",
            "data_exclusao_do_simples": null,
            "opcao_pelo_mei": false,
            "situacao_especial": null,
            "data_situacao_especial": null,
            "qsa": [
                {
                    "identificador_de_socio": 2,
                    "nome_socio": "Fulano de Tal",
                    "cnpj_cpf_do_socio": "***123456**",
                    "codigo_qualificacao_socio": 49,
                    "qualificacao_socio": "Sócio-Administrador",
                    "data_entrada_sociedade": "20/05/2010",
                    "pais": null,
                    "cpf_representante_legal": null,
                    "nome_representante_legal": null,
                    "codigo_qualificacao_representante_legal": 0,
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

        Assert.Equal("11222333000181", result.Cnpj);
        Assert.Equal("Empresa Exemplo LTDA", result.CompanyName);
        Assert.Equal("Exemplo", result.TradeName);
        Assert.Equal("1130000000", result.Phone);
        Assert.Equal("100000", result.ShareCapital);
        Assert.True(result.IsSimpleOptIn);
        Assert.False(result.IsMeiOptIn);
        Assert.Equal("São Paulo", result.City);
        var partner = Assert.Single(result.Partners);
        Assert.Equal("Fulano de Tal", partner.Name);
        Assert.Equal(new DateOnly(2010, 5, 20), partner.EntryDate);
        Assert.Equal(
            "https://brasilapi.com.br/api/cnpj/v1/11222333000181",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Theory]
    [InlineData("1130000000", "", "", "1130000000")]
    [InlineData("", "1130000001", "", "1130000001")]
    [InlineData("", "", "1130000002", "1130000002")]
    [InlineData("", "", "", null)]
    public async Task RequestAsync_QuandoTelefonePrincipalAusente_UsaProximoDisponivel(
        string ddd1,
        string ddd2,
        string dddFax,
        string? esperado)
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
                "ddd_telefone_1": "{{ddd1}}",
                "ddd_telefone_2": "{{ddd2}}",
                "ddd_fax": "{{dddFax}}",
                "capital_social": 0,
                "opcao_pelo_simples": false,
                "opcao_pelo_mei": false,
                "qsa": []
            }
            """;
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler));

        Assert.Equal(esperado, result.Phone);
    }

    [Theory]
    [InlineData("20/05/2010", "2010-05-20")]
    [InlineData("2010-05-20", "2010-05-20")]
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
                "opcao_pelo_simples": false,
                "opcao_pelo_mei": false,
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
    public async Task RequestAsync_QuandoApiRetorna404_LancaHttpRequestExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler)));

        Assert.Contains("BrasilApi", exception.Message);
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

        Assert.Contains("BrasilApi", exception.Message);
        Assert.Contains("11222333000181", exception.Message);
    }
}
