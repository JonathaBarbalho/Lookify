using Lookify.Ibge;

namespace Lookify.Tests.LookifyOptionsTests;

public class IbgeLookifyOptionsTests {

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateEnableIbgeProvider_SemProviders_LancaArgumentException(bool isEnabled)
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.UpdateEnableIbgeProvider(isEnabled));
    }

    [Theory]
    [InlineData(IbgeLookifyProviderEnum.Ibge)]
    [InlineData(IbgeLookifyProviderEnum.BrasilApi)]
    public void UpdateEnableIbgeProvider_QuandoDesabilitaUmProvider_SoAqueleFicaDesabilitado(
        IbgeLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();

        options.UpdateEnableIbgeProvider(false, provider);

        Assert.False(options.GetProviderOptions(provider).IsEnabled);
        Assert.All(
            options.IbgeProviders.Where(other => other != provider),
            other => Assert.True(options.GetProviderOptions(other).IsEnabled));
    }

    [Fact]
    public void OrderIbgeProviders_SemProviders_LancaArgumentException()
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.OrderIbgeProviders());
    }

    [Theory]
    [InlineData(IbgeLookifyProviderEnum.Ibge)]
    [InlineData(IbgeLookifyProviderEnum.BrasilApi)]
    public void OrderIbgeProviders_QuandoRecebeUmUnicoProvider_ColocaEleNoInicioMantendoOrdemOriginalDosDemais(
        IbgeLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();
        var ordemOriginal = options.IbgeProviders.ToList();

        options.OrderIbgeProviders(provider);

        var esperado = new List<IbgeLookifyProviderEnum> { provider };
        esperado.AddRange(ordemOriginal.Where(other => other != provider));

        Assert.Equal(esperado, options.IbgeProviders);
    }

    [Fact]
    public void OrderIbgeProviders_QuandoRecebeTodosOsProvidersEmOrdemDiferente_AplicaExatamenteAOrdemInformada()
    {
        var options = new Lookify.LookifyOptions();

        options.OrderIbgeProviders(
            IbgeLookifyProviderEnum.BrasilApi,
            IbgeLookifyProviderEnum.Ibge);

        Assert.Equal(
            [
                IbgeLookifyProviderEnum.BrasilApi,
                IbgeLookifyProviderEnum.Ibge
            ],
            options.IbgeProviders);
    }
}
