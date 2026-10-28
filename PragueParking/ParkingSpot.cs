class ParkingSpot
{
    private readonly List<Vehicle> vehicles = new();

    public int Number { get; }
    public int Size { get; }
    public IReadOnlyList<Vehicle> Vehicles => vehicles;

    public int UsedSize => vehicles.Sum(v => v.Size);
    public bool IsEmpty => vehicles.Count == 0;
    public bool IsFull => UsedSize >= Size;

    public ParkingSpot(int number, int size)
    {
        if (number < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Platsnummer måste vara minst 1.");
        }
        if (size < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Platsens storlek måste vara minst 1.");
        }
        Number = number;
        Size = size;
    }

    public bool CanFit(Vehicle vehicle)
    {
        return UsedSize + vehicle.Size <= Size;
    }

    public void Add(Vehicle vehicle)
    {
        if (!CanFit(vehicle))
        {
            throw new InvalidOperationException("Fordonet får inte plats.");
        }
        vehicles.Add(vehicle);
    }

    public void Remove(Vehicle vehicle)
    {
        vehicles.Remove(vehicle);
    }
}
