public class ConfigData
{
    public int SpotCount { get; init; } = 100;
    public int SpotSize { get; init; } = 4;
    public List<VehicleTypeConfig> VehicleTypes { get; init; } =
    [
        new VehicleTypeConfig { Name = "CAR", Size = 4 },
        new VehicleTypeConfig { Name = "MC", Size = 2 }
    ];
}
