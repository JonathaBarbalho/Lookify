using System.Text.Json.Serialization;
using Lookify.Providers.JsonConverters;
using Lookify.VehiclePlate;

namespace Lookify.Providers.PlacaFipe;

internal sealed record class PlacaFipeProviderResponse {

    [property: JsonPropertyName("codigo")]
    public int Codigo { get; init; }

    [property: JsonPropertyName("msg")]
    public string? Msg { get; init; }

    [property: JsonPropertyName("placa")]
    public string? Placa { get; init; }

    [property: JsonPropertyName("fipe")]
    public List<PlacaFipeMatchResponse> Fipe { get; init; } = [];

    [property: JsonPropertyName("informacoes_veiculo")]
    public PlacaFipeVehicleInfoResponse? InformacoesVeiculo { get; init; }

    public VehiclePlateLookifyResultDto ToResult() =>
        new VehiclePlateLookifyResultDto {
            Plate = Placa,
            Brand = InformacoesVeiculo?.Marca,
            Model = InformacoesVeiculo?.Modelo,
            ManufactureYear = InformacoesVeiculo?.Ano,
            ModelYear = InformacoesVeiculo?.AnoModelo,
            Color = InformacoesVeiculo?.Cor,
            Chassis = InformacoesVeiculo?.Chassi,
            Engine = InformacoesVeiculo?.Motor,
            City = InformacoesVeiculo?.Municipio,
            State = InformacoesVeiculo?.Uf,
            Segment = InformacoesVeiculo?.Segmento,
            SubSegment = InformacoesVeiculo?.SubSegmento,
            Displacement = InformacoesVeiculo?.Cilindradas,
            Fuel = InformacoesVeiculo?.Combustivel,
            FipeMatches = Fipe.Select(
                match => new VehiclePlateLookifyFipeMatch {
                    Similarity = match.Similaridade,
                    Correspondence = match.Correspondencia,
                    Brand = match.Marca,
                    Model = match.Modelo,
                    ModelYear = match.AnoModelo,
                    FipeCode = match.CodigoFipe,
                    BrandCode = match.CodigoMarca,
                    ModelCode = match.CodigoModelo,
                    ReferenceMonth = match.MesReferencia,
                    Fuel = match.Combustivel,
                    Value = match.Valor,
                    ValueUnit = match.UnidadeValor
                }).ToList()
        };
}

internal sealed record class PlacaFipeMatchResponse {

    [property: JsonPropertyName("similaridade")]
    [property: JsonConverter(typeof(FlexibleDecimalJsonConverter))]
    public decimal? Similaridade { get; init; }

    [property: JsonPropertyName("correspondencia")]
    [property: JsonConverter(typeof(FlexibleDecimalJsonConverter))]
    public decimal? Correspondencia { get; init; }

    [property: JsonPropertyName("marca")]
    public string? Marca { get; init; }

    [property: JsonPropertyName("modelo")]
    public string? Modelo { get; init; }

    [property: JsonPropertyName("ano_modelo")]
    [property: JsonConverter(typeof(StringOrNumberJsonConverter))]
    public string? AnoModelo { get; init; }

    [property: JsonPropertyName("codigo_fipe")]
    public string? CodigoFipe { get; init; }

    [property: JsonPropertyName("codigo_marca")]
    public string? CodigoMarca { get; init; }

    [property: JsonPropertyName("codigo_modelo")]
    public string? CodigoModelo { get; init; }

    [property: JsonPropertyName("mes_referencia")]
    public string? MesReferencia { get; init; }

    [property: JsonPropertyName("combustivel")]
    public string? Combustivel { get; init; }

    [property: JsonPropertyName("valor")]
    public string? Valor { get; init; }

    [property: JsonPropertyName("unidade_valor")]
    public string? UnidadeValor { get; init; }
}

internal sealed record class PlacaFipeVehicleInfoResponse {

    [property: JsonPropertyName("marca")]
    public string? Marca { get; init; }

    [property: JsonPropertyName("modelo")]
    public string? Modelo { get; init; }

    [property: JsonPropertyName("ano")]
    [property: JsonConverter(typeof(FlexibleInt32JsonConverter))]
    public int? Ano { get; init; }

    [property: JsonPropertyName("ano_modelo")]
    [property: JsonConverter(typeof(FlexibleInt32JsonConverter))]
    public int? AnoModelo { get; init; }

    [property: JsonPropertyName("cor")]
    public string? Cor { get; init; }

    [property: JsonPropertyName("chassi")]
    public string? Chassi { get; init; }

    [property: JsonPropertyName("motor")]
    public string? Motor { get; init; }

    [property: JsonPropertyName("municipio")]
    public string? Municipio { get; init; }

    [property: JsonPropertyName("uf")]
    public string? Uf { get; init; }

    [property: JsonPropertyName("segmento")]
    public string? Segmento { get; init; }

    [property: JsonPropertyName("sub_segmento")]
    public string? SubSegmento { get; init; }

    [property: JsonPropertyName("cilindradas")]
    public string? Cilindradas { get; init; }

    [property: JsonPropertyName("combustivel")]
    public string? Combustivel { get; init; }
}
