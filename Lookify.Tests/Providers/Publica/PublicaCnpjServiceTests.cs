using System.Net;
using System.Text;
using Lookify.Providers.Publica;
using Lookify.Tests.TestSupport;

namespace Lookify.Tests.Providers.Publica;

public class PublicaCnpjServiceTests {

    private static PublicaCnpjService CreateService() =>
        new() {
            ProviderName = "Publica",
            BaseAddress = "https://publica.cnpj.ws/"
        };

    private const string FullJson = """
        {
            "cnpj": "11222333000181",
            "razao_social": "Empresa Exemplo LTDA",
            "capital_social": "100000.00",
            "atualizado_em": "2024-01-01T10:00:00Z",
            "porte": { "id": 5, "descricao": "Demais" },
            "natureza_juridica": { "id": "2062", "descricao": "Sociedade Empresária Limitada" },
            "simples": { "simples": "sim", "mei": 0, "data_opcao_simples": "2015-01-01" },
            "socios": [
                {
                    "cpf_cnpj_socio": "***123456**",
                    "nome": "Fulano de Tal",
                    "data_entrada": "2010-05-20",
                    "faixa_etaria": "31 a 40 anos",
                    "pais_id": 76,
                    "qualificacao_socio": { "id": 49, "descricao": "Sócio-Administrador" }
                }
            ],
            "estabelecimento": {
                "tipo": "MATRIZ",
                "nome_fantasia": "Exemplo",
                "situacao_cadastral": "ATIVA",
                "data_situacao_cadastral": "2020-01-15",
                "data_inicio_atividade": "2010-05-20",
                "pais": { "nome": "Brasil" },
                "estado": { "sigla": "SP" },
                "cidade": { "nome": "São Paulo" },
                "cep": "01001000",
                "tipo_logradouro": "Rua",
                "logradouro": "das Flores",
                "numero": "100",
                "complemento": "Sala 1",
                "bairro": "Centro",
                "ddd1": "11",
                "telefone1": "30000000",
                "email": "contato@exemplo.com",
                "atividade_principal": { "id": "6201-5/01", "descricao": "Desenvolvimento de programas" },
                "atividades_secundarias": [ { "id": "6202-3/00", "descricao": "Suporte técnico" } ]
            }
        }
        """;

    [Fact]
    public async Task RequestAsync_QuandoApiRespondeComSucesso_MapeiaCamposAninhadosEUrl()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(FullJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler));

        Assert.Equal("Empresa Exemplo LTDA", result.CompanyName);
        Assert.Equal("Exemplo", result.TradeName);
        Assert.Equal("(11) 30000000", result.Phone);
        Assert.Equal("5", result.CompanySizeCode);
        Assert.Equal("Demais", result.CompanySizeDescription);
        Assert.Equal("2062", result.LegalNatureCode);
        Assert.True(result.IsSimpleOptIn);
        Assert.False(result.IsMeiOptIn);
        Assert.Equal("SP", result.State);
        Assert.Equal("São Paulo", result.City);
        Assert.Equal("Brasil", result.Country);
        Assert.Equal("6201-5/01", result.PrimaryCnaeCode);
        var secondary = Assert.Single(result.SecondaryCnaes);
        Assert.Equal("6202-3/00", secondary.Code);
        var partner = Assert.Single(result.Partners);
        Assert.Equal("Fulano de Tal", partner.Name);
        Assert.Equal("76", partner.Country);
        Assert.Equal(
            "https://publica.cnpj.ws/cnpj/11222333000181",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task RequestAsync_QuandoEstabelecimentoAusente_CamposDeEnderecoEAtividadeFicamNulos()
    {
        const string json = """
            {
                "cnpj": "11222333000181",
                "razao_social": "Empresa",
                "capital_social": "0",
                "socios": []
            }
            """;
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler));

        Assert.Null(result.TradeName);
        Assert.Null(result.Street);
        Assert.Null(result.City);
        Assert.Null(result.PrimaryCnaeCode);
        Assert.Empty(result.SecondaryCnaes);
    }

    [Theory]
    [InlineData("11", "30000000", "22", "40000000", null, null, "(11) 30000000")]
    [InlineData(null, null, "22", "40000000", null, null, "(22) 40000000")]
    [InlineData(null, null, null, null, "11", "50000000", "(11) 50000000")]
    [InlineData(null, "30000000", null, null, null, null, "30000000")]
    [InlineData(null, null, null, null, null, null, null)]
    public async Task RequestAsync_QuandoTelefonePrincipalAusente_UsaCadeiaDeFallback(
        string? ddd1,
        string? tel1,
        string? ddd2,
        string? tel2,
        string? dddFax,
        string? fax,
        string? esperado)
    {
        string? Json(string? value) => value is null ? null : $"\"{value}\"";

        var json = $$"""
            {
                "cnpj": "11222333000181",
                "razao_social": "Empresa",
                "capital_social": "0",
                "socios": [],
                "estabelecimento": {
                    "ddd1": {{Json(ddd1) ?? "null"}},
                    "telefone1": {{Json(tel1) ?? "null"}},
                    "ddd2": {{Json(ddd2) ?? "null"}},
                    "telefone2": {{Json(tel2) ?? "null"}},
                    "ddd_fax": {{Json(dddFax) ?? "null"}},
                    "fax": {{Json(fax) ?? "null"}}
                }
            }
            """;
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var service = CreateService();

        var result = await service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler));

        Assert.Equal(esperado, result.Phone);
    }

    [Fact]
    public async Task RequestAsync_QuandoApiRetorna404_LancaHttpRequestExceptionComProviderEIdentificador()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.RequestAsync("11222333000181", new FakeHttpClientFactory(handler)));

        Assert.Contains("Publica", exception.Message);
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

        Assert.Contains("Publica", exception.Message);
        Assert.Contains("11222333000181", exception.Message);
    }
}
