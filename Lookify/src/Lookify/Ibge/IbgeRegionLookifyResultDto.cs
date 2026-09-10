namespace Lookify.Ibge;

public sealed record class IbgeRegionLookifyResultDto {
    public int? Id { get; init; }
    public string? Name { get; init; }
    public string? Acronym { get; init; }
}
