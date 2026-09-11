namespace Lookify.VehiclePlate;

public sealed record class VehiclePlateLookifyResultDto {
    public string? Plate { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public int? ManufactureYear { get; init; }
    public int? ModelYear { get; init; }
    public string? Color { get; init; }
    public string? Chassis { get; init; }
    public string? Engine { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? Segment { get; init; }
    public string? SubSegment { get; init; }
    public string? Displacement { get; init; }
    public string? Fuel { get; init; }
    public List<VehiclePlateLookifyFipeMatch> FipeMatches { get; init; } = [];
}
