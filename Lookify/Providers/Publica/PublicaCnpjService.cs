using System.Text.Json.Serialization;
using Lookify.Cnpj;
using Lookify.Providers.JsonConverters;

namespace Lookify.Providers.Publica;

internal sealed class PublicaCnpjService : ICnpjProviderService {

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
        var fullAddress = $"{BaseAddress}cnpj/{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<PublicaProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}

internal sealed record class PublicaProviderResponse {

    [property: JsonPropertyName("cnpj")]
    public string? Cnpj { get; init; }

    [property: JsonPropertyName("razao_social")]
    public string? RazaoSocial { get; init; }

    [property: JsonPropertyName("capital_social")]
    public string? CapitalSocial { get; init; }

    [property: JsonPropertyName("atualizado_em")]
    public DateTimeOffset? AtualizadoEm { get; init; }

    [property: JsonPropertyName("porte")]
    public PublicaReferenceResponse? Porte { get; init; }

    [property: JsonPropertyName("natureza_juridica")]
    public PublicaReferenceResponse? NaturezaJuridica { get; init; }

    [property: JsonPropertyName("socios")]
    public List<PublicaPartnerResponse> Socios { get; init; } = [];

    [property: JsonPropertyName("simples")]
    public PublicaSimplesResponse? Simples { get; init; }

    [property: JsonPropertyName("estabelecimento")]
    public PublicaEstabelecimentoResponse? Estabelecimento { get; init; }

    public CnpjLookifyResultDto ToResult() =>
        new CnpjLookifyResultDto {
            Cnpj = Cnpj,
            CompanyName = RazaoSocial,
            TradeName = Estabelecimento?.NomeFantasia,
            Phone = BuildPhone(
                Estabelecimento?.Ddd1,
                Estabelecimento?.Telefone1,
                Estabelecimento?.Ddd2,
                Estabelecimento?.Telefone2,
                Estabelecimento?.DddFax,
                Estabelecimento?.Fax),
            Email = Estabelecimento?.Email,
            CompanyType = Estabelecimento?.Tipo,
            RegistrationStatusDescription = Estabelecimento?.SituacaoCadastral,
            RegistrationStatusReasonDescription = Estabelecimento?.MotivoSituacaoCadastral,
            RegistrationStatusDate = Estabelecimento?.DataSituacaoCadastral,
            SpecialSituationDate = Estabelecimento?.DataSituacaoEspecial,
            ActivityStartDate = Estabelecimento?.DataInicioAtividade,
            LegalNatureCode = NaturezaJuridica?.Id,
            LegalNatureDescription = NaturezaJuridica?.Descricao,
            ShareCapital = CapitalSocial,
            CompanySizeCode = Porte?.Id,
            CompanySizeDescription = Porte?.Descricao,
            IsSimpleOptIn = Simples?.Simples,
            SimpleOptInDate = Simples?.DataOpcaoSimples,
            SimpleOptOutDate = Simples?.DataExclusaoSimples,
            IsMeiOptIn = Simples?.Mei,
            PrimaryCnaeCode = Estabelecimento?.AtividadePrincipal?.Id,
            PrimaryCnaeDescription = Estabelecimento?.AtividadePrincipal?.Descricao,
            SecondaryCnaes = Estabelecimento is { } estabelecimento
                ? estabelecimento.AtividadesSecundarias.Select(
                    activity => new CnpjLookifySecondaryCnae {
                        Code = activity.Id,
                        Description = activity.Descricao
                    }).ToList()
                : [],
            StreetTypeDescription = Estabelecimento?.TipoLogradouro,
            Street = Estabelecimento?.Logradouro,
            Number = Estabelecimento?.Numero,
            Complement = Estabelecimento?.Complemento,
            Neighborhood = Estabelecimento?.Bairro,
            ZipCode = Estabelecimento?.Cep,
            State = Estabelecimento?.Estado?.Sigla,
            City = Estabelecimento?.Cidade?.Nome,
            PrimaryPhoneAreaCode = Estabelecimento?.Ddd1,
            SecondaryPhoneAreaCode = Estabelecimento?.Ddd2,
            FaxAreaCode = Estabelecimento?.DddFax,
            SpecialSituation = Estabelecimento?.SituacaoEspecial,
            ForeignCityName = Estabelecimento?.NomeCidadeExterior,
            Country = Estabelecimento?.Pais?.Nome,
            UpdatedAt = AtualizadoEm,
            Partners = Socios.Select(
                partner => new CnpjLookifyPartner {
                    Identifier = partner.CpfCnpjSocio,
                    Name = partner.Nome,
                    Document = partner.CpfCnpjSocio,
                    QualificationCode = partner.QualificacaoSocio?.Id,
                    QualificationDescription = partner.QualificacaoSocio?.Descricao,
                    EntryDate = partner.DataEntrada,
                    Country = partner.PaisId,
                    LegalRepresentativeDocument = partner.CpfRepresentanteLegal,
                    LegalRepresentativeName = partner.NomeRepresentante,
                    LegalRepresentativeQualificationCode = partner.QualificacaoRepresentante?.Id,
                    AgeGroup = partner.FaixaEtaria
                }).ToList()
        };

    private static string? BuildPhone(
        string? ddd1,
        string? telefone1,
        string? ddd2,
        string? telefone2,
        string? dddFax,
        string? fax)
    {
        if (!string.IsNullOrWhiteSpace(ddd1) && !string.IsNullOrWhiteSpace(telefone1))
            return $"({ddd1}) {telefone1}";

        if (!string.IsNullOrWhiteSpace(ddd2) && !string.IsNullOrWhiteSpace(telefone2))
            return $"({ddd2}) {telefone2}";

        if (!string.IsNullOrWhiteSpace(dddFax) && !string.IsNullOrWhiteSpace(fax))
            return $"({dddFax}) {fax}";

        return telefone1 ?? telefone2 ?? fax;
    }
}

internal sealed record class PublicaReferenceResponse {

