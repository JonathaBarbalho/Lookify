namespace Lookify.Cep.Dto;

public sealed record class CepLookifyResult {

    public string? ZipCode { get; init; }

    public string? Street { get; init; }

    public string? Complement { get; init; }

    public string? Neighborhood { get; init; }

    public string? City { get; init; }

    public string? State { get; init; }

    public string? IbgeCityCode { get; init; }
}
