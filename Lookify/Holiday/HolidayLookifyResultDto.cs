namespace Lookify.Holiday;

public sealed record class HolidayLookifyResultDto {
    public DateOnly? Date { get; init; }
    public string? Name { get; init; }
    public string? LocalName { get; init; }
    public string? Type { get; init; }
    public string? Weekday { get; init; }
}
