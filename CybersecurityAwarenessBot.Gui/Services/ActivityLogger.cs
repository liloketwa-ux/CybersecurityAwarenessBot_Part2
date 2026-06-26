using CybersecurityAwarenessBot.Gui.Models;

namespace CybersecurityAwarenessBot.Gui.Services;

/// <summary>
/// Records significant chatbot actions during the session.
/// All entries are held in memory; the log is per-session (not persisted).
/// </summary>
public sealed class ActivityLogger
{
    private readonly List<ActivityLogEntry> _entries = new();

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>Appends a new log entry with the current timestamp.</summary>
    public void Log(string action, string? detail = null)
    {
        _entries.Add(new ActivityLogEntry(DateTime.Now, action, detail));
    }

    /// <summary>Returns the most recent <paramref name="count"/> entries (default 10).</summary>
    public IReadOnlyList<ActivityLogEntry> GetRecent(int count = 10)
    {
        int skip = Math.Max(0, _entries.Count - count);
        return _entries.Skip(skip).ToList().AsReadOnly();
    }

    /// <summary>Returns every logged entry for the full history view.</summary>
    public IReadOnlyList<ActivityLogEntry> GetAll()
        => _entries.AsReadOnly();

    /// <summary>Clears all log entries.</summary>
    public void Clear() => _entries.Clear();

    /// <summary>Total number of entries recorded this session.</summary>
    public int Count => _entries.Count;
}
