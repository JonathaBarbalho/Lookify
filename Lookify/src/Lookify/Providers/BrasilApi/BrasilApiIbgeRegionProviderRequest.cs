using Lookify.Ibge;
using Lookify.Providers.Ibge;

namespace Lookify.Providers.BrasilApi;

internal static class BrasilApiIbgeRegionProviderRequest {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public static async Task<List<IbgeRegionLookifyResultDto>> RequestAllAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/ibge/regioes/v1";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Ibge");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<IbgeRegiaoResponse>>(
            providerName: ProviderName,
            identifier: "regioes",
            response,
            cancellationToken);

        return payload.Select(
            item => new IbgeRegionLookifyResultDto {
                Id = item.Id,
                Name = item.Nome,
                Acronym = item.Sigla
            }).ToList();
    }
}
