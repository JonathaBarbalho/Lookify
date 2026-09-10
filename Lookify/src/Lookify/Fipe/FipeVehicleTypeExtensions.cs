namespace Lookify.Fipe;

internal static class FipeVehicleTypeExtensions {

    public static string ToPathSegment(
        this FipeVehicleType vehicleType) =>
        vehicleType switch {
            FipeVehicleType.Cars => "carros",
            FipeVehicleType.Motorcycles => "motos",
            FipeVehicleType.Trucks => "caminhoes",
            _ => throw new ArgumentOutOfRangeException(nameof(vehicleType), vehicleType, null)
        };

    public static string ToParallelumPathSegment(
        this FipeVehicleType vehicleType) =>
        vehicleType switch {
            FipeVehicleType.Cars => "cars",
            FipeVehicleType.Motorcycles => "motorcycles",
            FipeVehicleType.Trucks => "trucks",
            _ => throw new ArgumentOutOfRangeException(nameof(vehicleType), vehicleType, null)
        };
}
