using System.Globalization;
using System.Text.Json.Serialization;
using Lookify.Cnpj;
using Lookify.Providers.JsonConverters;

namespace Lookify.Providers.ReceitaWs;

internal sealed class ReceitaWsCnpjService : ICnpjProviderService {

    public string ProviderName { get; init; } = string.Empty;

    public string BaseAddress { get; init; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public void UpdateEnabled(bool isEnabled) =>
        IsEnabled = isEnabled;

    public async Task<CnpjLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}v1/cnpj/{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<ReceitaWsProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}

internal sealed record class ReceitaWsProviderResponse {

    [property: JsonPropertyName("cnpj")]
    public string? Cnpj { get; init; }

    [property: JsonPropertyName("tipo")]
    public string? Tipo { get; init; }

    [property: JsonPropertyName("abertura")]
    public string? Abertura { get; init; }

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }

    [property: JsonPropertyName("fantasia")]
    public string? Fantasia { get; init; }

    [property: JsonPropertyName("atividade_principal")]
    public List<ReceitaWsActivityResponse> AtividadePrincipal { get; init; } = [];

    [property: JsonPropertyName("atividades_secundarias")]
    public List<ReceitaWsActivityResponse> AtividadesSecundarias { get; init; } = [];

    [property: JsonPropertyName("natureza_juridica")]
    public string? NaturezaJuridica { get; init; }

    [property: JsonPropertyName("logradouro")]
    public string? Logradouro { get; init; }

    [property: JsonPropertyName("numero")]
    public string? Numero { get; init; }

    [property: JsonPropertyName("complemento")]
    public string? Complemento { get; init; }

    [property: JsonPropertyName("bairro")]
    public string? Bairro { get; init; }

    [property: JsonPropertyName("municipio")]
    public string? Municipio { get; init; }

    [property: JsonPropertyName("uf")]
    public string? Uf { get; init; }

    [property: JsonPropertyName("cep")]
    public string? Cep { get; init; }

    [property: JsonPropertyName("email")]
    public string? Email { get; init; }

    [property: JsonPropertyName("telefone")]
    public string? Telefone { get; init; }

    [property: JsonPropertyName("situacao")]
    public string? Situacao { get; init; }

    [property: JsonPropertyName("data_situacao")]
    public string? DataSituacao { get; init; }

    [property: JsonPropertyName("motivo_situacao")]
    public string? MotivoSituacao { get; init; }

    [property: JsonPropertyName("situacao_especial")]
    public string? SituacaoEspecial { get; init; }

    [property: JsonPropertyName("data_situacao_especial")]
    public string? DataSituacaoEspecial { get; init; }

    [property: JsonPropertyName("capital_social")]
    [property: JsonConverter(typeof(StringOrNumberJsonConverter))]
    public string? CapitalSocial { get; init; }

    [property: JsonPropertyName("qsa")]
    public List<ReceitaWsPartnerResponse> Qsa { get; init; } = [];

    public CnpjLookifyResultDto ToResult() =>
        new CnpjLookifyResultDto {
            Cnpj = Cnpj,
            CompanyName = Nome,
            TradeName = Fantasia,
            Phone = Telefone,
            Email = Email,
            CompanyType = Tipo,
            RegistrationStatusDescription = Situacao,
            RegistrationStatusReasonDescription = MotivoSituacao,
            RegistrationStatusDate = ParseDateOnly(DataSituacao),
            SpecialSituationDate = ParseDateOnly(DataSituacaoEspecial),
            ActivityStartDate = ParseDateOnly(Abertura),
            LegalNatureCode = NaturezaJuridica,
            ShareCapital = CapitalSocial,
            PrimaryCnaeCode = AtividadePrincipal.FirstOrDefault()?.Code,
            PrimaryCnaeDescription = AtividadePrincipal.FirstOrDefault()?.Text,
            SecondaryCnaes = AtividadesSecundarias.Select(
                activity => new CnpjLookifySecondaryCnae {
                    Code = activity.Code,
                    Description = activity.Text
                }).ToList(),
            Street = Logradouro,
            Number = Numero,
            Complement = Complemento,
            Neighborhood = Bairro,
            ZipCode = Cep,
            State = Uf,
            City = Municipio,
            SpecialSituation = SituacaoEspecial,
            Partners = Qsa.Select(
                partner => new CnpjLookifyPartner {
                    Name = partner.Nome,
                    Document = partner.Documento,
                    QualificationCode = partner.Qualificacao,
                    QualificationDescription = partner.Qualificacao,
                    EntryDate = ParseDateOnly(partner.DataEntrada),
                    Country = partner.Pais,
                    LegalRepresentativeDocument = partner.DocumentoRepresentanteLegal,
                    LegalRepresentativeName = partner.NomeRepresentanteLegal,
                    LegalRepresentativeQualificationCode = partner.QualificacaoRepresentanteLegal,
                    AgeGroup = partner.FaixaEtaria
                }).ToList()
        };

    private static DateOnly? ParseDateOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var formats = new[] { "dd/MM/yyyy", "yyyy-MM-dd" };
        return DateOnly.TryParseExact(
            value,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed
            : DateOnly.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed)
                ? parsed
                : null;
    }
}

internal sealed record class ReceitaWsActivityResponse {

    [property: JsonPropertyName("code")]
    public string? Code { get; init; }

    [property: JsonPropertyName("text")]
    public string? Text { get; init; }
}

internal sealed record class ReceitaWsPartnerResponse {

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }

    [property: JsonPropertyName("qual")]
    public string? Qualificacao { get; init; }

    [property: JsonPropertyName("data_entrada")]
    public string? DataEntrada { get; init; }

    [property: JsonPropertyName("cpf_cnpj")]
    public string? Documento { get; init; }

    [property: JsonPropertyName("cpf_representante_legal")]
    public string? DocumentoRepresentanteLegal { get; init; }

    [property: JsonPropertyName("nome_representante_legal")]
    public string? NomeRepresentanteLegal { get; init; }

    [property: JsonPropertyName("qual_representante_legal")]
    public string? QualificacaoRepresentanteLegal { get; init; }

    [property: JsonPropertyName("pais")]
    public string? Pais { get; init; }

    [property: JsonPropertyName("faixa_etaria")]
    public string? FaixaEtaria { get; init; }
}
