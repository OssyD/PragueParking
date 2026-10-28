abstract class Vehicle
{
    public string RegNumber { get; }
    public DateTime Arrival { get; }
    public int Size { get; }
    public abstract string Type { get; }

    protected Vehicle(string regNumber, DateTime arrival, int size)
    {
        if (string.IsNullOrWhiteSpace(regNumber) || regNumber.Length > 10 || !regNumber.All(char.IsLetterOrDigit))
        {
            throw new ArgumentException("Ogiltigt registreringsnummer.", nameof(regNumber));
        }
        if (size < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Storleken måste vara minst 1.");
        }

        RegNumber = regNumber;
        Arrival = arrival;
        Size = size;
    }
}

class Car(string regNumber, DateTime arrival, int size) : Vehicle(regNumber, arrival, size)
{
    public override string Type => "CAR";
}

class MC(string regNumber, DateTime arrival, int size) : Vehicle(regNumber, arrival, size)
{
    public override string Type => "MC";
}
