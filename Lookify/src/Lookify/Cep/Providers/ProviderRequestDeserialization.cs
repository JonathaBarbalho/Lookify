using System.Reflection.Emit;
using System.Text.Json;

namespace Lookify.Cep.Providers;

internal static class ProviderRequestDeserialization {

    public static async Task<TResponse> ReadAndDeserializeAsync<TResponse>(
        string providerName,
        string zipCode,
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        if (providerName is null)
            throw new ArgumentNullException(nameof(providerName));

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode) {
            throw new HttpRequestException(
                $"{providerName} retornou {(int)response.StatusCode} ({response.ReasonPhrase}) para o CEP {zipCode}. Corpo: {TruncateForLog(content)}",
                null,
                response.StatusCode);
        }

        try {
            return JsonSerializer.Deserialize<TResponse>(content)
                ?? throw new InvalidOperationException(
                    $"Falha ao desserializar a resposta do CEP em {providerName}.");
        }
        catch (JsonException exception) {
            throw new InvalidOperationException(
                $"{providerName} não conseguiu desserializar o CEP {zipCode}. Corpo: {TruncateForLog(content)}",
                exception);
        }
    }

    private static string TruncateForLog(
        string value,
        int maxLength = 2048)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength) {
            return value;
        }

        return value[..maxLength] + "...";
    }
}
