using Lookify.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Fipe;

internal sealed class FipeLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : IFipeLookifyService {

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LookifyOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    public async Task<List<FipeReferenceTableLookifyResultDto>> GetReferenceTablesAsync(
        CancellationToken cancellationToken = default)
    {
        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            "tabelas de referência FIPE",
            _logger,
            provider => GetService(provider).GetReferenceTablesAsync(
                _httpFactory,
                cancellationToken));
    }

    public async Task<List<FipeBrandLookifyResultDto>> GetBrandsAsync(
        FipeVehicleType vehicleType,
        int? referenceTable = null,
        CancellationToken cancellationToken = default)
    {
        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"marcas FIPE de {vehicleType}",
            _logger,
            provider => GetService(provider).GetBrandsAsync(
                vehicleType,
                referenceTable,
                _httpFactory,
                cancellationToken));
    }

    public async Task<List<FipeModelLookifyResultDto>> GetModelsAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        int? referenceTable = null,
        CancellationToken cancellationToken = default)
    {
        RequireNotEmpty(brandCode, nameof(brandCode));

        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"modelos FIPE da marca {brandCode}",
            _logger,
            provider => GetService(provider).GetModelsAsync(
                vehicleType,
                brandCode,
                referenceTable,
                _httpFactory,
                cancellationToken));
    }

    public async Task<List<FipeModelYearLookifyResultDto>> GetModelYearsAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        string modelCode,
        int? referenceTable = null,
        CancellationToken cancellationToken = default)
    {
        RequireNotEmpty(brandCode, nameof(brandCode));
        RequireNotEmpty(modelCode, nameof(modelCode));

        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"anos FIPE do modelo {modelCode}",
            _logger,
            provider => GetService(provider).GetModelYearsAsync(
                vehicleType,
                brandCode,
                modelCode,
                referenceTable,
                _httpFactory,
                cancellationToken));
    }

    public async Task<FipeVehiclePriceLookifyResultDto> GetVehiclePriceAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        string modelCode,
        string yearCode,
        int? referenceTable = null,
        CancellationToken cancellationToken = default)
    {
        RequireNotEmpty(brandCode, nameof(brandCode));
        RequireNotEmpty(modelCode, nameof(modelCode));
        RequireNotEmpty(yearCode, nameof(yearCode));

        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"valor FIPE do veículo {brandCode}/{modelCode}/{yearCode}",
            _logger,
            provider => GetService(provider).GetVehiclePriceAsync(
                vehicleType,
                brandCode,
                modelCode,
                yearCode,
                referenceTable,
                _httpFactory,
                cancellationToken));
    }

    public async Task<List<FipeVehiclePriceLookifyResultDto>> GetPriceByFipeCodeAsync(
        string fipeCode,
        int? referenceTable = null,
        CancellationToken cancellationToken = default)
    {
        RequireNotEmpty(fipeCode, nameof(fipeCode));

        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"valor FIPE do código {fipeCode}",
            _logger,
            provider => GetService(provider).GetPriceByFipeCodeAsync(
                fipeCode,
                referenceTable,
                _httpFactory,
                cancellationToken));
    }

    private static void RequireNotEmpty(
        string value,
        string parameterName)
    {
        if (string.IsNullOrEmpty(value)) {
            throw new ArgumentException($"{parameterName} cannot be null or empty.", parameterName);
        }
    }

    private List<FipeLookifyProviderEnum> GetEnabledProviders() =>
        _options.FipeProviders
            .Where(provider => _options.GetProviderOptions(provider).IsEnabled)
            .ToList();

    private static IProvider GetProvider(FipeLookifyProviderEnum provider) =>
        provider switch {
            FipeLookifyProviderEnum.BrasilApi => ProviderRegistry.BrasilApi,
            FipeLookifyProviderEnum.Parallelum => ProviderRegistry.Parallelum,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };

    private static IFipeProviderService GetService(FipeLookifyProviderEnum provider) =>
        GetProvider(provider).Services
            .OfType<IFipeProviderService>()
            .First();
}
