namespace CybersecurityAwarenessBot.Gui.Models;

/// <summary>
/// A single entry in the chatbot's activity log.
/// Stored in memory for the session; displayed in the Activity Log tab.
/// </summary>
public sealed record ActivityLogEntry(
    DateTime Timestamp,
    string   Action,
    string?  Detail = null
)
{
    /// <summary>Single-line string for display in the log panel.</summary>
    public override string ToString()
    {
        string detail = string.IsNullOrWhiteSpace(Detail)
            ? string.Empty
            : $"  →  {Detail}";
        return $"[{Timestamp:HH:mm:ss}]  {Action}{detail}";
    }
}
