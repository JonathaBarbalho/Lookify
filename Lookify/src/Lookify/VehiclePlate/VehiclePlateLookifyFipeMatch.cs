namespace Lookify.VehiclePlate;

public sealed record class VehiclePlateLookifyFipeMatch {
    public decimal? Similarity { get; init; }
    public decimal? Correspondence { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public string? ModelYear { get; init; }
    public string? FipeCode { get; init; }
    public string? BrandCode { get; init; }
    public string? ModelCode { get; init; }
    public string? ReferenceMonth { get; init; }
    public string? Fuel { get; init; }
    public string? Value { get; init; }
    public string? ValueUnit { get; init; }
}
