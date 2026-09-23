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
            Timeout.InfiniteTimeSpan,
            NullLogger.Instance,
            (provider, _) => {
                tentativas.Add(provider);
                return Task.FromResult("ok");
            },
            CancellationToken.None);

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
            Timeout.InfiniteTimeSpan,
            NullLogger.Instance,
            (provider, _) => {
                tentativas.Add(provider);
                if (provider == 1)
                    throw new InvalidOperationException("falhou");
                return Task.FromResult("ok");
            },
            CancellationToken.None);

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
                Timeout.InfiniteTimeSpan,
                NullLogger.Instance,
                (provider, _) => throw new InvalidOperationException($"falha {provider}"),
                CancellationToken.None));

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
                Timeout.InfiniteTimeSpan,
                NullLogger.Instance,
                (_, _) => Task.FromResult("nunca chamado"),
                CancellationToken.None));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Empty(aggregate.InnerExceptions);
    }

    private static readonly TimeSpan TimeoutCurto = TimeSpan.FromMilliseconds(100);

    private static async Task<string> PendurarAteCancelarAsync(CancellationToken token)
    {
        await Task.Delay(Timeout.Infinite, token);
        return "nunca retorna";
    }

    [Fact]
    public async Task ExecuteAsync_QuandoPrimeiroProviderEstouraTimeout_TentaProximoERetornaResultadoDele()
    {
        var tentativas = new List<int>();

        var resultado = await ProviderFallback.ExecuteAsync<int, string>(
            [1, 2],
            "operação",
            TimeoutCurto,
            NullLogger.Instance,
            (provider, token) => {
                tentativas.Add(provider);
                return provider == 1
                    ? PendurarAteCancelarAsync(token)
                    : Task.FromResult("ok 2");
            },
            CancellationToken.None);

        Assert.Equal("ok 2", resultado);
        Assert.Equal([1, 2], tentativas);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoTodosEstouramTimeout_LancaInvalidOperationExceptionComTimeoutExceptions()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ProviderFallback.ExecuteAsync<int, string>(
                [7, 9],
                "o recurso X",
                TimeoutCurto,
                NullLogger.Instance,
                (_, token) => PendurarAteCancelarAsync(token),
                CancellationToken.None));

        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Equal(2, aggregate.InnerExceptions.Count);
        Assert.All(
            aggregate.InnerExceptions.Zip(new[] { 7, 9 }, (inner, provider) => (inner, provider)),
            item => {
                var timeoutException = Assert.IsType<TimeoutException>(item.inner);
                Assert.Contains(item.provider.ToString(), timeoutException.Message);
                Assert.Contains("o recurso X", timeoutException.Message);
                Assert.IsAssignableFrom<OperationCanceledException>(timeoutException.InnerException);
            });
    }

    [Fact]
    public async Task ExecuteAsync_QuandoPrimeiroEstouraTimeout_ProximoRecebeTokenNaoCancelado()
    {
        var tokenCanceladoNoInicioDoSegundo = true;

        var resultado = await ProviderFallback.ExecuteAsync<int, string>(
            [1, 2],
            "operação",
            TimeoutCurto,
            NullLogger.Instance,
            (provider, token) => {
                if (provider == 1)
                    return PendurarAteCancelarAsync(token);

                tokenCanceladoNoInicioDoSegundo = token.IsCancellationRequested;
                return Task.FromResult("ok 2");
            },
            CancellationToken.None);

        Assert.Equal("ok 2", resultado);
        Assert.False(tokenCanceladoNoInicioDoSegundo);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoChamadorCancelaDuranteTentativa_PropagaOperationCanceledExceptionSemTentarProximo()
    {
        var tentativas = new List<int>();
        var primeiroIniciou = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellationSource = new CancellationTokenSource();

        var execucao = ProviderFallback.ExecuteAsync<int, string>(
            [1, 2],
            "operação",
            Timeout.InfiniteTimeSpan,
            NullLogger.Instance,
            (provider, token) => {
                tentativas.Add(provider);
                if (provider != 1)
                    return Task.FromResult("ok 2");

                primeiroIniciou.SetResult();
                return PendurarAteCancelarAsync(token);
            },
            cancellationSource.Token);
        await primeiroIniciou.Task;
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execucao);
        Assert.Equal([1], tentativas);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoTokenDoChamadorJaCancelado_PropagaOperationCanceledExceptionSemTentarProvedores()
    {
        var tentativas = new List<int>();
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ProviderFallback.ExecuteAsync<int, string>(
                [1, 2],
                "operação",
                TimeoutCurto,
                NullLogger.Instance,
                (provider, token) => {
                    tentativas.Add(provider);
                    token.ThrowIfCancellationRequested();
                    return Task.FromResult("ok");
                },
                cancellationSource.Token));

        Assert.True(tentativas.Count <= 1);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoProviderLancaTaskCanceledExceptionSemCancelamento_TrataComoFalhaComumSemEmbrulhar()
    {
        var tentativas = new List<int>();
        var cancelamentoDoProvedor = new TaskCanceledException("cancelado pelo provedor");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ProviderFallback.ExecuteAsync<int, string>(
                [1, 2],
                "operação",
                TimeoutCurto,
                NullLogger.Instance,
                (provider, _) => {
                    tentativas.Add(provider);
                    if (provider == 1)
                        throw cancelamentoDoProvedor;
                    throw new InvalidOperationException("falha 2");
                },
                CancellationToken.None));

        Assert.Equal([1, 2], tentativas);
        var aggregate = Assert.IsType<AggregateException>(exception.InnerException);
        Assert.Equal(2, aggregate.InnerExceptions.Count);
        Assert.Same(cancelamentoDoProvedor, aggregate.InnerExceptions[0]);
        Assert.IsType<InvalidOperationException>(aggregate.InnerExceptions[1]);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoTimeoutInfinito_TokenNaoCancelaERequisicaoCompleta()
    {
        CancellationToken tokenRecebido = default;

        var resultado = await ProviderFallback.ExecuteAsync<int, string>(
            [1],
            "operação",
            Timeout.InfiniteTimeSpan,
            NullLogger.Instance,
            async (_, token) => {
                tokenRecebido = token;
                await Task.Delay(TimeSpan.FromMilliseconds(50), token);
                return "ok";
            },
            CancellationToken.None);

        Assert.Equal("ok", resultado);
        Assert.False(tokenRecebido.IsCancellationRequested);
    }
}
