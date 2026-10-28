using System.Text.Json;

public static class FileHandler
{
    private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

    private static readonly string[] DefaultPrices =
    [
        "# Prislista för Prague Parking",
        "# Skriv fordonstyp och pris i CZK per påbörjad timme.",
        "# Allt efter tecknet # på en rad är kommentarer.",
        "CAR 20",
        "MC 10",
        "",
        "# Så många minuter är parkeringen gratis",
        "FREE_MINUTES 10"
    ];

    public static void Backup(string path)
    {
        if (File.Exists(path))
        {
            File.Copy(path, path + ".bak", true);
        }
    }

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
            Backup(path);
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
            Backup(path);
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
            using var writer = new StreamWriter(path);
            foreach (var defaultLine in DefaultPrices)
            {
                writer.WriteLine(defaultLine);
            }
        }

        var prices = new PriceData();
        using var reader = new StreamReader(path);
        string line;
        while ((line = reader.ReadLine()) != null)
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
