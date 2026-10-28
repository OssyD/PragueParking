class ParkingGarage
{
    private readonly ConfigData config;
    private readonly List<ParkingSpot> spots = new();

    public IReadOnlyList<ParkingSpot> Spots => spots;

    public ParkingGarage(ConfigData config)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (config.SpotCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(config), "Antal platser måste vara minst 1.");
        }

        this.config = config;
        for (var i = 1; i <= config.SpotCount; i++)
        {
            spots.Add(new ParkingSpot(i, config.SpotSize));
        }
    }

    public IEnumerable<string> TypeNames =>
        config.VehicleTypes.Select(t => t.Name).Where(name => name == "CAR" || name == "MC");

    public Vehicle CreateVehicle(string type, string regNumber, DateTime arrival)
    {
        var typeConfig = config.VehicleTypes.FirstOrDefault(t => t.Name == type)
            ?? throw new ArgumentException("Okänd fordonstyp.", nameof(type));
        return type == "MC"
            ? new MC(regNumber, arrival, typeConfig.Size)
            : new Car(regNumber, arrival, typeConfig.Size);
    }

    public int Park(Vehicle vehicle)
    {
        var spot = spots.FirstOrDefault(s => !s.IsEmpty && s.CanFit(vehicle))
            ?? spots.FirstOrDefault(s => s.IsEmpty);
        if (spot == null)
        {
            return -1;
        }
        spot.Add(vehicle);
        return spot.Number;
    }

    public bool Place(Vehicle vehicle, int spotNumber)
    {
        if (spotNumber < 1 || spotNumber > spots.Count || !spots[spotNumber - 1].CanFit(vehicle))
        {
            return false;
        }
        spots[spotNumber - 1].Add(vehicle);
        return true;
    }

    public ParkingSpot Find(string regNumber)
    {
        return spots.FirstOrDefault(s => s.Vehicles.Any(v => v.RegNumber == regNumber));
    }

    public bool Move(string regNumber, int toSpot)
    {
        var from = Find(regNumber);
        if (from == null)
        {
            return false;
        }

        var vehicle = from.Vehicles.First(v => v.RegNumber == regNumber);
        if (from.Number == toSpot || !Place(vehicle, toSpot))
        {
            return false;
        }
        from.Remove(vehicle);
        return true;
    }

    public Vehicle Remove(string regNumber)
    {
        var spot = Find(regNumber);
        if (spot == null)
        {
            return null;
        }

        var vehicle = spot.Vehicles.First(v => v.RegNumber == regNumber);
        spot.Remove(vehicle);
        return vehicle;
    }

    public List<VehicleRecord> ToRecords()
    {
        return spots
            .SelectMany(s => s.Vehicles.Select(v => new VehicleRecord(v.Type, v.RegNumber, s.Number, v.Arrival)))
            .ToList();
    }
}
