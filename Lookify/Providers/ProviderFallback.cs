using Microsoft.Extensions.Logging;

namespace Lookify.Providers;

internal static class ProviderFallback {

    /// <summary>
    /// Executa a requisição em cada provedor habilitado, na ordem, até que um deles tenha sucesso.
    /// </summary>
    /// <param name="enabledProviders">Provedores habilitados, na ordem de tentativa.</param>
    /// <param name="operationDescription">Descrição da operação, usada em logs e mensagens de erro.</param>
    /// <param name="timeout">
    /// Tempo limite de cada tentativa de provedor. <see cref="Timeout.InfiniteTimeSpan"/> desativa o limite.
    /// </param>
    /// <param name="logger">Logger usado para registrar a falha de cada provedor.</param>
    /// <param name="request">Requisição ao provedor; deve observar o token recebido, e não o do chamador.</param>
    /// <param name="cancellationToken">Token do chamador; seu cancelamento aborta todo o fallback.</param>
    /// <exception cref="OperationCanceledException">Quando o chamador cancela a operação.</exception>
    /// <exception cref="InvalidOperationException">Quando nenhum provedor consegue atender a requisição.</exception>
    public static async Task<TResult> ExecuteAsync<TEnum, TResult>(
        IEnumerable<TEnum> enabledProviders,
        string operationDescription,
        TimeSpan timeout,
        ILogger logger,
        Func<TEnum, CancellationToken, Task<TResult>> request,
        CancellationToken cancellationToken)
    {
        var failures = new List<Exception>();

        foreach (var provider in enabledProviders) {
            cancellationToken.ThrowIfCancellationRequested();

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (timeout != Timeout.InfiniteTimeSpan) {
                timeoutSource.CancelAfter(timeout);
            }

            try {
                return await request(provider, timeoutSource.Token);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                throw;
            }
            catch (OperationCanceledException exception) when (timeoutSource.IsCancellationRequested) {
                var timeoutException = new TimeoutException(
                    $"O provedor {provider} excedeu o tempo limite de {timeout} ao consultar {operationDescription}.",
                    exception);
                failures.Add(timeoutException);
                logger.LogError(
                    timeoutException,
                    "Tempo limite excedido ao consultar {Operation} no provedor {Provider}.",
                    operationDescription,
                    provider);
            }
            catch (Exception exception) {
                failures.Add(exception);
                logger.LogError(
                    exception,
                    "Falha ao consultar {Operation} no provedor {Provider}.",
                    operationDescription,
                    provider);
            }
        }

        throw new InvalidOperationException(
            $"Não foi possível consultar {operationDescription} em nenhum provedor configurado.",
            new AggregateException(failures));
    }
}
