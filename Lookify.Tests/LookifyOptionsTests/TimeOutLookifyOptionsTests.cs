namespace Lookify.Tests.LookifyOptionsTests;

public class TimeOutLookifyOptionsTests {

    private const double MaximoSuportadoEmMilissegundos = uint.MaxValue - 1;

    public static TheoryData<TimeSpan> ValoresValidos => new() {
        TimeSpan.FromMilliseconds(1),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMilliseconds(MaximoSuportadoEmMilissegundos),
        Timeout.InfiniteTimeSpan
    };

    public static TheoryData<TimeSpan> ValoresInvalidos => new() {
        TimeSpan.Zero,
        TimeSpan.FromMilliseconds(-2),
        TimeSpan.FromSeconds(-1),
        TimeSpan.FromMilliseconds(MaximoSuportadoEmMilissegundos + 1),
        TimeSpan.MaxValue,
        TimeSpan.MinValue
    };

    [Fact]
    public void TimeOut_QuandoNaoConfigurado_UsaPadraoDeTresMinutos()
    {
        var options = new Lookify.LookifyOptions();

        Assert.Equal(TimeSpan.FromMinutes(3), options.TimeOut);
    }

    [Theory]
    [MemberData(nameof(ValoresValidos))]
    public void TimeOut_QuandoValorValido_AceitaEDevolveOValor(TimeSpan valor)
    {
        var options = new Lookify.LookifyOptions();

        options.TimeOut = valor;

        Assert.Equal(valor, options.TimeOut);
    }

    [Theory]
    [MemberData(nameof(ValoresInvalidos))]
    public void TimeOut_QuandoValorInvalido_LancaArgumentOutOfRangeExceptionEPreservaValorAnterior(TimeSpan valor)
    {
        var options = new Lookify.LookifyOptions {
            TimeOut = TimeSpan.FromSeconds(10)
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => options.TimeOut = valor);

        Assert.Equal(TimeSpan.FromSeconds(10), options.TimeOut);
    }
}
