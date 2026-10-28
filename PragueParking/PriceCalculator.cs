static class PriceCalculator
{
    public static int Calculate(Vehicle vehicle, DateTime now, PriceData prices)
    {
        var minutes = (now - vehicle.Arrival).TotalMinutes;
        if (minutes < prices.FreeMinutes)
        {
            return 0;
        }
        var hours = (int)Math.Ceiling(minutes / 60);
        return hours * prices.HourlyPrices.GetValueOrDefault(vehicle.Type);
    }
}
