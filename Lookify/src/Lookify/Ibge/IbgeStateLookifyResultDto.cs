namespace Lookify.Ibge;

public sealed record class IbgeStateLookifyResultDto {
    public int? Id { get; init; }
    public string? Name { get; init; }
    public string? Uf { get; init; }
    public int? RegionId { get; init; }
    public string? RegionName { get; init; }
    public string? RegionAcronym { get; init; }
}
