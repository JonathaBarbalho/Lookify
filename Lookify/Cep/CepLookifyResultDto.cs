namespace Lookify.Cep;

public sealed record class CepLookifyResultDto {

    public string? ZipCode { get; init; }

    public string? Street { get; init; }

    public string? Complement { get; init; }

    public string? Neighborhood { get; init; }

    public string? City { get; init; }

    public string? State { get; init; }

    public string? IbgeCityCode { get; init; }

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public string? Ddd { get; init; }
}
