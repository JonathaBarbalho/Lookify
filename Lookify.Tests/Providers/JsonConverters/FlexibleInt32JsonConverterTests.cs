using System.Text.Json;
using System.Text.Json.Serialization;
using Lookify.Providers.JsonConverters;

namespace Lookify.Tests.Providers.JsonConverters;

public class FlexibleInt32JsonConverterTests {

    private sealed record Wrapper(
        [property: JsonConverter(typeof(FlexibleInt32JsonConverter))] int? Value);

    [Fact]
    public void Read_QuandoNumero_ConverteParaInt()
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>("""{"Value":42}""");

        Assert.Equal(42, wrapper!.Value);
    }

    [Fact]
    public void Read_QuandoStringNumerica_ConverteParaInt()
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>("""{"Value":"42"}""");

        Assert.Equal(42, wrapper!.Value);
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
    public void Read_QuandoStringComOverflow_LancaJsonException()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Wrapper>("""{"Value":"99999999999"}"""));
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
        var json = JsonSerializer.Serialize(new Wrapper(42));

        Assert.Equal("""{"Value":42}""", json);
    }

    [Fact]
    public void Write_QuandoValorNulo_EscreveNull()
    {
        var json = JsonSerializer.Serialize(new Wrapper(null));

        Assert.Equal("""{"Value":null}""", json);
    }
}
