namespace Lookify.Ibge;

public sealed record class IbgeCityLookifyResultDto {
    public int? Id { get; init; }
    public string? Name { get; init; }
    public int? StateId { get; init; }
    public string? StateUf { get; init; }
    public string? StateName { get; init; }
    public int? RegionId { get; init; }
    public string? RegionName { get; init; }
    public string? RegionAcronym { get; init; }
}
