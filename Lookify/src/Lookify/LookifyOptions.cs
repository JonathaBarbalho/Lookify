using Lookify.Bank;
using Lookify.Cep;
using Lookify.Cnpj;
using Lookify.Fipe;
using Lookify.Holiday;
using Lookify.Ibge;
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

    #region CEP
    public CepLookifyProviderOptions ViaCep { get; set; } = new() {
        BaseAddress = ViaCepProviderRequest.BaseAddress
    };

    public CepLookifyProviderOptions BrasilApi { get; set; } = new() {
        BaseAddress = BrasilApiCepProviderRequest.BaseAddress
    };

    public CepLookifyProviderOptions CepOpenCep { get; set; } = new() {
        BaseAddress = OpenCepProviderRequest.BaseAddress
    };

    public CepLookifyProviderOptions CepAwesomeApi { get; set; } = new() {
        BaseAddress = AwesomeApiCepProviderRequest.BaseAddress
    };

    public List<CepLookifyProviderEnum> CepProviders { get; set; } = new() {
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
    public CnpjLookifyProviderOptions CnpjBrasilApi { get; set; } = new() {
        BaseAddress = BrasilApiCnpjProviderRequest.BaseAddress
    };

    public CnpjLookifyProviderOptions CnpjReceitaWs { get; set; } = new() {
        BaseAddress = ReceitaWsProviderRequest.BaseAddress
    };

    public CnpjLookifyProviderOptions CnpjPublica { get; set; } = new() {
        BaseAddress = PublicaProviderRequest.BaseAddress
    };

    public CnpjLookifyProviderOptions CnpjMinhaReceita { get; set; } = new() {
        BaseAddress = MinhaReceitaProviderRequest.BaseAddress
    };

    public List<CnpjLookifyProviderEnum> CnpjProviders { get; set; } = new() {
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
    /// Token do provedor PlacaFipe. Por padrão vem da variável de ambiente
    /// <c>LOOKIFY_PLACAFIPE_TOKEN</c> — nunca deve ser hardcoded ou commitado.
    /// </summary>
    public VehiclePlateLookifyProviderOptions PlacaFipe { get; set; } = new() {
        BaseAddress = PlacaFipeProviderRequest.BaseAddress,
        Token = Environment.GetEnvironmentVariable("LOOKIFY_PLACAFIPE_TOKEN") ?? string.Empty
    };

    public List<VehiclePlateLookifyProviderEnum> VehiclePlateProviders { get; set; } = new() {
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
    public FipeLookifyProviderOptions FipeBrasilApi { get; set; } = new() {
        BaseAddress = BrasilApiFipeBrandProviderRequest.BaseAddress
    };

    public FipeLookifyProviderOptions FipeParallelum { get; set; } = new() {
        BaseAddress = ParallelumFipeBrandProviderRequest.BaseAddress
    };

    public List<FipeLookifyProviderEnum> FipeProviders { get; set; } = new() {
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
    public IbgeLookifyProviderOptions Ibge { get; set; } = new() {
        BaseAddress = IbgeStateProviderRequest.BaseAddress
    };

    public IbgeLookifyProviderOptions IbgeBrasilApi { get; set; } = new() {
        BaseAddress = BrasilApiIbgeStateProviderRequest.BaseAddress
    };

    public List<IbgeLookifyProviderEnum> IbgeProviders { get; set; } = new() {
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
    public BankLookifyProviderOptions BankBrasilApi { get; set; } = new() {
        BaseAddress = BrasilApiBankProviderRequest.BaseAddress
    };

    public List<BankLookifyProviderEnum> BankProviders { get; set; } = new() {
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
    public HolidayLookifyProviderOptions HolidayBrasilApi { get; set; } = new() {
        BaseAddress = BrasilApiHolidayProviderRequest.BaseAddress
    };

    public HolidayLookifyProviderOptions HolidayNagerDate { get; set; } = new() {
        BaseAddress = NagerDateHolidayProviderRequest.BaseAddress
    };

    public List<HolidayLookifyProviderEnum> HolidayProviders { get; set; } = new() {
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
    public WeatherLookifyProviderOptions WeatherOpenMeteo { get; set; } = new() {
        BaseAddress = OpenMeteoWeatherProviderRequest.BaseAddress
    };

    public WeatherLookifyProviderOptions WeatherCptec { get; set; } = new() {
        BaseAddress = CptecWeatherProviderRequest.BaseAddress
    };

    public List<WeatherLookifyProviderEnum> WeatherProviders { get; set; } = new() {
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
