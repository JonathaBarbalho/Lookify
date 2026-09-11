using System.Net;
using System.Text;
using Lookify.Providers;

namespace Lookify.Tests.Providers;

public class ProviderRequestDeserializationTests {

    private sealed record SampleResponse(string Value);

    [Fact]
    public async Task ReadAndDeserializeAsync_QuandoRespostaComSucesso_DeserializaCorretamente()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("""{"Value":"ok"}""", Encoding.UTF8, "application/json")
        };

        var result = await ProviderRequestDeserialization.ReadAndDeserializeAsync<SampleResponse>(
            "Provider", "identificador", response);

        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public async Task ReadAndDeserializeAsync_QuandoStatusDeErro_LancaHttpRequestExceptionComProviderEIdentificadorNoTexto()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound) {
            Content = new StringContent("não encontrado", Encoding.UTF8, "text/plain")
        };

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            ProviderRequestDeserialization.ReadAndDeserializeAsync<SampleResponse>(
                "Provider", "identificador", response));

        Assert.Contains("Provider", exception.Message);
        Assert.Contains("identificador", exception.Message);
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task ReadAndDeserializeAsync_QuandoCorpoInvalido_LancaInvalidOperationException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{ isso não é json válido", Encoding.UTF8, "application/json")
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ProviderRequestDeserialization.ReadAndDeserializeAsync<SampleResponse>(
                "Provider", "identificador", response));

        Assert.Contains("Provider", exception.Message);
        Assert.Contains("identificador", exception.Message);
    }

    [Fact]
    public async Task ReadAndDeserializeAsync_QuandoJsonDeserializaParaNull_LancaInvalidOperationException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ProviderRequestDeserialization.ReadAndDeserializeAsync<SampleResponse>(
                "Provider", "identificador", response));

        Assert.Contains("Provider", exception.Message);
        Assert.Contains("identificador", exception.Message);
    }

    [Fact]
    public async Task ReadAndDeserializeAsync_QuandoProviderNameNulo_LancaArgumentNullException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("""{"Value":"ok"}""", Encoding.UTF8, "application/json")
        };

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            ProviderRequestDeserialization.ReadAndDeserializeAsync<SampleResponse>(
                null!, "identificador", response));
    }

    [Fact]
    public async Task ReadAndDeserializeAsync_QuandoCorpoMuitoLongo_TruncaNaMensagemDeErro()
    {
        var corpoLongo = new string('x', 3000);
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError) {
            Content = new StringContent(corpoLongo, Encoding.UTF8, "text/plain")
        };

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            ProviderRequestDeserialization.ReadAndDeserializeAsync<SampleResponse>(
                "Provider", "identificador", response));

        Assert.Contains("...", exception.Message);
        Assert.DoesNotContain(corpoLongo, exception.Message);
    }
}
