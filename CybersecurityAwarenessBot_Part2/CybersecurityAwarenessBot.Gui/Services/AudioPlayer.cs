using System.Media;

namespace CybersecurityAwarenessBot.Gui.Services;

public static class AudioPlayer
{
    public static void TryPlayGreeting(string wavPath)
    {
        try
        {
            if (!File.Exists(wavPath))
            {
                return;
            }

            using var player = new SoundPlayer(wavPath);
            player.PlaySync();
        }
        catch
        {
            // Audio should never block the GUI from loading.
        }
    }
}