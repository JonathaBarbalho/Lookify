using Lookify.Cnpj;

namespace Lookify.Tests.LookifyOptionsTests;

public class CnpjLookifyOptionsTests {

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateEnableCnpjProvider_SemProviders_LancaArgumentException(bool isEnabled)
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.UpdateEnableCnpjProvider(isEnabled));
    }

    [Theory]
    [InlineData(CnpjLookifyProviderEnum.BrasilApi)]
    [InlineData(CnpjLookifyProviderEnum.ReceitaWs)]
    [InlineData(CnpjLookifyProviderEnum.Publica)]
    [InlineData(CnpjLookifyProviderEnum.MinhaReceita)]
    public void UpdateEnableCnpjProvider_QuandoDesabilitaUmProvider_SoAqueleFicaDesabilitado(
        CnpjLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();

        options.UpdateEnableCnpjProvider(false, provider);

        Assert.False(options.GetProviderOptions(provider).IsEnabled);
        Assert.All(
            options.CnpjProviders.Where(other => other != provider),
            other => Assert.True(options.GetProviderOptions(other).IsEnabled));
    }

    [Fact]
    public void OrderCnpjProviders_SemProviders_LancaArgumentException()
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.OrderCnpjProviders());
    }

    [Theory]
    [InlineData(CnpjLookifyProviderEnum.BrasilApi)]
    [InlineData(CnpjLookifyProviderEnum.ReceitaWs)]
    [InlineData(CnpjLookifyProviderEnum.Publica)]
    [InlineData(CnpjLookifyProviderEnum.MinhaReceita)]
    public void OrderCnpjProviders_QuandoRecebeUmUnicoProvider_ColocaEleNoInicioMantendoOrdemOriginalDosDemais(
        CnpjLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();
        var ordemOriginal = options.CnpjProviders.ToList();

        options.OrderCnpjProviders(provider);

        var esperado = new List<CnpjLookifyProviderEnum> { provider };
        esperado.AddRange(ordemOriginal.Where(other => other != provider));

        Assert.Equal(esperado, options.CnpjProviders);
    }

    [Fact]
    public void OrderCnpjProviders_QuandoRecebeTodosOsProvidersEmOrdemDiferente_AplicaExatamenteAOrdemInformada()
    {
        var options = new Lookify.LookifyOptions();

        options.OrderCnpjProviders(
            CnpjLookifyProviderEnum.MinhaReceita,
            CnpjLookifyProviderEnum.Publica,
            CnpjLookifyProviderEnum.ReceitaWs,
            CnpjLookifyProviderEnum.BrasilApi);

        Assert.Equal(
            [
                CnpjLookifyProviderEnum.MinhaReceita,
                CnpjLookifyProviderEnum.Publica,
                CnpjLookifyProviderEnum.ReceitaWs,
                CnpjLookifyProviderEnum.BrasilApi
            ],
            options.CnpjProviders);
    }
}
