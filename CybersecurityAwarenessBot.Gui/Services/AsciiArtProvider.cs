namespace CybersecurityAwarenessBot.Gui.Services;

public static class AsciiArtProvider
{
    public static string Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Assets", "ascii_art.txt");

        try
        {
            if (File.Exists(path))
                return File.ReadAllText(path);
        }
        catch
        {
            // Fall through to built-in banner.
        }

        return """
╔══════════════════════════════════════════════════════════════════════╗
║   CYBERSECURITY AWARENESS ASSISTANT  ·  PART 3                       ║
╚══════════════════════════════════════════════════════════════════════╝
""";
    }
}
