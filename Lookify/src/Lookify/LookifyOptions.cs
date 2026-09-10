using Lookify.Bank;
using Lookify.Cep;
using Lookify.Cnpj;
using Lookify.Fipe;
using Lookify.Holiday;
using Lookify.Ibge;
using Lookify.Providers;
using Lookify.Providers.AwesomeApi;
using Lookify.Providers.BrasilApi;
using Lookify.Providers.Cptec;
using Lookify.Providers.Ibge;
using Lookify.Providers.MinhaReceita;
using Lookify.Providers.NagerDate;
using Lookify.Providers.OpenCep;
using Lookify.Providers.OpenMeteo;
using Lookify.Providers.Parallelum;
using Lookify.Providers.PlacaFipe;
using Lookify.Providers.Publica;
using Lookify.Providers.ReceitaWs;
using Lookify.Providers.ViaCep;
using Lookify.VehiclePlate;
using Lookify.Weather;

namespace Lookify;

public sealed class LookifyOptions {

    /// <summary>
    /// Name of the software who will use this library. This is used to identify the software in the user agent string.
    /// </summary>
    public string UserAgent { get; set; } = "Lookify/1.0";

    public TimeSpan TimeOut { get; set; } = TimeSpan.FromMinutes(3);

    private static void UpdateEnabled<TOptions>(
        bool isEnabled,
        IEnumerable<TOptions> providers) where TOptions : IProviderOptions
    {
        foreach (var provider in providers)
            provider.UpdateEnabled(isEnabled);
    }

    private static List<TEnum> ReorderProviders<TEnum>(
        List<TEnum> currentOrder,
        TEnum[] providersEnums) where TEnum : struct, Enum
    {
        if (providersEnums.Length == 0)
            throw new ArgumentException("At least one provider must be specified.", nameof(providersEnums));

        var orderedProviders = new List<TEnum>(providersEnums);
        orderedProviders.AddRange(currentOrder.Except(orderedProviders));
        return orderedProviders;
    }

    #region CEP
    /// <summary>
    /// Enables or disables the specified CEP providers.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the providers should be enabled.</param>
    /// <param name="providersEnums">The CEP providers to enable.</param>
    /// <exception cref="ArgumentException"></exception>
    public void UpdateEnableCepProvider(
        bool isEnabled,
        params CepLookifyProviderEnum[] providersEnums)
    {
        if (providersEnums.Length == 0)
            throw new ArgumentException("At least one provider must be specified.", nameof(providersEnums));

        UpdateEnabled(isEnabled, providersEnums.Select(GetProviderOptions));
    }

    /// <summary>
    /// Orders the CEP providers based on the specified order. Providers not included in the order will be placed at the end in their original order.
    /// </summary>
    /// <param name="providersEnums">The CEP providers to order.</param>
    /// <exception cref="ArgumentException"></exception>
    public void OrderCepProviders(params CepLookifyProviderEnum[] providersEnums) =>
        CepProviders = ReorderProviders(CepProviders, providersEnums);

    private CepLookifyProviderOptions ViaCep { get; set; } = new(
        ViaCepProvider.BaseAddress);
    private CepLookifyProviderOptions BrasilApi { get; set; } = new(
        BrasilApiProvider.BaseAddress);
    private CepLookifyProviderOptions CepOpenCep { get; set; } = new(
        OpenCepProvider.BaseAddress);
    private CepLookifyProviderOptions CepAwesomeApi { get; set; } = new(
        AwesomeApiProvider.BaseAddress);

    internal List<CepLookifyProviderEnum> CepProviders { get; set; } = new() {
        CepLookifyProviderEnum.ViaCep,
        CepLookifyProviderEnum.BrasilApi,
        CepLookifyProviderEnum.OpenCep,
        CepLookifyProviderEnum.AwesomeApi
    };

