using Spectre.Console;
using Spectre.Console.Rendering;

class App
{
    private readonly string configPath;
    private readonly string dataPath;
    private readonly string pricePath;
    private readonly string receiptPath;

    private readonly ParkingGarage garage;
    private PriceData prices;
    private string startupWarning;

    private static string FindProjectFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && dir.GetFiles("*.csproj").Length == 0)
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? AppContext.BaseDirectory;
    }

    public App()
    {
        var folder = FindProjectFolder();
        Logger.UseFile(Path.Combine(folder, "log.txt"));
        configPath = Path.Combine(folder, "config.json");
        dataPath = Path.Combine(folder, "parking.json");
        pricePath = Path.Combine(folder, "prices.txt");
        receiptPath = Path.Combine(folder, "receipts.json");

        var config = FileHandler.ReadConfig(configPath);
        garage = new ParkingGarage(config);
        prices = FileHandler.ReadPrices(pricePath);

        var skipped = 0;
        foreach (var record in FileHandler.ReadVehicles(dataPath))
        {
            try
            {
                var vehicle = garage.CreateVehicle(record.Type, record.RegNumber, record.Arrival);
                if (garage.Find(vehicle.RegNumber) != null || !garage.Place(vehicle, record.Spot))
                {
                    Logger.Log($"Fordon fick inte plats vid inläsning: {record}");
                    skipped++;
                }
            }
            catch (ArgumentException ex)
            {
                Logger.Log($"Ogiltigt fordon i datafilen: {record} ({ex.Message})");
                skipped++;
            }
        }

        if (skipped > 0)
        {
            FileHandler.Backup(dataPath);
            startupWarning = $"{skipped} fordon i datafilen kunde inte läsas in. En kopia sparades som parking.json.bak.";
        }
    }

    public void Run()
    {
        var running = true;
        while (running)
        {
            AnsiConsole.Clear();
            ShowMap();
            if (startupWarning != null)
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(startupWarning)}[/]");
            }
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[bold]Välj i menyn[/]")
                    .AddChoices("Parkera fordon", "Flytta fordon", "Hämta ut fordon", "Sök fordon",
                        "Visa fordonslista", "Kontrollera kvitton", "Visa prislista", "Läs in prislistan på nytt", "Avsluta"));

            AnsiConsole.Clear();
            switch (choice)
            {
                case "Parkera fordon": ParkVehicle(); break;
                case "Flytta fordon": MoveVehicle(); break;
                case "Hämta ut fordon": RemoveVehicle(); break;
                case "Sök fordon": SearchVehicle(); break;
                case "Visa fordonslista": ShowVehicles(); break;
                case "Kontrollera kvitton": CheckReceipts(); break;
                case "Visa prislista": ShowPrices(); break;
                case "Läs in prislistan på nytt": ReloadPrices(); break;
                case "Avsluta": running = false; break;
            }
            if (running)
            {
                AnsiConsole.MarkupLine("\n[grey]Tryck på valfri tangent för att fortsätta...[/]");
                Console.ReadKey(true);
            }
        }
    }

    private static string TypeLabel(string type)
    {
        return type == "CAR" ? "Bil" : type;
    }

    private static string AskReg()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Registreringsnummer:")
                .Validate(reg => reg.Length >= 1 && reg.Length <= 10 && reg.All(char.IsLetterOrDigit)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Använd 1-10 bokstäver eller siffror, utan mellanslag eller tecken.[/]")))
            .ToUpper();
    }

    private int AskSpot()
    {
        var count = garage.Spots.Count;
        return AnsiConsole.Prompt(
            new TextPrompt<int>($"Plats (1-{count}):")
                .ValidationErrorMessage("[red]Ange ett tal.[/]")
                .Validate(spot => spot >= 1 && spot <= count
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"[red]Ange ett tal mellan 1 och {count}.[/]")));
    }

    private void Save()
    {
        try
        {
            FileHandler.SaveVehicles(dataPath, garage.ToRecords());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Logger.Log($"Kunde inte spara: {ex.Message}");
            AnsiConsole.MarkupLine("[red]Ändringen kunde inte sparas till fil. Kontakta ansvarig.[/]");
        }
    }

    private void ShowMap()
    {
        var grid = new Grid();
        for (var i = 0; i < 10; i++)
        {
            grid.AddColumn(new GridColumn().NoWrap());
        }

        for (var start = 0; start < garage.Spots.Count; start += 10)
        {
            var cells = garage.Spots
                .Skip(start)
                .Take(10)
                .Select(s => (IRenderable)new Markup($"[black on {SpotColor(s)}] {s.Number,3} [/]"))
                .ToArray();
            grid.AddRow(cells);
        }

        var empty = garage.Spots.Count(s => s.IsEmpty);
        var full = garage.Spots.Count(s => s.IsFull);
        var half = garage.Spots.Count - empty - full;

        AnsiConsole.Write(new Rule("[bold orange1]PRAGUE PARKING 2.0[/]"));
        AnsiConsole.Write(grid);
        AnsiConsole.MarkupLine($"\n[black on green]   [/] Lediga: {empty}   [black on yellow]   [/] Delvis fulla: {half}   [black on red]   [/] Fulla: {full}");
    }

    private static string SpotColor(ParkingSpot spot)
    {
        if (spot.IsEmpty)
        {
            return "green";
        }
        return spot.IsFull ? "red" : "yellow";
    }

    private void ParkVehicle()
    {
        var type = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Fordonstyp")
                .AddChoices(garage.TypeNames)
                .UseConverter(TypeLabel));
        var reg = AskReg();
        if (garage.Find(reg) != null)
        {
            AnsiConsole.MarkupLine("[red]Fordonet är redan parkerat.[/]");
            return;
        }

        var spot = garage.Park(garage.CreateVehicle(type, reg, DateTime.Now));
        if (spot == -1)
        {
            AnsiConsole.MarkupLine("[red]Ingen ledig plats.[/]");
            return;
        }

        Save();
        AnsiConsole.MarkupLine($"Kör {TypeLabel(type)} {reg} till plats [green]{spot}[/].");
    }

    private void MoveVehicle()
    {
        var reg = AskReg();
        var from = garage.Find(reg);
        if (from == null)
        {
            AnsiConsole.MarkupLine("[red]Fordonet hittades inte.[/]");
            return;
        }

        var to = AskSpot();
        if (garage.Move(reg, to))
        {
            Save();
            AnsiConsole.MarkupLine($"Flyttade {reg} från plats {from.Number} till plats [green]{to}[/].");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Fordonet får inte plats där.[/]");
        }
    }

    private void RemoveVehicle()
    {
        var reg = AskReg();
        var spot = garage.Find(reg);
        if (spot == null)
        {
            AnsiConsole.MarkupLine("[red]Fordonet hittades inte.[/]");
            return;
        }

        var vehicle = garage.Remove(reg);
        Save();

        var now = DateTime.Now;
        var time = now - vehicle.Arrival;
        var price = PriceCalculator.Calculate(vehicle, now, prices);
        AnsiConsole.MarkupLine($"Fordon {reg} hämtades ut från plats {spot.Number}.");
        AnsiConsole.MarkupLine($"Parkerad i {time.Days} dagar, {time.Hours} timmar och {time.Minutes} minuter.");
        AnsiConsole.MarkupLine($"Att betala: [bold green]{price} CZK[/]");

        try
        {
            FileHandler.AddReceipt(receiptPath, Receipt.Create(reg, vehicle.Type, spot.Number, vehicle.Arrival, now, price));
            AnsiConsole.MarkupLine("[grey]Kvitto sparat.[/]");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Logger.Log($"Kunde inte spara kvitto: {ex.Message}");
            AnsiConsole.MarkupLine("[red]Kvittot kunde inte sparas. Kontakta ansvarig.[/]");
        }
    }

    private void CheckReceipts()
    {
        var receipts = FileHandler.ReadReceipts(receiptPath);
        if (receipts.Count == 0)
        {
            AnsiConsole.MarkupLine("Inga kvitton finns.");
            return;
        }

        var table = new Table().RoundedBorder();
        table.AddColumn("Regnr");
        table.AddColumn("Hämtad");
        table.AddColumn("Belopp");
        table.AddColumn("Status");

        foreach (var receipt in receipts)
        {
            var status = receipt.IsValid() ? "[green]OK[/]" : "[red]Manipulerat![/]";
            table.AddRow(Markup.Escape(receipt.RegNumber ?? "-"), receipt.Departure.ToString("yyyy-MM-dd HH:mm"), $"{receipt.Price} CZK", status);
        }
        AnsiConsole.Write(table);

        var invalid = receipts.Count(r => !r.IsValid());
        if (invalid > 0)
        {
            Logger.Log($"{invalid} kvitton har manipulerats.");
        }
    }

    private void SearchVehicle()
    {
        var reg = AskReg();
        var spot = garage.Find(reg);
        if (spot == null)
        {
            AnsiConsole.MarkupLine("[red]Fordonet hittades inte.[/]");
            return;
        }

        var vehicle = spot.Vehicles.First(v => v.RegNumber == reg);
        AnsiConsole.MarkupLine($"{reg} står på plats [green]{spot.Number}[/], anlände {vehicle.Arrival:yyyy-MM-dd HH:mm}.");
    }

    private void ShowVehicles()
    {
        var table = new Table().RoundedBorder();
        table.AddColumn("Plats");
        table.AddColumn("Typ");
        table.AddColumn("Regnr");
        table.AddColumn("Ankomst");

        foreach (var spot in garage.Spots)
        {
            foreach (var vehicle in spot.Vehicles)
            {
                table.AddRow(spot.Number.ToString(), TypeLabel(vehicle.Type), vehicle.RegNumber, vehicle.Arrival.ToString("yyyy-MM-dd HH:mm"));
            }
        }
        AnsiConsole.Write(table);
    }

    private void ShowPrices()
    {
        var table = new Table().RoundedBorder();
        table.AddColumn("Fordonstyp");
        table.AddColumn("Pris per påbörjad timme");

        foreach (var price in prices.HourlyPrices)
        {
            table.AddRow(Markup.Escape(TypeLabel(price.Key)), $"{price.Value} CZK");
        }
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"De första {prices.FreeMinutes} minuterna är gratis.");
    }

    private void ReloadPrices()
    {
        try
        {
            prices = FileHandler.ReadPrices(pricePath);
            AnsiConsole.MarkupLine("[green]Prislistan lästes in på nytt.[/]");
        }
        catch (IOException ex)
        {
            Logger.Log($"Kunde inte läsa prislistan: {ex.Message}");
            AnsiConsole.MarkupLine("[red]Prislistan kunde inte läsas. Den gamla prislistan används.[/]");
        }
    }
}
