namespace Lookify.Fipe;

public sealed record class FipeVehiclePriceLookifyResultDto {
    public string? FipeCode { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public int? ModelYear { get; init; }
    public string? Fuel { get; init; }
    public string? FuelAcronym { get; init; }
    public string? Value { get; init; }
    public string? ReferenceMonth { get; init; }
    public int? VehicleTypeCode { get; init; }
    public string? RequestDate { get; init; }
}
