using Lookify.Weather;

namespace Lookify.Tests.LookifyOptionsTests;

public class WeatherLookifyOptionsTests {

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateEnableWeatherProvider_SemProviders_LancaArgumentException(bool isEnabled)
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.UpdateEnableWeatherProvider(isEnabled));
    }

    [Theory]
    [InlineData(WeatherLookifyProviderEnum.OpenMeteo)]
    [InlineData(WeatherLookifyProviderEnum.Cptec)]
    public void UpdateEnableWeatherProvider_QuandoDesabilitaUmProvider_SoAqueleFicaDesabilitado(
        WeatherLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();

        options.UpdateEnableWeatherProvider(false, provider);

        Assert.False(options.GetProviderOptions(provider).IsEnabled);
        Assert.All(
            options.WeatherProviders.Where(other => other != provider),
            other => Assert.True(options.GetProviderOptions(other).IsEnabled));
    }

    [Fact]
    public void OrderWeatherProviders_SemProviders_LancaArgumentException()
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.OrderWeatherProviders());
    }

    [Theory]
    [InlineData(WeatherLookifyProviderEnum.OpenMeteo)]
    [InlineData(WeatherLookifyProviderEnum.Cptec)]
    public void OrderWeatherProviders_QuandoRecebeUmUnicoProvider_ColocaEleNoInicioMantendoOrdemOriginalDosDemais(
        WeatherLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();
        var ordemOriginal = options.WeatherProviders.ToList();

        options.OrderWeatherProviders(provider);

        var esperado = new List<WeatherLookifyProviderEnum> { provider };
        esperado.AddRange(ordemOriginal.Where(other => other != provider));

        Assert.Equal(esperado, options.WeatherProviders);
    }

    [Fact]
    public void OrderWeatherProviders_QuandoRecebeTodosOsProvidersEmOrdemDiferente_AplicaExatamenteAOrdemInformada()
    {
        var options = new Lookify.LookifyOptions();

        options.OrderWeatherProviders(
            WeatherLookifyProviderEnum.Cptec,
            WeatherLookifyProviderEnum.OpenMeteo);

        Assert.Equal(
            [
                WeatherLookifyProviderEnum.Cptec,
                WeatherLookifyProviderEnum.OpenMeteo
            ],
            options.WeatherProviders);
    }
}
