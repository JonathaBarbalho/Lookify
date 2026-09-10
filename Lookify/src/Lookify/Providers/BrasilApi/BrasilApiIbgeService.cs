using Lookify.Ibge;

namespace Lookify.Providers.BrasilApi;

internal sealed partial class BrasilApiIbgeService : IIbgeProviderService {

    public string ProviderName { get; init; } = string.Empty;

    public string BaseAddress { get; init; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public void UpdateEnabled(bool isEnabled) =>
        IsEnabled = isEnabled;
}
