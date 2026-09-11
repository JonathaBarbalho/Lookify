namespace Lookify.Bank;

public sealed record class BankLookifyResultDto {
    public int? Code { get; init; }
    public string? Ispb { get; init; }
    public string? Name { get; init; }
    public string? FullName { get; init; }
    public string? Cnpj { get; init; }
    public string? Street { get; init; }
    public string? Number { get; init; }
    public string? Complement { get; init; }
    public string? District { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? LogoUrl { get; init; }
}
