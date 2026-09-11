namespace Lookify.Tests.TestSupport;

internal sealed class FakeHttpMessageHandler(
    Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler {

    private readonly List<HttpRequestMessage> _requests = new();

    public IReadOnlyList<HttpRequestMessage> Requests => _requests;

    public HttpRequestMessage? LastRequest => _requests.Count > 0 ? _requests[^1] : null;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _requests.Add(request);
        return Task.FromResult(responder(request));
    }
}
