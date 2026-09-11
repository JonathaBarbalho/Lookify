using System.Text.Json;
using System.Text.Json.Serialization;
using Lookify.Providers.JsonConverters;

namespace Lookify.Tests.Providers.JsonConverters;

public class FlexibleDecimalJsonConverterTests {

    private sealed record Wrapper(
        [property: JsonConverter(typeof(FlexibleDecimalJsonConverter))] decimal? Value);

    [Fact]
    public void Read_QuandoNumero_ConverteParaDecimal()
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>("""{"Value":12.5}""");

        Assert.Equal(12.5m, wrapper!.Value);
    }

    [Fact]
    public void Read_QuandoStringNumerica_ConverteParaDecimalComCulturaInvariante()
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>("""{"Value":"1234.56"}""");

        Assert.Equal(1234.56m, wrapper!.Value);
    }

    [Fact]
    public void Read_QuandoNulo_RetornaNull()
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>("""{"Value":null}""");

        Assert.Null(wrapper!.Value);
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
    public void Read_QuandoStringInvalida_LancaJsonException()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Wrapper>("""{"Value":"abc"}"""));
    }

    [Fact]
    public void Read_QuandoTokenNaoSuportado_LancaJsonException()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Wrapper>("""{"Value":true}"""));
    }

    [Fact]
    public void Write_QuandoValorPresente_EscreveNumero()
    {
        var json = JsonSerializer.Serialize(new Wrapper(12.5m));

        Assert.Equal("""{"Value":12.5}""", json);
    }

    [Fact]
    public void Write_QuandoValorNulo_EscreveNull()
    {
        var json = JsonSerializer.Serialize(new Wrapper(null));

        Assert.Equal("""{"Value":null}""", json);
    }
}
