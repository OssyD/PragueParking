namespace PragueParking.Tests;

[TestClass]
public sealed class ParkingTests
{
    [TestMethod]
    public void TwoMcShareSpot_CarDoesNotFit()
    {
        var garage = new ParkingGarage(new ConfigData());
        var now = DateTime.Now;

        var first = garage.Park(garage.CreateVehicle("MC", "MC1", now));
        var second = garage.Park(garage.CreateVehicle("MC", "MC2", now));
        var car = garage.Park(garage.CreateVehicle("CAR", "CAR1", now));

        Assert.AreEqual(1, first);
        Assert.AreEqual(1, second);
        Assert.AreEqual(2, car);
    }

    [TestMethod]
    public void Price_FreeUnderTenMinutes_ThenPerStartedHour()
    {
        var prices = new PriceData();
        prices.HourlyPrices["CAR"] = 20;
        var now = DateTime.Now;

        Assert.AreEqual(0, PriceCalculator.Calculate(new Car("ABC123", now.AddMinutes(-9), 4), now, prices));
        Assert.AreEqual(20, PriceCalculator.Calculate(new Car("ABC123", now.AddMinutes(-11), 4), now, prices));
        Assert.AreEqual(40, PriceCalculator.Calculate(new Car("ABC123", now.AddMinutes(-61), 4), now, prices));
    }
}
