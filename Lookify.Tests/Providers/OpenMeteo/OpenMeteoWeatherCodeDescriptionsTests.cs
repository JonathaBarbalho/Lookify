using Lookify.Providers.OpenMeteo;

namespace Lookify.Tests.Providers.OpenMeteo;

public class OpenMeteoWeatherCodeDescriptionsTests {

    [Theory]
    [InlineData(0, "Céu limpo")]
    [InlineData(3, "Nublado")]
    [InlineData(95, "Trovoada")]
    public void Describe_QuandoCodigoConhecido_RetornaDescricao(int code, string esperado)
    {
        Assert.Equal(esperado, OpenMeteoWeatherCodeDescriptions.Describe(code));
    }

    [Fact]
    public void Describe_QuandoCodigoDesconhecido_RetornaNull()
    {
        Assert.Null(OpenMeteoWeatherCodeDescriptions.Describe(9999));
    }

    [Fact]
    public void Describe_QuandoCodigoNulo_RetornaNull()
    {
        Assert.Null(OpenMeteoWeatherCodeDescriptions.Describe(null));
    }
}
