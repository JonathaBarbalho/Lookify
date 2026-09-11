using Lookify.Fipe;

namespace Lookify.Providers.BrasilApi;

internal sealed partial class BrasilApiFipeService : IFipeProviderService {

    public string ProviderName { get; init; } = string.Empty;

    public string BaseAddress { get; init; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public void UpdateEnabled(bool isEnabled) =>
        IsEnabled = isEnabled;
}
