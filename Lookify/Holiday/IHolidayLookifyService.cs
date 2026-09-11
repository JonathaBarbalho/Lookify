namespace Lookify.Holiday;

public interface IHolidayLookifyService {

    Task<List<HolidayLookifyResultDto>> GetHolidaysAsync(
        int year,
        CancellationToken cancellationToken = default);
}
