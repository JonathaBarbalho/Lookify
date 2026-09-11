namespace Lookify.Providers;

internal interface IProviderEnablement {

    bool IsEnabled { get; }

    void UpdateEnabled(bool isEnabled);
}
