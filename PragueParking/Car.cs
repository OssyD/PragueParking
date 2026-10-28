class Car(string regNumber, DateTime arrival, int size) : Vehicle(regNumber, arrival, size)
{
    public override string Type => "CAR";
}
