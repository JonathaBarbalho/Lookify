namespace Lookify.Tests.TestSupport;

/// <summary>
/// Handler HTTP falso com resposta assíncrona, que recebe o token da requisição —
/// permite simular provedores que demoram ou ficam pendurados até o cancelamento.
/// </summary>
internal sealed class AsyncFakeHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder) : HttpMessageHandler {

    private readonly List<HttpRequestMessage> _requests = new();
    private readonly object _lock = new();

    public IReadOnlyList<HttpRequestMessage> Requests {
        get {
            lock (_lock) {
                return _requests.ToList();
            }
        }
    }

    /// <summary>
    /// Resposta que só termina quando o token da requisição é cancelado.
    /// </summary>
    public static async Task<HttpResponseMessage> HangUntilCanceledAsync(
        CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
        throw new InvalidOperationException("Task.Delay infinito não deveria terminar sem cancelamento.");
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        lock (_lock) {
            _requests.Add(request);
        }

        return responder(request, cancellationToken);
    }
}
