using System.Drawing;

namespace CybersecurityAwarenessBot.Gui.Models;

public sealed record ChatMessage(
    string Speaker,
    string Text,
    DateTimeOffset Timestamp,
    Color Colour
);