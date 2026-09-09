using Lookify.Cep.Dto;

namespace Lookify.Cep.Providers;

internal interface IProviderRequest {
    public static abstract string ProviderName { get; }
    
    public static abstract string BaseAddress { get; }

    public static abstract Task<CepLookifyResult> RequestAsync(
        string zipCode,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
