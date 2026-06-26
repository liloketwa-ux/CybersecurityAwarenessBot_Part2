namespace CybersecurityAwarenessBot.Gui.Models;

/// <summary>
/// Represents a cybersecurity-related task the user wants to track.
/// Maps directly to the cyber_tasks table in MySQL.
/// </summary>
public sealed class CyberTask
{
    public int       Id           { get; set; }
    public string    Title        { get; set; } = string.Empty;
    public string    Description  { get; set; } = string.Empty;
    public DateTime? ReminderDate { get; set; }
    public bool      IsCompleted  { get; set; }
    public DateTime  CreatedAt    { get; set; } = DateTime.Now;

    /// <summary>Returns a human-readable reminder string for display.</summary>
    public string ReminderDisplay =>
        ReminderDate.HasValue
            ? ReminderDate.Value.ToString("dd MMM yyyy")
            : "None";

    /// <summary>Returns a status string for display in the task list.</summary>
    public string StatusDisplay => IsCompleted ? "✓ Done" : "Pending";
}
