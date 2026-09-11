using System.Text.RegularExpressions;
using Lookify.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify.VehiclePlate;

internal sealed class VehiclePlateLookifyService(
    IHttpClientFactory httpFactory,
    IOptions<LookifyOptions> options,
    ILogger logger) : IVehiclePlateLookifyService {

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LookifyOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    private static readonly Regex PlateFormatRegex = new(
        "^[A-Z]{3}\\d[A-Z\\d]\\d{2}$",
        RegexOptions.Compiled);

    public async Task<VehiclePlateLookifyResultDto> ConsultAsync(
        string plate,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(plate)) {
            throw new ArgumentException("Plate cannot be null or empty.", nameof(plate));
        }

        var sanitizedPlate = SanitizePlate(plate);

        return await ProviderFallback.ExecuteAsync(
            GetEnabledProviders(),
            $"a placa {sanitizedPlate}",
            _logger,
            provider => GetService(provider).RequestAsync(
                sanitizedPlate,
                _options.GetProviderOptions(provider).Token,
                _httpFactory,
                cancellationToken));
    }

    private static string SanitizePlate(string plate)
    {
        var sanitizedPlate = new string(plate.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        if (!PlateFormatRegex.IsMatch(sanitizedPlate))
            throw new ArgumentException(
                "Plate must match the Brazilian format (old LLL9999 or Mercosul LLL9L99).",
                nameof(plate));

        return sanitizedPlate;
    }

    private List<VehiclePlateLookifyProviderEnum> GetEnabledProviders() =>
        _options.VehiclePlateProviders
            .Where(provider => _options.GetProviderOptions(provider).IsEnabled)
            .ToList();

    private static IProvider GetProvider(VehiclePlateLookifyProviderEnum provider) =>
        provider switch {
            VehiclePlateLookifyProviderEnum.PlacaFipe => ProviderRegistry.PlacaFipe,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };

    private static IVehiclePlateProviderService GetService(VehiclePlateLookifyProviderEnum provider) =>
        GetProvider(provider).Services
            .OfType<IVehiclePlateProviderService>()
            .First();
}
