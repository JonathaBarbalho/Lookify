namespace Lookify.Cnpj;

public sealed record class CnpjLookifyResultDto {
    public string? Cnpj { get; init; }
    public string? CompanyName { get; init; }
    public string? TradeName { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? CompanyType { get; init; }
    public int? HeadquartersOrBranchIdentifier { get; init; }
    public string? HeadquartersOrBranchDescription { get; init; }
    public string? RegistrationStatusCode { get; init; }
    public string? RegistrationStatusDescription { get; init; }
    public string? RegistrationStatusReasonCode { get; init; }
    public string? RegistrationStatusReasonDescription { get; init; }
    public DateOnly? RegistrationStatusDate { get; init; }
    public DateOnly? SpecialSituationDate { get; init; }
    public DateOnly? ActivityStartDate { get; init; }
    public string? LegalNatureCode { get; init; }
    public string? LegalNatureDescription { get; init; }
    public string? ShareCapital { get; init; }
    public string? CompanySizeCode { get; init; }
    public string? CompanySizeDescription { get; init; }
    public bool? IsSimpleOptIn { get; init; }
    public DateOnly? SimpleOptInDate { get; init; }
    public DateOnly? SimpleOptOutDate { get; init; }
    public bool? IsMeiOptIn { get; init; }
    public string? PrimaryCnaeCode { get; init; }
    public string? PrimaryCnaeDescription { get; init; }
    public List<CnpjLookifySecondaryCnae> SecondaryCnaes { get; init; } = [];
    public string? StreetTypeDescription { get; init; }
    public string? Street { get; init; }
    public string? Number { get; init; }
    public string? Complement { get; init; }
    public string? Neighborhood { get; init; }
    public string? ZipCode { get; init; }
    public string? State { get; init; }
    public string? City { get; init; }
    public string? PrimaryPhoneAreaCode { get; init; }
    public string? SecondaryPhoneAreaCode { get; init; }
    public string? FaxAreaCode { get; init; }
    public string? SpecialSituation { get; init; }
    public string? ForeignCityName { get; init; }
    public string? Country { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public List<CnpjLookifyPartner> Partners { get; init; } = [];
}
