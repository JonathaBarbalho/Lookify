using Lookify.Cep;

namespace Lookify.Tests.LookifyOptionsTests;

public class CepLookifyOptionsTests {

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateEnableCepProvider_SemProviders_LancaArgumentException(bool isEnabled)
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.UpdateEnableCepProvider(isEnabled));
    }

    [Theory]
    [InlineData(CepLookifyProviderEnum.ViaCep)]
    [InlineData(CepLookifyProviderEnum.BrasilApi)]
    [InlineData(CepLookifyProviderEnum.OpenCep)]
    [InlineData(CepLookifyProviderEnum.AwesomeApi)]
    public void UpdateEnableCepProvider_QuandoDesabilitaUmProvider_SoAqueleFicaDesabilitado(
        CepLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();

        options.UpdateEnableCepProvider(false, provider);

        Assert.False(options.GetProviderOptions(provider).IsEnabled);
        Assert.All(
            options.CepProviders.Where(other => other != provider),
            other => Assert.True(options.GetProviderOptions(other).IsEnabled));
    }

    [Fact]
    public void OrderCepProviders_SemProviders_LancaArgumentException()
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.OrderCepProviders());
    }

    [Theory]
    [InlineData(CepLookifyProviderEnum.ViaCep)]
    [InlineData(CepLookifyProviderEnum.BrasilApi)]
    [InlineData(CepLookifyProviderEnum.OpenCep)]
    [InlineData(CepLookifyProviderEnum.AwesomeApi)]
    public void OrderCepProviders_QuandoRecebeUmUnicoProvider_ColocaEleNoInicioMantendoOrdemOriginalDosDemais(
        CepLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();
        var ordemOriginal = options.CepProviders.ToList();

        options.OrderCepProviders(provider);

        var esperado = new List<CepLookifyProviderEnum> { provider };
        esperado.AddRange(ordemOriginal.Where(other => other != provider));

        Assert.Equal(esperado, options.CepProviders);
    }

    [Fact]
    public void OrderCepProviders_QuandoRecebeTodosOsProvidersEmOrdemDiferente_AplicaExatamenteAOrdemInformada()
    {
        var options = new Lookify.LookifyOptions();

        options.OrderCepProviders(
            CepLookifyProviderEnum.AwesomeApi,
            CepLookifyProviderEnum.OpenCep,
            CepLookifyProviderEnum.ViaCep,
            CepLookifyProviderEnum.BrasilApi);

        Assert.Equal(
            [
                CepLookifyProviderEnum.AwesomeApi,
                CepLookifyProviderEnum.OpenCep,
                CepLookifyProviderEnum.ViaCep,
                CepLookifyProviderEnum.BrasilApi
            ],
            options.CepProviders);
    }
}
