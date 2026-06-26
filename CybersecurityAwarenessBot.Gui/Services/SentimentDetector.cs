using CybersecurityAwarenessBot.Gui.Models;

namespace CybersecurityAwarenessBot.Gui.Services;

public static class SentimentDetector
{
    public static SentimentLevel Detect(string input)
    {
        string lowered = input.ToLowerInvariant();

        if (ContainsAny(lowered, "worried", "scared", "afraid", "anxious", "concerned", "nervous", "unsafe"))
            return SentimentLevel.Worried;

        if (ContainsAny(lowered, "frustrated", "annoyed", "angry", "stuck", "overwhelmed", "fed up"))
            return SentimentLevel.Frustrated;

        if (ContainsAny(lowered, "curious", "wondering", "interested", "learn", "want to know"))
            return SentimentLevel.Curious;

        if (ContainsAny(lowered, "good", "great", "thanks", "thank you", "perfect", "awesome"))
            return SentimentLevel.Positive;

        if (ContainsAny(lowered, "confused", "unclear", "not sure", "explain"))
            return SentimentLevel.Confused;

        return SentimentLevel.Neutral;
    }

    private static bool ContainsAny(string input, params string[] terms)
        => terms.Any(input.Contains);
}
