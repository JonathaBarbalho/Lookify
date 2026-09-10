namespace Lookify.Providers;

internal interface IProviderRequest<TResult> {
    public static abstract string ProviderName { get; }

    public static abstract string BaseAddress { get; }

    public static abstract Task<TResult> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
