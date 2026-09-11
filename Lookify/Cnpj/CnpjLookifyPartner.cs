namespace Lookify.Cnpj;

public sealed record class CnpjLookifyPartner {
    public string? Identifier { get; init; }
    public string? Name { get; init; }
    public string? Document { get; init; }
    public string? QualificationCode { get; init; }
    public string? QualificationDescription { get; init; }
    public DateOnly? EntryDate { get; init; }
    public string? Country { get; init; }
    public string? LegalRepresentativeDocument { get; init; }
    public string? LegalRepresentativeName { get; init; }
    public string? LegalRepresentativeQualificationCode { get; init; }
    public string? AgeGroup { get; init; }
}
