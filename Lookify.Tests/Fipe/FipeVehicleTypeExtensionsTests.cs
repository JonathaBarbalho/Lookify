using Lookify.Fipe;

namespace Lookify.Tests.Fipe;

public class FipeVehicleTypeExtensionsTests {

    [Theory]
    [InlineData(FipeVehicleType.Cars, "carros")]
    [InlineData(FipeVehicleType.Motorcycles, "motos")]
    [InlineData(FipeVehicleType.Trucks, "caminhoes")]
    public void ToPathSegment_QuandoTipoValido_RetornaSegmentoEmPortugues(
        FipeVehicleType vehicleType,
        string esperado)
    {
        Assert.Equal(esperado, vehicleType.ToPathSegment());
    }

    [Fact]
    public void ToPathSegment_QuandoTipoInvalido_LancaArgumentOutOfRangeException()
    {
        var vehicleType = (FipeVehicleType)99;

        Assert.Throws<ArgumentOutOfRangeException>(() => vehicleType.ToPathSegment());
    }

    [Theory]
    [InlineData(FipeVehicleType.Cars, "cars")]
    [InlineData(FipeVehicleType.Motorcycles, "motorcycles")]
    [InlineData(FipeVehicleType.Trucks, "trucks")]
    public void ToParallelumPathSegment_QuandoTipoValido_RetornaSegmentoEmIngles(
        FipeVehicleType vehicleType,
        string esperado)
    {
        Assert.Equal(esperado, vehicleType.ToParallelumPathSegment());
    }

    [Fact]
    public void ToParallelumPathSegment_QuandoTipoInvalido_LancaArgumentOutOfRangeException()
    {
        var vehicleType = (FipeVehicleType)99;

        Assert.Throws<ArgumentOutOfRangeException>(() => vehicleType.ToParallelumPathSegment());
    }
}
