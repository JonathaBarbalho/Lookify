using Lookify.Providers;
using Lookify.Providers.BrasilApi;
using Lookify.Providers.NagerDate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.Holiday;

internal sealed class HolidayLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : IHolidayLookifyService {

    private static readonly BrasilApiProvider _brasilApi = new();
    private static readonly NagerDateProvider _nagerDate = new();

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LookifyOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    public async Task<List<HolidayLookifyResultDto>> GetHolidaysAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"os feriados de {year}",
            _logger,
            provider => GetService(provider).GetHolidaysAsync(
                year,
                _httpFactory,
                cancellationToken));
    }

    private List<HolidayLookifyProviderEnum> GetEnabledProviders() =>
        _options.HolidayProviders
            .Where(provider => _options.GetProviderOptions(provider).IsEnabled)
            .ToList();

    private static IProvider GetProvider(HolidayLookifyProviderEnum provider) =>
        provider switch {
            HolidayLookifyProviderEnum.BrasilApi => _brasilApi,
            HolidayLookifyProviderEnum.NagerDate => _nagerDate,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };

    private static IHolidayProviderService GetService(HolidayLookifyProviderEnum provider) =>
        GetProvider(provider).Services
            .OfType<IHolidayProviderService>()
            .First();
}
