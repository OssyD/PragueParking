public class PriceData
{
    public Dictionary<string, int> HourlyPrices { get; } = new();
    public int FreeMinutes { get; set; } = 10;
}
