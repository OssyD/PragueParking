using System.Security.Cryptography;
using System.Text;

public record Receipt(string RegNumber, string Type, int Spot, DateTime Arrival, DateTime Departure, int Price, string Hash = "")
{
    public static Receipt Create(string regNumber, string type, int spot, DateTime arrival, DateTime departure, int price)
    {
        var receipt = new Receipt(regNumber, type, spot, arrival, departure, price);
        return receipt with { Hash = receipt.ComputeHash() };
    }

    public bool IsValid()
    {
        return Hash == ComputeHash();
    }

    private string ComputeHash()
    {
        var content = $"{RegNumber}|{Type}|{Spot}|{Arrival:yyyy-MM-dd HH:mm:ss}|{Departure:yyyy-MM-dd HH:mm:ss}|{Price}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
    }
}
