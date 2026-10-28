try
{
    new App().Run();
}
catch (Exception ex)
{
    Logger.Log(ex.ToString());
    Console.WriteLine("Ett oväntat fel uppstod. Programmet avslutas.");
}
