namespace Lookify.Providers;

internal interface IProviderOptions {

    bool IsEnabled { get; }

    void UpdateEnabled(bool isEnabled);
}
