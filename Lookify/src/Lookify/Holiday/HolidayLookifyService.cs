using Lookify.Providers.BrasilApi;
using Lookify.Providers.NagerDate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Holiday;

internal sealed class HolidayLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : IHolidayLookifyService {

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LookifyOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    public async Task<List<HolidayLookifyResultDto>> GetHolidaysAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        var enabledProviders = GetEnabledProviders();
        var failures = new List<Exception>();

        foreach (var provider in enabledProviders) {
            try {
                return provider switch {
                    HolidayLookifyProviderEnum.BrasilApi =>
                        await BrasilApiHolidayProviderRequest.RequestAsync(
                            year,
                            _httpFactory,
                            cancellationToken),
                    HolidayLookifyProviderEnum.NagerDate =>
                        await NagerDateHolidayProviderRequest.RequestAsync(
                            year,
                            _httpFactory,
                            cancellationToken),
                    _ => throw new NotSupportedException($"Provider {provider} not supported")
                };
            }
            catch (Exception exception) {
                failures.Add(exception);
                _logger.LogError(
                    exception,
                    "Falha ao consultar os feriados de {Year} no provedor {Provider}.",
                    year,
                    provider);
            }
        }

        throw new InvalidOperationException(
            $"Não foi possível consultar os feriados de {year} em nenhum provedor configurado.",
            new AggregateException(failures));
    }

    private List<HolidayLookifyProviderEnum> GetEnabledProviders() =>
        _options.HolidayProviders
            .Where(provider => _options.GetProviderOptions(provider).Enabled)
            .ToList();
}
