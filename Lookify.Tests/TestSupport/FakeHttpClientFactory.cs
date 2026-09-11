namespace Lookify.Tests.TestSupport;

internal sealed class FakeHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory {

    public HttpClient CreateClient(string name) =>
        new(handler, disposeHandler: false);
}
