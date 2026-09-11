namespace Lookify.Weather;

public sealed record class WeatherForecastLookifyResultDto {
    public DateOnly? Date { get; init; }
    public decimal? MinTemperature { get; init; }
    public decimal? MaxTemperature { get; init; }
    public string? ConditionCode { get; init; }
    public string? ConditionDescription { get; init; }
    public int? PrecipitationProbability { get; init; }
    public decimal? UvIndex { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
}
