using System.Globalization;
using System.Text.Json.Serialization;
using Lookify.Cnpj;
using Lookify.Providers.JsonConverters;

namespace Lookify.Providers.BrasilApi;

internal sealed class BrasilApiCnpjService : ICnpjProviderService {

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
        var fullAddress = $"{BaseAddress}api/cnpj/v1/{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Cnpj");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<BrasilApiCnpjProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}

internal sealed record class BrasilApiCnpjProviderResponse {

    [property: JsonPropertyName("cnpj")]
    public string? Cnpj { get; init; }

    [property: JsonPropertyName("identificador_matriz_filial")]
    public int IdentificadorMatrizFilial { get; init; }

    [property: JsonPropertyName("descricao_matriz_filial")]
    public string? DescricaoMatrizFilial { get; init; }

    [property: JsonPropertyName("razao_social")]
    public string? RazaoSocial { get; init; }

    [property: JsonPropertyName("nome_fantasia")]
    public string? NomeFantasia { get; init; }

    [property: JsonPropertyName("situacao_cadastral")]
    public int SituacaoCadastral { get; init; }

    [property: JsonPropertyName("descricao_situacao_cadastral")]
    public string? DescricaoSituacaoCadastral { get; init; }

    [property: JsonPropertyName("data_situacao_cadastral")]
    public DateOnly? DataSituacaoCadastral { get; init; }

    [property: JsonPropertyName("motivo_situacao_cadastral")]
    public int MotivoSituacaoCadastral { get; init; }

    [property: JsonPropertyName("nome_cidade_exterior")]
    public string? NomeCidadeExterior { get; init; }

    [property: JsonPropertyName("codigo_natureza_juridica")]
    public int CodigoNaturezaJuridica { get; init; }

    [property: JsonPropertyName("data_inicio_atividade")]
    public DateOnly? DataInicioAtividade { get; init; }

    [property: JsonPropertyName("cnae_fiscal")]
    public int CnaeFiscal { get; init; }

    [property: JsonPropertyName("cnae_fiscal_descricao")]
    public string? CnaeFiscalDescricao { get; init; }

    [property: JsonPropertyName("descricao_tipo_de_logradouro")]
    public string? DescricaoTipoDeLogradouro { get; init; }

    [property: JsonPropertyName("logradouro")]
    public string? Logradouro { get; init; }

    [property: JsonPropertyName("numero")]
    public string? Numero { get; init; }

    [property: JsonPropertyName("complemento")]
    public string? Complemento { get; init; }

    [property: JsonPropertyName("bairro")]
    public string? Bairro { get; init; }

    [property: JsonPropertyName("cep")]
    public string? Cep { get; init; }

    [property: JsonPropertyName("uf")]
    public string? Uf { get; init; }

    [property: JsonPropertyName("municipio")]
    public string? Municipio { get; init; }

    [property: JsonPropertyName("ddd_telefone_1")]
    public string? DddTelefone1 { get; init; }

    [property: JsonPropertyName("ddd_telefone_2")]
    public string? DddTelefone2 { get; init; }

    [property: JsonPropertyName("ddd_fax")]
    public string? DddFax { get; init; }

    [property: JsonPropertyName("capital_social")]
    [property: JsonConverter(typeof(StringOrNumberJsonConverter))]
    public string? CapitalSocial { get; init; }

    [property: JsonPropertyName("porte")]
    public string? Porte { get; init; }

    [property: JsonPropertyName("descricao_porte")]
    public string? DescricaoPorte { get; init; }

    [property: JsonPropertyName("opcao_pelo_simples")]
    public bool OpcaoPeloSimples { get; init; }

    [property: JsonPropertyName("data_opcao_pelo_simples")]
    public DateOnly? DataOpcaoPeloSimples { get; init; }

    [property: JsonPropertyName("data_exclusao_do_simples")]
    public DateOnly? DataExclusaoDoSimples { get; init; }

    [property: JsonPropertyName("opcao_pelo_mei")]
    public bool OpcaoPeloMei { get; init; }

    [property: JsonPropertyName("situacao_especial")]
    public string? SituacaoEspecial { get; init; }

    [property: JsonPropertyName("data_situacao_especial")]
    public DateOnly? DataSituacaoEspecial { get; init; }

    [property: JsonPropertyName("qsa")]
    public List<BrasilApiCnpjPartnerResponse> Qsa { get; init; } = [];

