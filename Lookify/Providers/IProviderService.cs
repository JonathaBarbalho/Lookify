namespace Lookify.Providers;

internal interface IProviderService {

    public string ProviderName { get; init; }

    public string BaseAddress { get; init; }

    public bool IsEnabled { get; }

    public void UpdateEnabled(bool isEnabled);

    
}