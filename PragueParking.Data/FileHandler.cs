using System.Text.Json;

public static class FileHandler
{
    private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

    private const string DefaultPrices =
        "# Prislista för Prague Parking\n" +
        "# Skriv fordonstyp och pris i CZK per påbörjad timme.\n" +
        "# Allt efter tecknet # på en rad är kommentarer.\n" +
        "CAR 20\n" +
        "MC 10\n" +
        "\n" +
        "# Så många minuter är parkeringen gratis\n" +
        "FREE_MINUTES 10\n";

    public static ConfigData ReadConfig(string path)
    {
        if (!File.Exists(path))
        {
            SaveConfig(path, new ConfigData());
        }

        try
        {
            var config = JsonSerializer.Deserialize<ConfigData>(File.ReadAllText(path));
            if (IsValid(config))
            {
                return config;
            }
            Logger.Log("Ogiltiga värden i konfigurationen.");
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            Logger.Log($"Kunde inte läsa konfigurationen: {ex.Message}");
        }
        return new ConfigData();
    }

    public static void SaveConfig(string path, ConfigData config)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(config, options));
    }

    public static List<VehicleRecord> ReadVehicles(string path)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<VehicleRecord>>(File.ReadAllText(path)) ?? [];
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            Logger.Log($"Kunde inte läsa datafilen: {ex.Message}");
            File.Copy(path, path + ".bak", true);
            return [];
        }
    }

    public static void SaveVehicles(string path, List<VehicleRecord> vehicles)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(vehicles, options));
    }

    public static List<Receipt> ReadReceipts(string path)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<Receipt>>(File.ReadAllText(path)) ?? [];
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            Logger.Log($"Kunde inte läsa kvittofilen: {ex.Message}");
            File.Copy(path, path + ".bak", true);
            return [];
        }
    }

    public static void AddReceipt(string path, Receipt receipt)
    {
        var receipts = ReadReceipts(path);
        receipts.Add(receipt);
        File.WriteAllText(path, JsonSerializer.Serialize(receipts, options));
    }

    public static PriceData ReadPrices(string path)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, DefaultPrices);
        }

        var prices = new PriceData();
        foreach (var line in File.ReadAllLines(path))
        {
            var text = line.Split('#')[0];
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                continue;
            }
            if (parts.Length != 2 || !int.TryParse(parts[1], out var value) || value < 0)
            {
                Logger.Log($"Ogiltig rad i prislistan: {line}");
                continue;
            }

            if (parts[0].ToUpper() == "FREE_MINUTES")
            {
                prices.FreeMinutes = value;
            }
            else
            {
                prices.HourlyPrices[parts[0].ToUpper()] = value;
            }
        }
        return prices;
    }

    private static bool IsValid(ConfigData config)
    {
        return config != null
            && config.SpotCount >= 1
            && config.SpotSize >= 1
            && config.VehicleTypes != null
            && config.VehicleTypes.Count > 0
            && config.VehicleTypes.All(t => !string.IsNullOrWhiteSpace(t.Name) && t.Size >= 1 && t.Size <= config.SpotSize);
    }
}
