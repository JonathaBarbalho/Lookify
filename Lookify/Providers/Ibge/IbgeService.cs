using Lookify.Ibge;

namespace Lookify.Providers.Ibge;

internal sealed partial class IbgeService : IIbgeProviderService {

    public string ProviderName { get; init; } = string.Empty;

    public string BaseAddress { get; init; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public void UpdateEnabled(bool isEnabled) =>
        IsEnabled = isEnabled;
}
