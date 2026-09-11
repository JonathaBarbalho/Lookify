using Lookify.VehiclePlate;

namespace Lookify.Tests.LookifyOptionsTests;

public class VehiclePlateLookifyOptionsTests {

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateEnableVehiclePlateProvider_SemProviders_LancaArgumentException(bool isEnabled)
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.UpdateEnableVehiclePlateProvider(isEnabled));
    }

    [Fact]
    public void UpdateEnableVehiclePlateProvider_QuandoDesabilita_ProviderFicaDesabilitado()
    {
        var options = new Lookify.LookifyOptions();

        options.UpdateEnableVehiclePlateProvider(false, VehiclePlateLookifyProviderEnum.PlacaFipe);

        Assert.False(options.GetProviderOptions(VehiclePlateLookifyProviderEnum.PlacaFipe).IsEnabled);
    }

    [Fact]
    public void OrderVehiclePlateProviders_SemProviders_LancaArgumentException()
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.OrderVehiclePlateProviders());
    }

    [Fact]
    public void OrderVehiclePlateProviders_QuandoRecebeOUnicoProvider_MantemNaLista()
    {
        var options = new Lookify.LookifyOptions();

        options.OrderVehiclePlateProviders(VehiclePlateLookifyProviderEnum.PlacaFipe);

        Assert.Equal([VehiclePlateLookifyProviderEnum.PlacaFipe], options.VehiclePlateProviders);
    }

    [Fact]
    public void SetPlacaFipeToken_QuandoChamado_AtualizaTokenDoProviderPlacaFipe()
    {
        var options = new Lookify.LookifyOptions();

        options.SetPlacaFipeToken("meu-token");

        Assert.Equal(
            "meu-token",
            options.GetProviderOptions(VehiclePlateLookifyProviderEnum.PlacaFipe).Token);
    }

    [Fact]
    public void PlacaFipeToken_QuandoVariavelDeAmbienteDefinida_UsaValorComoPadrao()
    {
        var valorOriginal = Environment.GetEnvironmentVariable("LOOKIFY_PLACAFIPE_TOKEN");
        try {
            Environment.SetEnvironmentVariable("LOOKIFY_PLACAFIPE_TOKEN", "token-do-ambiente");

            var options = new Lookify.LookifyOptions();

            Assert.Equal(
                "token-do-ambiente",
                options.GetProviderOptions(VehiclePlateLookifyProviderEnum.PlacaFipe).Token);
        }
        finally {
            Environment.SetEnvironmentVariable("LOOKIFY_PLACAFIPE_TOKEN", valorOriginal);
        }
    }

    [Fact]
    public void PlacaFipeToken_QuandoVariavelDeAmbienteNaoDefinida_UsaStringVazia()
    {
        var valorOriginal = Environment.GetEnvironmentVariable("LOOKIFY_PLACAFIPE_TOKEN");
        try {
            Environment.SetEnvironmentVariable("LOOKIFY_PLACAFIPE_TOKEN", null);

            var options = new Lookify.LookifyOptions();

            Assert.Equal(
                string.Empty,
                options.GetProviderOptions(VehiclePlateLookifyProviderEnum.PlacaFipe).Token);
        }
        finally {
            Environment.SetEnvironmentVariable("LOOKIFY_PLACAFIPE_TOKEN", valorOriginal);
        }
    }
}
