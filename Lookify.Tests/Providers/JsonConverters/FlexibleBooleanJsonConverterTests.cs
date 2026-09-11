using System.Text.Json;
using System.Text.Json.Serialization;
using Lookify.Providers.JsonConverters;

namespace Lookify.Tests.Providers.JsonConverters;

public class FlexibleBooleanJsonConverterTests {

    private sealed record Wrapper(
        [property: JsonConverter(typeof(FlexibleBooleanJsonConverter))] bool? Value);

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("\"sim\"", true)]
    [InlineData("\"s\"", true)]
    [InlineData("\"yes\"", true)]
    [InlineData("\"y\"", true)]
    [InlineData("\"não\"", false)]
    [InlineData("\"nao\"", false)]
    [InlineData("\"n\"", false)]
    [InlineData("\"no\"", false)]
    [InlineData("\"1\"", true)]
    [InlineData("\"0\"", false)]
    [InlineData("\"true\"", true)]
    [InlineData("\"false\"", false)]
    public void Read_QuandoValorReconhecido_ConverteParaBooleano(string json, bool esperado)
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>($$"""{"Value":{{json}}}""");

        Assert.Equal(esperado, wrapper!.Value);
    }

    [Fact]
    public void Read_QuandoNulo_RetornaNull()
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>("""{"Value":null}""");

        Assert.Null(wrapper!.Value);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(2.5, true)]
    public void Read_QuandoNumero_ConverteConformeZeroOuNaoZero(double numero, bool esperado)
    {
        var json = numero.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var wrapper = JsonSerializer.Deserialize<Wrapper>($$"""{"Value":{{json}}}""");

        Assert.Equal(esperado, wrapper!.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Read_QuandoStringVaziaOuEmBranco_RetornaNull(string valor)
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>($$"""{"Value":"{{valor}}"}""");

        Assert.Null(wrapper!.Value);
    }

    [Fact]
    public void Read_QuandoStringNaoReconhecida_LancaJsonException()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Wrapper>("""{"Value":"talvez"}"""));
    }

    [Fact]
    public void Read_QuandoTokenNaoSuportado_LancaJsonException()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Wrapper>("""{"Value":[1,2]}"""));
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Write_QuandoValorPresente_EscreveBooleano(bool valor, string esperado)
    {
        var json = JsonSerializer.Serialize(new Wrapper(valor));

        Assert.Equal($$"""{"Value":{{esperado}}}""", json);
    }

    [Fact]
    public void Write_QuandoValorNulo_EscreveNull()
    {
        var json = JsonSerializer.Serialize(new Wrapper(null));

        Assert.Equal("""{"Value":null}""", json);
    }
}
