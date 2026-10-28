# Prague Parking 2.0

Objektorienterat konsolprogram för en valet parking, med Spectre.Console som gränssnitt.

## Projekt

- `PragueParking` - konsolprogrammet
- `PragueParking.Data` - klassbibliotek för dataåtkomst (läsa och spara filer)
- `PragueParking.Tests` - enhetstester med MSTest

Klasser i `PragueParking`: `Vehicle` (med `Car` och `MC`), `ParkingSpot`, `ParkingGarage`, `PriceCalculator` och `App`.

På skärmen visas en karta över parkeringen. Grön ruta är ledig, gul är delvis full (till exempel en MC) och röd är full.

## Storlekar

Varje P-plats och fordonstyp har en storlek. Ett fordon får plats om summan av storlekarna på platsen inte blir större än platsens storlek. Standard är P-plats 4, bil 4 och MC 2, alltså en bil eller två MC per plats. Storlekarna ändras i `config.json`.

Om ett fordon i `parking.json` inte får plats vid start (till exempel för att antalet platser har minskats) visas en varning och datafilen kopieras till `parking.json.bak`, så att inget fordon försvinner.

## Filer

Filerna ligger i projektmappen `PragueParking`. Programmet letar uppåt från sin körbara fil tills det hittar mappen med `.csproj`. Körs programmet utanför projektet används den egna mappen i stället.

| Fil | Innehåll |
|---|---|
| `config.json` | Antal P-platser (`SpotCount`), P-platsens storlek (`SpotSize`) och fordonstyper med storlek (`VehicleTypes`) |
| `prices.txt` | Prislistan. Allt efter `#` på en rad är kommentarer |
| `parking.json` | Parkerade fordon. Sparas efter varje parkering, flytt och uthämtning |

Prislistan kan läsas in på nytt från menyn utan att starta om programmet. Parkeringen är gratis om fordonet står mindre än `FREE_MINUTES` minuter, och därefter debiteras varje påbörjad timme.

Om en fil saknas skapas en standardfil (utan fordon) när programmet startar.

## Köra programmet

Kräver .NET 10 SDK.

Öppna `PragueParking.slnx` i Visual Studio, välj `PragueParking` som startprojekt och tryck F5 eller Ctrl+F5.

Eller från en terminal:

```
dotnet run --project PragueParking
```

Navigera i menyn med piltangenterna och Enter. Kör programmet i ett vanligt konsolfönster för att färger och meny ska visas rätt.

## Tester

Kör testerna i Visual Studio via Test Explorer, eller från en terminal:

```
dotnet test
```
