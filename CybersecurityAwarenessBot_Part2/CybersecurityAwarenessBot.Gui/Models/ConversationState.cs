namespace CybersecurityAwarenessBot.Gui.Models;

public sealed class ConversationState
{
    public string? UserName { get; set; }
    public string? FavouriteTopic { get; set; }
    public string? LastTopic { get; set; }
    public string? LastUserMessage { get; set; }
    public DateTimeOffset StartedAt { get; } = DateTimeOffset.Now;

    public bool HasName => !string.IsNullOrWhiteSpace(UserName);

    public void Reset()
    {
        UserName = null;
        FavouriteTopic = null;
        LastTopic = null;
        LastUserMessage = null;
    }
}