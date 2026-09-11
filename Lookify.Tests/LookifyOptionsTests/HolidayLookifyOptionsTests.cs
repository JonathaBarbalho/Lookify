using Lookify.Holiday;

namespace Lookify.Tests.LookifyOptionsTests;

public class HolidayLookifyOptionsTests {

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateEnableHolidayProvider_SemProviders_LancaArgumentException(bool isEnabled)
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.UpdateEnableHolidayProvider(isEnabled));
    }

    [Theory]
    [InlineData(HolidayLookifyProviderEnum.BrasilApi)]
    [InlineData(HolidayLookifyProviderEnum.NagerDate)]
    public void UpdateEnableHolidayProvider_QuandoDesabilitaUmProvider_SoAqueleFicaDesabilitado(
        HolidayLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();

        options.UpdateEnableHolidayProvider(false, provider);

        Assert.False(options.GetProviderOptions(provider).IsEnabled);
        Assert.All(
            options.HolidayProviders.Where(other => other != provider),
            other => Assert.True(options.GetProviderOptions(other).IsEnabled));
    }

    [Fact]
    public void OrderHolidayProviders_SemProviders_LancaArgumentException()
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.OrderHolidayProviders());
    }

    [Theory]
    [InlineData(HolidayLookifyProviderEnum.BrasilApi)]
    [InlineData(HolidayLookifyProviderEnum.NagerDate)]
    public void OrderHolidayProviders_QuandoRecebeUmUnicoProvider_ColocaEleNoInicioMantendoOrdemOriginalDosDemais(
        HolidayLookifyProviderEnum provider)
    {
        var options = new Lookify.LookifyOptions();
        var ordemOriginal = options.HolidayProviders.ToList();

        options.OrderHolidayProviders(provider);

        var esperado = new List<HolidayLookifyProviderEnum> { provider };
        esperado.AddRange(ordemOriginal.Where(other => other != provider));

        Assert.Equal(esperado, options.HolidayProviders);
    }

    [Fact]
    public void OrderHolidayProviders_QuandoRecebeTodosOsProvidersEmOrdemDiferente_AplicaExatamenteAOrdemInformada()
    {
        var options = new Lookify.LookifyOptions();

        options.OrderHolidayProviders(
            HolidayLookifyProviderEnum.NagerDate,
            HolidayLookifyProviderEnum.BrasilApi);

        Assert.Equal(
            [
                HolidayLookifyProviderEnum.NagerDate,
                HolidayLookifyProviderEnum.BrasilApi
            ],
            options.HolidayProviders);
    }
}
