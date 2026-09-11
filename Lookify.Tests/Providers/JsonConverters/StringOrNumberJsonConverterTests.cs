using System.Text.Json;
using System.Text.Json.Serialization;
using Lookify.Providers.JsonConverters;

namespace Lookify.Tests.Providers.JsonConverters;

public class StringOrNumberJsonConverterTests {

    private sealed record Wrapper(
        [property: JsonConverter(typeof(StringOrNumberJsonConverter))] string? Value);

    [Fact]
    public void Read_QuandoString_MantemValor()
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>("""{"Value":"abc"}""");

        Assert.Equal("abc", wrapper!.Value);
    }

    [Fact]
    public void Read_QuandoNumeroInteiro_ConverteParaString()
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>("""{"Value":123}""");

        Assert.Equal("123", wrapper!.Value);
    }

    [Fact]
    public void Read_QuandoNumeroDecimal_ConverteParaStringComCulturaInvariante()
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>("""{"Value":12.50}""");

        Assert.Equal("12.50", wrapper!.Value);
    }

    [Fact]
    public void Read_QuandoNulo_RetornaNull()
    {
        var wrapper = JsonSerializer.Deserialize<Wrapper>("""{"Value":null}""");

        Assert.Null(wrapper!.Value);
    }

    [Fact]
    public void Read_QuandoTokenNaoSuportado_LancaJsonException()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Wrapper>("""{"Value":true}"""));
    }

    [Fact]
    public void Write_QuandoValorPresente_EscreveString()
    {
        var json = JsonSerializer.Serialize(new Wrapper("abc"));

        Assert.Equal("""{"Value":"abc"}""", json);
    }

    [Fact]
    public void Write_QuandoValorNulo_EscreveNull()
    {
        var json = JsonSerializer.Serialize(new Wrapper(null));

        Assert.Equal("""{"Value":null}""", json);
    }
}
