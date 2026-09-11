using Lookify.Fipe;

namespace Lookify.Tests.LookifyOptionsTests;

public class FipeLookifyOptionsTests {

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateEnableFipeProvider_SemProviders_LancaArgumentException(bool isEnabled)
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.UpdateEnableFipeProvider(isEnabled));
    }

    [Theory]
    [InlineData(FipeLookifyProviderEnum.BrasilApi)]
    [InlineData(FipeLookifyProviderEnum.Parallelum)]
    public void UpdateEnableFipeProvider_QuandoDesabilitaUmProvider_SoAqueleFicaDesabilitado(
        FipeLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();

        options.UpdateEnableFipeProvider(false, provider);

        Assert.False(options.GetProviderOptions(provider).IsEnabled);
        Assert.All(
            options.FipeProviders.Where(other => other != provider),
            other => Assert.True(options.GetProviderOptions(other).IsEnabled));
    }

    [Fact]
    public void OrderFipeProviders_SemProviders_LancaArgumentException()
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.OrderFipeProviders());
    }

    [Theory]
    [InlineData(FipeLookifyProviderEnum.BrasilApi)]
    [InlineData(FipeLookifyProviderEnum.Parallelum)]
    public void OrderFipeProviders_QuandoRecebeUmUnicoProvider_ColocaEleNoInicioMantendoOrdemOriginalDosDemais(
        FipeLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();
        var ordemOriginal = options.FipeProviders.ToList();

        options.OrderFipeProviders(provider);

        var esperado = new List<FipeLookifyProviderEnum> { provider };
        esperado.AddRange(ordemOriginal.Where(other => other != provider));

        Assert.Equal(esperado, options.FipeProviders);
    }

    [Fact]
    public void OrderFipeProviders_QuandoRecebeTodosOsProvidersEmOrdemDiferente_AplicaExatamenteAOrdemInformada()
    {
        var options = new Lookify.LookifyOptions();

        options.OrderFipeProviders(
            FipeLookifyProviderEnum.Parallelum,
            FipeLookifyProviderEnum.BrasilApi);

        Assert.Equal(
            [
                FipeLookifyProviderEnum.Parallelum,
                FipeLookifyProviderEnum.BrasilApi
            ],
            options.FipeProviders);
    }
}