    [property: JsonPropertyName("id")]
    [property: JsonConverter(typeof(StringOrNumberJsonConverter))]
    public string? Id { get; init; }

    [property: JsonPropertyName("descricao")]
    public string? Descricao { get; init; }
}

internal sealed record class PublicaPartnerResponse {

    [property: JsonPropertyName("cpf_cnpj_socio")]
    public string? CpfCnpjSocio { get; init; }

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }

    [property: JsonPropertyName("data_entrada")]
    public DateOnly? DataEntrada { get; init; }

    [property: JsonPropertyName("cpf_representante_legal")]
    public string? CpfRepresentanteLegal { get; init; }

    [property: JsonPropertyName("nome_representante")]
    public string? NomeRepresentante { get; init; }

    [property: JsonPropertyName("faixa_etaria")]
    public string? FaixaEtaria { get; init; }

    [property: JsonPropertyName("pais_id")]
    [property: JsonConverter(typeof(StringOrNumberJsonConverter))]
    public string? PaisId { get; init; }

    [property: JsonPropertyName("qualificacao_socio")]
    public PublicaReferenceResponse? QualificacaoSocio { get; init; }

    [property: JsonPropertyName("qualificacao_representante")]
    public PublicaReferenceResponse? QualificacaoRepresentante { get; init; }
}

internal sealed record class PublicaSimplesResponse {

    [property: JsonPropertyName("simples")]
    [property: JsonConverter(typeof(FlexibleBooleanJsonConverter))]
    public bool? Simples { get; init; }

    [property: JsonPropertyName("data_opcao_simples")]
    public DateOnly? DataOpcaoSimples { get; init; }

    [property: JsonPropertyName("data_exclusao_simples")]
    public DateOnly? DataExclusaoSimples { get; init; }

    [property: JsonPropertyName("mei")]
    [property: JsonConverter(typeof(FlexibleBooleanJsonConverter))]
    public bool? Mei { get; init; }
}

internal sealed record class PublicaEstabelecimentoResponse {

    [property: JsonPropertyName("tipo")]
    public string? Tipo { get; init; }

    [property: JsonPropertyName("nome_fantasia")]
    public string? NomeFantasia { get; init; }

    [property: JsonPropertyName("situacao_cadastral")]
    public string? SituacaoCadastral { get; init; }

    [property: JsonPropertyName("data_situacao_cadastral")]
    public DateOnly? DataSituacaoCadastral { get; init; }

    [property: JsonPropertyName("data_inicio_atividade")]
    public DateOnly? DataInicioAtividade { get; init; }

    [property: JsonPropertyName("nome_cidade_exterior")]
    public string? NomeCidadeExterior { get; init; }

    [property: JsonPropertyName("pais")]
    public PublicaCountryResponse? Pais { get; init; }

    [property: JsonPropertyName("estado")]
    public PublicaStateResponse? Estado { get; init; }

    [property: JsonPropertyName("cidade")]
    public PublicaCityResponse? Cidade { get; init; }

    [property: JsonPropertyName("cep")]
    public string? Cep { get; init; }

    [property: JsonPropertyName("tipo_logradouro")]
    public string? TipoLogradouro { get; init; }

    [property: JsonPropertyName("logradouro")]
    public string? Logradouro { get; init; }

    [property: JsonPropertyName("numero")]
    public string? Numero { get; init; }

    [property: JsonPropertyName("complemento")]
    public string? Complemento { get; init; }

    [property: JsonPropertyName("bairro")]
    public string? Bairro { get; init; }

    [property: JsonPropertyName("ddd1")]
    public string? Ddd1 { get; init; }

    [property: JsonPropertyName("telefone1")]
    public string? Telefone1 { get; init; }

    [property: JsonPropertyName("ddd2")]
    public string? Ddd2 { get; init; }

    [property: JsonPropertyName("telefone2")]
    public string? Telefone2 { get; init; }

    [property: JsonPropertyName("ddd_fax")]
    public string? DddFax { get; init; }

    [property: JsonPropertyName("fax")]
    public string? Fax { get; init; }

    [property: JsonPropertyName("email")]
    public string? Email { get; init; }

    [property: JsonPropertyName("situacao_especial")]
    public string? SituacaoEspecial { get; init; }

    [property: JsonPropertyName("data_situacao_especial")]
    public DateOnly? DataSituacaoEspecial { get; init; }

    [property: JsonPropertyName("atividade_principal")]
    public PublicaActivityResponse? AtividadePrincipal { get; init; }

    [property: JsonPropertyName("atividades_secundarias")]
    public List<PublicaActivityResponse> AtividadesSecundarias { get; init; } = [];

    [property: JsonPropertyName("motivo_situacao_cadastral")]
    public string? MotivoSituacaoCadastral { get; init; }
}

internal sealed record class PublicaCountryResponse {

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }
}

internal sealed record class PublicaStateResponse {

    [property: JsonPropertyName("sigla")]
    public string? Sigla { get; init; }
}

internal sealed record class PublicaCityResponse {

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }
}

internal sealed record class PublicaActivityResponse {

    [property: JsonPropertyName("id")]
    [property: JsonConverter(typeof(StringOrNumberJsonConverter))]
    public string? Id { get; init; }

    [property: JsonPropertyName("descricao")]
    public string? Descricao { get; init; }
}
