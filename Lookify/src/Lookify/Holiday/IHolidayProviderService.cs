using Lookify.Providers;

namespace Lookify.Holiday;

internal interface IHolidayProviderService : IProviderService {

    Task<List<HolidayLookifyResultDto>> GetHolidaysAsync(
        int year,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
