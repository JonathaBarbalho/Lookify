using Lookify.Providers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lookify.Tests.Providers;

public class ProviderFallbackTests {

    [Fact]
    public async Task ExecuteAsync_QuandoPrimeiroProviderSucede_NaoTentaOsDemais()
    {
        var tentativas = new List<int>();

        var resultado = await ProviderFallback.ExecuteAsync<int, string>(
            [1, 2, 3],
            "operação",
            NullLogger.Instance,
            provider => {
                tentativas.Add(provider);
                return Task.FromResult("ok");
            });

        Assert.Equal("ok", resultado);
        Assert.Equal([1], tentativas);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoProviderFalhaEProximoSucede_RetornaResultadoDoProximo()
    {
        var tentativas = new List<int>();

        var resultado = await ProviderFallback.ExecuteAsync<int, string>(
            [1, 2],
            "operação",
            NullLogger.Instance,
            provider => {
                tentativas.Add(provider);
                if (provider == 1)
                    throw new InvalidOperationException("falhou");
                return Task.FromResult("ok");
            });

        Assert.Equal("ok", resultado);
        Assert.Equal([1, 2], tentativas);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoTodosFalham_LancaInvalidOperationExceptionComAggregateException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ProviderFallback.ExecuteAsync<int, string>(
                [1, 2, 3],
                "operação",
                NullLogger.Instance,
                provider => throw new InvalidOperationException($"falha {provider}")));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Equal(3, aggregate.InnerExceptions.Count);
        Assert.Equal(
            ["falha 1", "falha 2", "falha 3"],
            aggregate.InnerExceptions.Select(inner => inner.Message));
    }

    [Fact]
    public async Task ExecuteAsync_QuandoListaVazia_LancaInvalidOperationExceptionComAggregateExceptionVazia()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ProviderFallback.ExecuteAsync<int, string>(
                [],
                "operação",
                NullLogger.Instance,
                _ => Task.FromResult("nunca chamado")));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Empty(aggregate.InnerExceptions);
    }
}
