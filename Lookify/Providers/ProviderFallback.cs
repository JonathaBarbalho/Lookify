using Microsoft.Extensions.Logging;

namespace Lookify.Providers;

internal static class ProviderFallback {

    public static async Task<TResult> ExecuteAsync<TEnum, TResult>(
        IEnumerable<TEnum> enabledProviders,
        string operationDescription,
        ILogger logger,
        Func<TEnum, Task<TResult>> request)
    {
        var failures = new List<Exception>();

        foreach (var provider in enabledProviders) {
            try {
                return await request(provider);
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
