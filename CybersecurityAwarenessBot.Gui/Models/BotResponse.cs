using System.Drawing;

namespace CybersecurityAwarenessBot.Gui.Models;

public sealed record BotResponse(
    string Title,
    string Message,
    Color AccentColor,
    string? Topic = null,
    SentimentLevel Sentiment = SentimentLevel.Neutral,
    bool ShowFollowUpHint = false
);

public delegate BotResponse ResponseResolver(string input);
