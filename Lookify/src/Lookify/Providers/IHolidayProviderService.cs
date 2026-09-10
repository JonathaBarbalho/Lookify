using Lookify.Holiday;

namespace Lookify.Providers;

internal interface IHolidayProviderService : IProviderService {

    Task<List<HolidayLookifyResultDto>> GetHolidaysAsync(
        int year,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default);
}
