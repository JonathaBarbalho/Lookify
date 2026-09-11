using Lookify.Ibge;

namespace Lookify.Providers.Ibge;

internal sealed partial class IbgeService {

    public async Task<List<IbgeRegionLookifyResultDto>> GetRegionsAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/v1/localidades/regioes";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
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
