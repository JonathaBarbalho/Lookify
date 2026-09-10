using System.Text.Json.Serialization;
using Lookify.Bank;

namespace Lookify.Providers.BrasilApi;

internal static class BrasilApiBankProviderRequest {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public static async Task<List<BankLookifyResultDto>> RequestAllAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/banks/v1";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Bank");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<BrasilApiBankProviderResponse>>(
            providerName: ProviderName,
            identifier: "banks",
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }

    public static async Task<BankLookifyResultDto> RequestByCodeAsync(
        int code,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/banks/v1/{code}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Bank");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<BrasilApiBankProviderResponse>(
            providerName: ProviderName,
            identifier: code.ToString(),
            response,
            cancellationToken);

        return payload.ToResult();
    }
}

internal sealed record class BrasilApiBankProviderResponse {

    [property: JsonPropertyName("ispb")]
    public string? Ispb { get; init; }

    [property: JsonPropertyName("name")]
    public string? Name { get; init; }

    [property: JsonPropertyName("code")]
    public int? Code { get; init; }

    [property: JsonPropertyName("fullName")]
    public string? FullName { get; init; }

    [property: JsonPropertyName("cnpj")]
    public string? Cnpj { get; init; }

    [property: JsonPropertyName("headquarters_address")]
    public BrasilApiBankAddressResponse? HeadquartersAddress { get; init; }

    [property: JsonPropertyName("logo_url")]
    public string? LogoUrl { get; init; }

    public BankLookifyResultDto ToResult() =>
        new BankLookifyResultDto {
            Code = Code,
            Ispb = Ispb,
            Name = Name,
            FullName = FullName,
            Cnpj = Cnpj,
            Street = HeadquartersAddress?.Street,
            Number = HeadquartersAddress?.Number,
            Complement = HeadquartersAddress?.Complement,
            District = HeadquartersAddress?.District,
            City = HeadquartersAddress?.City,
            State = HeadquartersAddress?.State,
            ZipCode = HeadquartersAddress?.ZipCode,
            LogoUrl = LogoUrl
        };
}

internal sealed record class BrasilApiBankAddressResponse {

    [property: JsonPropertyName("street")]
    public string? Street { get; init; }

    [property: JsonPropertyName("number")]
    public string? Number { get; init; }

    [property: JsonPropertyName("complement")]
    public string? Complement { get; init; }

    [property: JsonPropertyName("district")]
    public string? District { get; init; }

    [property: JsonPropertyName("city")]
    public string? City { get; init; }

    [property: JsonPropertyName("state")]
    public string? State { get; init; }

    [property: JsonPropertyName("zipCode")]
    public string? ZipCode { get; init; }
}
