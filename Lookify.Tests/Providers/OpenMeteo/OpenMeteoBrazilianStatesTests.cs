using Lookify.Providers.OpenMeteo;

namespace Lookify.Tests.Providers.OpenMeteo;

public class OpenMeteoBrazilianStatesTests {

    public static IEnumerable<object[]> AllStates() =>
        new (string Uf, string Name)[] {
            ("AC", "Acre"), ("AL", "Alagoas"), ("AP", "Amapá"), ("AM", "Amazonas"),
            ("BA", "Bahia"), ("CE", "Ceará"), ("DF", "Distrito Federal"), ("ES", "Espírito Santo"),
            ("GO", "Goiás"), ("MA", "Maranhão"), ("MT", "Mato Grosso"), ("MS", "Mato Grosso do Sul"),
            ("MG", "Minas Gerais"), ("PA", "Pará"), ("PB", "Paraíba"), ("PR", "Paraná"),
            ("PE", "Pernambuco"), ("PI", "Piauí"), ("RJ", "Rio de Janeiro"), ("RN", "Rio Grande do Norte"),
            ("RS", "Rio Grande do Sul"), ("RO", "Rondônia"), ("RR", "Roraima"), ("SC", "Santa Catarina"),
            ("SP", "São Paulo"), ("SE", "Sergipe"), ("TO", "Tocantins")
        }.Select(pair => new object[] { pair.Uf, pair.Name });

    [Theory]
    [MemberData(nameof(AllStates))]
    public void GetName_QuandoUfConhecida_RetornaNomeDoEstado(string uf, string nomeEsperado)
    {
        Assert.Equal(nomeEsperado, OpenMeteoBrazilianStates.GetName(uf));
    }

    [Theory]
    [MemberData(nameof(AllStates))]
    public void GetUf_QuandoNomeConhecido_RetornaUf(string ufEsperada, string nome)
    {
        Assert.Equal(ufEsperada, OpenMeteoBrazilianStates.GetUf(nome));
    }

    [Fact]
    public void GetName_QuandoUfDesconhecida_RetornaNull()
    {
        Assert.Null(OpenMeteoBrazilianStates.GetName("XX"));
    }

    [Fact]
    public void GetName_QuandoUfNula_RetornaNull()
    {
        Assert.Null(OpenMeteoBrazilianStates.GetName(null));
    }

    [Fact]
    public void GetUf_QuandoNomeDesconhecido_RetornaNull()
    {
        Assert.Null(OpenMeteoBrazilianStates.GetUf("Nárnia"));
    }

    [Fact]
    public void GetUf_QuandoNomeNulo_RetornaNull()
    {
        Assert.Null(OpenMeteoBrazilianStates.GetUf(null));
    }
}