    public CnpjLookifyResultDto ToResult() =>
        new CnpjLookifyResultDto {
            Cnpj = Cnpj,
            CompanyName = RazaoSocial,
            TradeName = NomeFantasia,
            Phone = FirstNonEmpty(DddTelefone1, DddTelefone2, DddFax),
            CompanyType = DescricaoMatrizFilial,
            HeadquartersOrBranchIdentifier = IdentificadorMatrizFilial,
            HeadquartersOrBranchDescription = DescricaoMatrizFilial,
            RegistrationStatusCode = SituacaoCadastral.ToString(CultureInfo.InvariantCulture),
            RegistrationStatusDescription = DescricaoSituacaoCadastral,
            RegistrationStatusReasonCode = MotivoSituacaoCadastral.ToString(CultureInfo.InvariantCulture),
            RegistrationStatusDate = DataSituacaoCadastral,
            SpecialSituationDate = DataSituacaoEspecial,
            ActivityStartDate = DataInicioAtividade,
            LegalNatureCode = CodigoNaturezaJuridica.ToString(CultureInfo.InvariantCulture),
            ShareCapital = CapitalSocial,
            CompanySizeCode = Porte,
            CompanySizeDescription = DescricaoPorte,
            IsSimpleOptIn = OpcaoPeloSimples,
            SimpleOptInDate = DataOpcaoPeloSimples,
            SimpleOptOutDate = DataExclusaoDoSimples,
            IsMeiOptIn = OpcaoPeloMei,
            PrimaryCnaeCode = CnaeFiscal.ToString(CultureInfo.InvariantCulture),
            PrimaryCnaeDescription = CnaeFiscalDescricao,
            StreetTypeDescription = DescricaoTipoDeLogradouro,
            Street = Logradouro,
            Number = Numero,
            Complement = Complemento,
            Neighborhood = Bairro,
            ZipCode = Cep,
            State = Uf,
            City = Municipio,
            PrimaryPhoneAreaCode = DddTelefone1,
            SecondaryPhoneAreaCode = DddTelefone2,
            FaxAreaCode = DddFax,
            SpecialSituation = SituacaoEspecial,
            ForeignCityName = NomeCidadeExterior,
            Partners = Qsa.Select(
                partner => new CnpjLookifyPartner {
                    Identifier = partner.IdentificadorDeSocio.ToString(CultureInfo.InvariantCulture),
                    Name = partner.NomeSocio,
                    Document = partner.CnpjCpfDoSocio,
                    QualificationCode = partner.CodigoQualificacaoSocio.ToString(CultureInfo.InvariantCulture),
                    QualificationDescription = partner.QualificacaoSocio,
                    EntryDate = ParseDateOnly(partner.DataEntradaSociedade),
                    Country = partner.Pais,
                    LegalRepresentativeDocument = partner.RepresentanteLegal,
                    LegalRepresentativeName = partner.NomeRepresentante,
                    LegalRepresentativeQualificationCode = partner.CodigoQualificacaoRepresentanteLegal.ToString(CultureInfo.InvariantCulture),
                    AgeGroup = partner.FaixaEtaria
                }).ToList()
        };

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

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

internal sealed record class BrasilApiCnpjPartnerResponse {

    [property: JsonPropertyName("identificador_de_socio")]
    public int IdentificadorDeSocio { get; init; }

    [property: JsonPropertyName("nome_socio")]
    public string? NomeSocio { get; init; }

    [property: JsonPropertyName("cnpj_cpf_do_socio")]
    public string? CnpjCpfDoSocio { get; init; }

    [property: JsonPropertyName("codigo_qualificacao_socio")]
    public int CodigoQualificacaoSocio { get; init; }

    [property: JsonPropertyName("qualificacao_socio")]
    public string? QualificacaoSocio { get; init; }

    [property: JsonPropertyName("data_entrada_sociedade")]
    public string? DataEntradaSociedade { get; init; }

    [property: JsonPropertyName("pais")]
    public string? Pais { get; init; }

    [property: JsonPropertyName("cpf_representante_legal")]
    public string? RepresentanteLegal { get; init; }

    [property: JsonPropertyName("nome_representante_legal")]
    public string? NomeRepresentante { get; init; }

    [property: JsonPropertyName("codigo_qualificacao_representante_legal")]
    public int CodigoQualificacaoRepresentanteLegal { get; init; }

    [property: JsonPropertyName("faixa_etaria")]
    public string? FaixaEtaria { get; init; }
}
