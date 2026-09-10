namespace Lookify.Providers;

internal interface IProviderService {

    public string ProviderName { get; }

    public string BaseAddress { get; }

    public bool IsEnabled { get; }

    public void UpdateEnabled(bool isEnabled);

    
}