    internal CepLookifyProviderOptions GetProviderOptions(
        CepLookifyProviderEnum provider)
    {
        return provider switch {
            CepLookifyProviderEnum.ViaCep => ViaCep,
            CepLookifyProviderEnum.BrasilApi => BrasilApi,
            CepLookifyProviderEnum.OpenCep => CepOpenCep,
            CepLookifyProviderEnum.AwesomeApi => CepAwesomeApi,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
    #endregion

    #region CNPJ
    /// <summary>
    /// Enables or disables the specified CNPJ providers.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the providers should be enabled.</param>
    /// <param name="providersEnums">The CNPJ providers to enable.</param>
    /// <exception cref="ArgumentException"></exception>
    public void UpdateEnableCnpjProvider(
        bool isEnabled,
        params CnpjLookifyProviderEnum[] providersEnums)
    {
        if (providersEnums.Length == 0)
            throw new ArgumentException("At least one provider must be specified.", nameof(providersEnums));

        UpdateEnabled(isEnabled, providersEnums.Select(GetProviderOptions));
    }

    /// <summary>
    /// Orders the CNPJ providers based on the specified order. Providers not included in the order will be placed at the end in their original order.
    /// </summary>
    /// <param name="providersEnums">The CNPJ providers to order.</param>
    /// <exception cref="ArgumentException"></exception>
    public void OrderCnpjProviders(params CnpjLookifyProviderEnum[] providersEnums) =>
        CnpjProviders = ReorderProviders(CnpjProviders, providersEnums);

    private CnpjLookifyProviderOptions CnpjBrasilApi { get; set; } = new(
        BrasilApiProvider.BaseAddress);
    private CnpjLookifyProviderOptions CnpjReceitaWs { get; set; } = new(
        ReceitaWsProvider.BaseAddress);
    private CnpjLookifyProviderOptions CnpjPublica { get; set; } = new(
        PublicaProvider.BaseAddress);
    private CnpjLookifyProviderOptions CnpjMinhaReceita { get; set; } = new(
        MinhaReceitaProvider.BaseAddress);

    internal List<CnpjLookifyProviderEnum> CnpjProviders { get; set; } = new() {
        CnpjLookifyProviderEnum.BrasilApi,
        CnpjLookifyProviderEnum.ReceitaWs,
        CnpjLookifyProviderEnum.Publica,
        CnpjLookifyProviderEnum.MinhaReceita
    };

    internal CnpjLookifyProviderOptions GetProviderOptions(
        CnpjLookifyProviderEnum provider)
    {
        return provider switch {
            CnpjLookifyProviderEnum.BrasilApi => CnpjBrasilApi,
            CnpjLookifyProviderEnum.ReceitaWs => CnpjReceitaWs,
            CnpjLookifyProviderEnum.Publica => CnpjPublica,
            CnpjLookifyProviderEnum.MinhaReceita => CnpjMinhaReceita,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
    #endregion

    #region VEHICLE PLATE
    /// <summary>
    /// Enables or disables the specified vehicle plate providers.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the providers should be enabled.</param>
    /// <param name="providersEnums">The vehicle plate providers to enable.</param>
    /// <exception cref="ArgumentException"></exception>
    public void UpdateEnableVehiclePlateProvider(
        bool isEnabled,
        params VehiclePlateLookifyProviderEnum[] providersEnums)
    {
        if (providersEnums.Length == 0)
            throw new ArgumentException("At least one provider must be specified.", nameof(providersEnums));

        UpdateEnabled(isEnabled, providersEnums.Select(GetProviderOptions));
    }

    /// <summary>
    /// Orders the vehicle plate providers based on the specified order. Providers not included in the order will be placed at the end in their original order.
    /// </summary>
    /// <param name="providersEnums">The vehicle plate providers to order.</param>
    /// <exception cref="ArgumentException"></exception>
    public void OrderVehiclePlateProviders(params VehiclePlateLookifyProviderEnum[] providersEnums) =>
        VehiclePlateProviders = ReorderProviders(VehiclePlateProviders, providersEnums);

    /// <summary>
    /// Sets the token used to authenticate against the PlacaFipe provider.
    /// </summary>
    /// <param name="token">The PlacaFipe API token.</param>
    public void SetPlacaFipeToken(string token) =>
        PlacaFipe.Token = token;

    /// <summary>
    /// Token do provedor PlacaFipe. Por padrão vem da variável de ambiente
    /// <c>LOOKIFY_PLACAFIPE_TOKEN</c> — nunca deve ser hardcoded ou commitado.
    /// </summary>
    private VehiclePlateLookifyProviderOptions PlacaFipe { get; set; } = new(
        PlacaFipeProvider.BaseAddress) {
        Token = Environment.GetEnvironmentVariable("LOOKIFY_PLACAFIPE_TOKEN") ?? string.Empty
    };

    internal List<VehiclePlateLookifyProviderEnum> VehiclePlateProviders { get; set; } = new() {
        VehiclePlateLookifyProviderEnum.PlacaFipe
    };

    internal VehiclePlateLookifyProviderOptions GetProviderOptions(
        VehiclePlateLookifyProviderEnum provider)
    {
        return provider switch {
            VehiclePlateLookifyProviderEnum.PlacaFipe => PlacaFipe,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
    #endregion

    #region FIPE
    /// <summary>
    /// Enables or disables the specified FIPE providers.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the providers should be enabled.</param>
    /// <param name="providersEnums">The FIPE providers to enable.</param>
    /// <exception cref="ArgumentException"></exception>
    public void UpdateEnableFipeProvider(
        bool isEnabled,
        params FipeLookifyProviderEnum[] providersEnums)
    {
        if (providersEnums.Length == 0)
            throw new ArgumentException("At least one provider must be specified.", nameof(providersEnums));

        UpdateEnabled(isEnabled, providersEnums.Select(GetProviderOptions));
    }

    /// <summary>
    /// Orders the FIPE providers based on the specified order. Providers not included in the order will be placed at the end in their original order.
    /// </summary>
    /// <param name="providersEnums">The FIPE providers to order.</param>
    /// <exception cref="ArgumentException"></exception>
    public void OrderFipeProviders(params FipeLookifyProviderEnum[] providersEnums) =>
        FipeProviders = ReorderProviders(FipeProviders, providersEnums);

    private FipeLookifyProviderOptions FipeBrasilApi { get; set; } = new(
        BrasilApiProvider.BaseAddress);
    private FipeLookifyProviderOptions FipeParallelum { get; set; } = new(
        ParallelumProvider.BaseAddress);

    internal List<FipeLookifyProviderEnum> FipeProviders { get; set; } = new() {
        FipeLookifyProviderEnum.BrasilApi,
        FipeLookifyProviderEnum.Parallelum
    };

    internal FipeLookifyProviderOptions GetProviderOptions(
        FipeLookifyProviderEnum provider)
    {
        return provider switch {
            FipeLookifyProviderEnum.BrasilApi => FipeBrasilApi,
            FipeLookifyProviderEnum.Parallelum => FipeParallelum,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
    #endregion

    #region IBGE
    /// <summary>
    /// Enables or disables the specified IBGE providers.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the providers should be enabled.</param>
    /// <param name="providersEnums">The IBGE providers to enable.</param>
    /// <exception cref="ArgumentException"></exception>
    public void UpdateEnableIbgeProvider(
        bool isEnabled,
        params IbgeLookifyProviderEnum[] providersEnums)
    {
        if (providersEnums.Length == 0)
            throw new ArgumentException("At least one provider must be specified.", nameof(providersEnums));

        UpdateEnabled(isEnabled, providersEnums.Select(GetProviderOptions));
    }

    /// <summary>
    /// Orders the IBGE providers based on the specified order. Providers not included in the order will be placed at the end in their original order.
    /// </summary>
    /// <param name="providersEnums">The IBGE providers to order.</param>
    /// <exception cref="ArgumentException"></exception>
    public void OrderIbgeProviders(params IbgeLookifyProviderEnum[] providersEnums) =>
        IbgeProviders = ReorderProviders(IbgeProviders, providersEnums);

    private IbgeLookifyProviderOptions Ibge { get; set; } = new(
        IbgeProvider.BaseAddress);
    private IbgeLookifyProviderOptions IbgeBrasilApi { get; set; } = new(
        BrasilApiProvider.BaseAddress);

    internal List<IbgeLookifyProviderEnum> IbgeProviders { get; set; } = new() {
        IbgeLookifyProviderEnum.Ibge,
        IbgeLookifyProviderEnum.BrasilApi
    };

    internal IbgeLookifyProviderOptions GetProviderOptions(
        IbgeLookifyProviderEnum provider)
    {
        return provider switch {
            IbgeLookifyProviderEnum.Ibge => Ibge,
            IbgeLookifyProviderEnum.BrasilApi => IbgeBrasilApi,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
    #endregion

    #region BANK
    /// <summary>
    /// Enables or disables the specified bank providers.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the providers should be enabled.</param>
    /// <param name="providersEnums">The bank providers to enable.</param>
    /// <exception cref="ArgumentException"></exception>
    public void UpdateEnableBankProvider(
        bool isEnabled,
        params BankLookifyProviderEnum[] providersEnums)
    {
        if (providersEnums.Length == 0)
            throw new ArgumentException("At least one provider must be specified.", nameof(providersEnums));

        UpdateEnabled(isEnabled, providersEnums.Select(GetProviderOptions));
    }

    /// <summary>
    /// Orders the bank providers based on the specified order. Providers not included in the order will be placed at the end in their original order.
    /// </summary>
    /// <param name="providersEnums">The bank providers to order.</param>
    /// <exception cref="ArgumentException"></exception>
    public void OrderBankProviders(params BankLookifyProviderEnum[] providersEnums) =>
        BankProviders = ReorderProviders(BankProviders, providersEnums);

    private BankLookifyProviderOptions BankBrasilApi { get; set; } = new(
        BrasilApiProvider.BaseAddress);

    internal List<BankLookifyProviderEnum> BankProviders { get; set; } = new() {
        BankLookifyProviderEnum.BrasilApi
    };

    internal BankLookifyProviderOptions GetProviderOptions(
        BankLookifyProviderEnum provider)
    {
        return provider switch {
            BankLookifyProviderEnum.BrasilApi => BankBrasilApi,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
    #endregion

    #region HOLIDAY
    /// <summary>
    /// Enables or disables the specified holiday providers.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the providers should be enabled.</param>
    /// <param name="providersEnums">The holiday providers to enable.</param>
    /// <exception cref="ArgumentException"></exception>
    public void UpdateEnableHolidayProvider(
        bool isEnabled,
        params HolidayLookifyProviderEnum[] providersEnums)
    {
        if (providersEnums.Length == 0)
            throw new ArgumentException("At least one provider must be specified.", nameof(providersEnums));

        UpdateEnabled(isEnabled, providersEnums.Select(GetProviderOptions));
    }

    /// <summary>
    /// Orders the holiday providers based on the specified order. Providers not included in the order will be placed at the end in their original order.
    /// </summary>
    /// <param name="providersEnums">The holiday providers to order.</param>
    /// <exception cref="ArgumentException"></exception>
    public void OrderHolidayProviders(params HolidayLookifyProviderEnum[] providersEnums) =>
        HolidayProviders = ReorderProviders(HolidayProviders, providersEnums);

    private HolidayLookifyProviderOptions HolidayBrasilApi { get; set; } = new(
        BrasilApiProvider.BaseAddress);
    private HolidayLookifyProviderOptions HolidayNagerDate { get; set; } = new(
        NagerDateProvider.BaseAddress);

    internal List<HolidayLookifyProviderEnum> HolidayProviders { get; set; } = new() {
        HolidayLookifyProviderEnum.BrasilApi,
        HolidayLookifyProviderEnum.NagerDate
    };

    internal HolidayLookifyProviderOptions GetProviderOptions(
        HolidayLookifyProviderEnum provider)
    {
        return provider switch {
            HolidayLookifyProviderEnum.BrasilApi => HolidayBrasilApi,
            HolidayLookifyProviderEnum.NagerDate => HolidayNagerDate,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
    #endregion

    #region WEATHER
    /// <summary>
    /// Enables or disables the specified weather providers.
    /// </summary>
    /// <param name="isEnabled">Indicates whether the providers should be enabled.</param>
    /// <param name="providersEnums">The weather providers to enable.</param>
    /// <exception cref="ArgumentException"></exception>
    public void UpdateEnableWeatherProvider(
        bool isEnabled,
        params WeatherLookifyProviderEnum[] providersEnums)
    {
        if (providersEnums.Length == 0)
            throw new ArgumentException("At least one provider must be specified.", nameof(providersEnums));

        UpdateEnabled(isEnabled, providersEnums.Select(GetProviderOptions));
    }

    /// <summary>
    /// Orders the weather providers based on the specified order. Providers not included in the order will be placed at the end in their original order.
    /// </summary>
    /// <param name="providersEnums">The weather providers to order.</param>
    /// <exception cref="ArgumentException"></exception>
    public void OrderWeatherProviders(params WeatherLookifyProviderEnum[] providersEnums) =>
        WeatherProviders = ReorderProviders(WeatherProviders, providersEnums);

    private WeatherLookifyProviderOptions WeatherOpenMeteo { get; set; } = new(
        OpenMeteoProvider.BaseAddress);
    private WeatherLookifyProviderOptions WeatherCptec { get; set; } = new(
        CptecProvider.BaseAddress);

    internal List<WeatherLookifyProviderEnum> WeatherProviders { get; set; } = new() {
        WeatherLookifyProviderEnum.OpenMeteo,
        WeatherLookifyProviderEnum.Cptec
    };

    internal WeatherLookifyProviderOptions GetProviderOptions(
        WeatherLookifyProviderEnum provider)
    {
        return provider switch {
            WeatherLookifyProviderEnum.OpenMeteo => WeatherOpenMeteo,
            WeatherLookifyProviderEnum.Cptec => WeatherCptec,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
    #endregion
}
