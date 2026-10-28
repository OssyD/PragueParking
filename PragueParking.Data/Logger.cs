public static class Logger
{
    private static string path = Path.Combine(AppContext.BaseDirectory, "log.txt");

    public static void UseFile(string filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            path = filePath;
        }
    }

    public static void Log(string message)
    {
        try
        {
            File.AppendAllText(path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }
}
