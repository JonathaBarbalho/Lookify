using Lookify.Bank;

namespace Lookify.Tests.LookifyOptionsTests;

public class BankLookifyOptionsTests {

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateEnableBankProvider_SemProviders_LancaArgumentException(bool isEnabled)
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.UpdateEnableBankProvider(isEnabled));
    }

    [Fact]
    public void UpdateEnableBankProvider_QuandoDesabilita_ProviderFicaDesabilitado()
    {
        var options = new Lookify.LookifyOptions();

        options.UpdateEnableBankProvider(false, BankLookifyProviderEnum.BrasilApi);

        Assert.False(options.GetProviderOptions(BankLookifyProviderEnum.BrasilApi).IsEnabled);
    }

    [Fact]
    public void OrderBankProviders_SemProviders_LancaArgumentException()
    {
        var options = new Lookify.LookifyOptions();

        Assert.Throws<ArgumentException>(() => options.OrderBankProviders());
    }

    [Fact]
    public void OrderBankProviders_QuandoRecebeOUnicoProvider_MantemNaLista()
    {
        var options = new Lookify.LookifyOptions();

        options.OrderBankProviders(BankLookifyProviderEnum.BrasilApi);

        Assert.Equal([BankLookifyProviderEnum.BrasilApi], options.BankProviders);
    }
}
