using System.Media;

namespace CybersecurityAwarenessBot.Gui.Services;

public static class AudioPlayer
{
    /// <summary>Plays the greeting WAV file if it exists; silently ignores errors.</summary>
    public static void TryPlayGreeting(string path)
    {
        try
        {
            if (!File.Exists(path)) return;
            using var player = new SoundPlayer(path);
            player.Play();
        }
        catch
        {
            // Audio is optional — never crash the app over it.
        }
    }
}
