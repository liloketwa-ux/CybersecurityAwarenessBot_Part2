namespace CybersecurityAwarenessBot.Gui.Services;

public static class AsciiArtProvider
{
    public static string Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Assets", "ascii_art.txt");

        try
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
        }
        catch
        {
            // Fallback below.
        }

        return """
╔══════════════════════════════════════════════════════════════════════╗
║   CYBERSECURITY AWARENESS ASSISTANT                                  ║
╚══════════════════════════════════════════════════════════════════════╝
""";
    }
}