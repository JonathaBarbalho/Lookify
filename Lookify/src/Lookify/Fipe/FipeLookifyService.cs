using Lookify.Providers.BrasilApi;
using Lookify.Providers.Parallelum;
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
        return await ExecuteAsync(
            "tabelas de referência FIPE",
            provider => provider switch {
                FipeLookifyProviderEnum.BrasilApi =>
                    BrasilApiFipeReferenceTableProviderRequest.RequestAsync(
                        _httpFactory,
                        cancellationToken),
                FipeLookifyProviderEnum.Parallelum =>
                    ParallelumFipeReferenceTableProviderRequest.RequestAsync(
                        _httpFactory,
                        cancellationToken),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
    }

    public async Task<List<FipeBrandLookifyResultDto>> GetBrandsAsync(
        FipeVehicleType vehicleType,
        int? referenceTable = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            $"marcas FIPE de {vehicleType}",
            provider => provider switch {
                FipeLookifyProviderEnum.BrasilApi =>
                    BrasilApiFipeBrandProviderRequest.RequestAsync(
                        vehicleType,
                        referenceTable,
                        _httpFactory,
                        cancellationToken),
                FipeLookifyProviderEnum.Parallelum =>
                    ParallelumFipeBrandProviderRequest.RequestAsync(
                        vehicleType,
                        referenceTable,
                        _httpFactory,
                        cancellationToken),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
    }

    public async Task<List<FipeModelLookifyResultDto>> GetModelsAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        int? referenceTable = null,
        CancellationToken cancellationToken = default)
    {
        RequireNotEmpty(brandCode, nameof(brandCode));

        return await ExecuteAsync(
            $"modelos FIPE da marca {brandCode}",
            provider => provider switch {
                FipeLookifyProviderEnum.BrasilApi =>
                    BrasilApiFipeModelProviderRequest.RequestAsync(
                        vehicleType,
                        brandCode,
                        referenceTable,
                        _httpFactory,
                        cancellationToken),
                FipeLookifyProviderEnum.Parallelum =>
                    ParallelumFipeModelProviderRequest.RequestAsync(
                        vehicleType,
                        brandCode,
                        referenceTable,
                        _httpFactory,
                        cancellationToken),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
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

        return await ExecuteAsync(
            $"anos FIPE do modelo {modelCode}",
            provider => provider switch {
                FipeLookifyProviderEnum.BrasilApi =>
                    BrasilApiFipeModelYearProviderRequest.RequestAsync(
                        vehicleType,
                        brandCode,
                        modelCode,
                        referenceTable,
                        _httpFactory,
                        cancellationToken),
                FipeLookifyProviderEnum.Parallelum =>
                    ParallelumFipeModelYearProviderRequest.RequestAsync(
                        vehicleType,
                        brandCode,
                        modelCode,
                        referenceTable,
                        _httpFactory,
                        cancellationToken),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
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

        return await ExecuteAsync(
            $"valor FIPE do veículo {brandCode}/{modelCode}/{yearCode}",
            provider => provider switch {
                FipeLookifyProviderEnum.BrasilApi =>
                    BrasilApiFipeVehiclePriceProviderRequest.RequestDetailsAsync(
                        vehicleType,
                        brandCode,
                        modelCode,
                        yearCode,
                        referenceTable,
                        _httpFactory,
                        cancellationToken),
                FipeLookifyProviderEnum.Parallelum =>
                    ParallelumFipeVehiclePriceProviderRequest.RequestDetailsAsync(
                        vehicleType,
                        brandCode,
                        modelCode,
                        yearCode,
                        referenceTable,
                        _httpFactory,
                        cancellationToken),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
    }

    public async Task<List<FipeVehiclePriceLookifyResultDto>> GetPriceByFipeCodeAsync(
        string fipeCode,
        int? referenceTable = null,
        CancellationToken cancellationToken = default)
    {
        RequireNotEmpty(fipeCode, nameof(fipeCode));

        return await ExecuteAsync(
            $"valor FIPE do código {fipeCode}",
            provider => provider switch {
                FipeLookifyProviderEnum.BrasilApi =>
                    BrasilApiFipeVehiclePriceProviderRequest.RequestByFipeCodeAsync(
                        fipeCode,
                        referenceTable,
                        _httpFactory,
                        cancellationToken),
                FipeLookifyProviderEnum.Parallelum =>
                    throw new NotSupportedException(
                        $"Provider {FipeLookifyProviderEnum.Parallelum} não oferece busca por código FIPE direto."),
                _ => throw new NotSupportedException($"Provider {provider} not supported")
            });
    }

    private async Task<TResult> ExecuteAsync<TResult>(
        string operationDescription,
        Func<FipeLookifyProviderEnum, Task<TResult>> request)
    {
        var enabledProviders = GetEnabledProviders();
        var failures = new List<Exception>();

        foreach (var provider in enabledProviders) {
            try {
                return await request(provider);
            }
            catch (Exception exception) {
                failures.Add(exception);
                _logger.LogError(
                    exception,
                    "Falha ao consultar {Operation} no provedor {Provider}.",
                    operationDescription,
                    provider);
            }
        }

        throw new InvalidOperationException(
            $"Não foi possível consultar {operationDescription} em nenhum provedor configurado.",
            new AggregateException(failures));
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
            .Where(provider => _options.GetProviderOptions(provider).Enabled)
            .ToList();
}
