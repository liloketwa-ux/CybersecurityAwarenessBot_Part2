using System.Text.RegularExpressions;

namespace CybersecurityAwarenessBot.Gui.Services;

// ── Intent taxonomy ──────────────────────────────────────────────────────────

/// <summary>
/// Describes what the user is trying to accomplish, detected via keyword
/// analysis (NLP simulation with string manipulation and regular expressions).
/// </summary>
public enum UserIntent
{
    None,           // Unrecognised — route to the main chatbot engine
    AddTask,        // "add task / remind me to ..."
    ViewTasks,      // "show tasks / my tasks ..."
    CompleteTask,   // "complete / mark done ..."
    DeleteTask,     // "delete / remove task ..."
    StartQuiz,      // "start quiz / quiz me ..."
    ShowActivityLog // "show log / what have you done ..."
}

/// <summary>
/// Carries the intent and any data extracted from the user's message.
/// </summary>
public sealed class NlpResult
{
    public UserIntent Intent        { get; init; } = UserIntent.None;
    public string?    TaskTitle     { get; init; }
    public DateTime?  ReminderDate  { get; init; }
    public string?    TargetKeyword { get; init; }   // For complete/delete by keyword
}

// ── Main processor ───────────────────────────────────────────────────────────

/// <summary>
/// Simulates Natural Language Processing using keyword detection and regular
/// expressions to understand user intent even when phrased in different ways.
/// </summary>
public static class NlpProcessor
{
    // Compiled regex patterns for data extraction
    private static readonly Regex AddTaskPattern = new(
        @"\b(?:add|create|new|set up)\s+(?:a\s+)?task\s*[-:–]?\s*(?<title>.+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RemindMePattern = new(
        @"\b(?:remind|remember)\s+me\s+(?:about\s+|of\s+)?(?:to\s+)?(?<title>.+?)" +
        @"(?:\s+in\s+(?<days>\d+)\s+days?|\s+tomorrow|\s+next\s+week|\s+in\s+a\s+week)?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DaysPattern = new(
        @"in\s+(?<days>\d+)\s+days?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CompletePattern = new(
        @"\b(?:complete|mark|finish|done)\s+(?:task\s+)?(?:called\s+)?[""']?(?<kw>[^""']+?)[""']?" +
        @"\s*(?:as\s+(?:done|complete|completed|finished))?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DeletePattern = new(
        @"\b(?:delete|remove|cancel)\s+(?:task\s+)?(?:called\s+)?[""']?(?<kw>[^""']+?)[""']?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // ── Entry point ───────────────────────────────────────────────────────

    /// <summary>
    /// Analyses <paramref name="input"/> and returns the detected intent plus
    /// any structured data (task title, reminder date, target keyword).
    /// </summary>
    public static NlpResult Analyse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new NlpResult();

        string lowered = input.Trim().ToLowerInvariant();

        // 1. Quiz intent ------------------------------------------------
        if (Has(lowered,
            "start quiz", "quiz me", "play quiz", "take quiz", "begin quiz",
            "start the quiz", "cybersecurity quiz", "test my knowledge",
            "take a quiz", "do the quiz"))
        {
            return new NlpResult { Intent = UserIntent.StartQuiz };
        }

        // 2. Activity log intent ----------------------------------------
        if (Has(lowered,
            "show activity log", "activity log", "show log", "view log",
            "what have you done", "what have you done for me",
            "recent actions", "action history", "show history",
            "view history", "show actions", "list actions", "my history"))
        {
            return new NlpResult { Intent = UserIntent.ShowActivityLog };
        }

        // 3. View tasks intent ------------------------------------------
        if (Has(lowered,
            "view tasks", "show tasks", "my tasks", "list tasks",
            "show my tasks", "view my tasks", "all tasks", "show all tasks",
            "list my tasks", "what tasks", "task list", "pending tasks"))
        {
            return new NlpResult { Intent = UserIntent.ViewTasks };
        }

        // 4. Complete task intent ---------------------------------------
        if (Has(lowered,
            "complete task", "mark task", "finish task", "task done",
            "mark as done", "mark as complete", "mark as completed",
            "set task done", "i finished", "i completed"))
        {
            var m = CompletePattern.Match(input);
            return new NlpResult
            {
                Intent        = UserIntent.CompleteTask,
                TargetKeyword = m.Success ? m.Groups["kw"].Value.Trim() : null
            };
        }

        // 5. Delete task intent -----------------------------------------
        if (Has(lowered, "delete task", "remove task", "cancel task"))
        {
            var m = DeletePattern.Match(input);
            return new NlpResult
            {
                Intent        = UserIntent.DeleteTask,
                TargetKeyword = m.Success ? m.Groups["kw"].Value.Trim() : null
            };
        }

        // 6. Add task — "remind me to …" pattern -----------------------
        var remindMatch = RemindMePattern.Match(input);
        if (remindMatch.Success)
        {
            string title = remindMatch.Groups["title"].Value.Trim();
            DateTime? date = ExtractReminderDate(lowered, remindMatch);
            return new NlpResult
            {
                Intent       = UserIntent.AddTask,
                TaskTitle    = title,
                ReminderDate = date
            };
        }

        // 7. Add task — "add task …" / "create task …" pattern ---------
        var addMatch = AddTaskPattern.Match(input);
        if (addMatch.Success)
        {
            string title = addMatch.Groups["title"].Value.Trim();
            // Strip inline days phrase from the title (e.g. "enable 2FA in 3 days")
            DateTime? date = null;
            var dm = DaysPattern.Match(title);
            if (dm.Success)
            {
                date  = DateTime.Now.AddDays(int.Parse(dm.Groups["days"].Value));
                title = DaysPattern.Replace(title, string.Empty).Trim().TrimEnd('-', '–', ',');
            }
            return new NlpResult
            {
                Intent       = UserIntent.AddTask,
                TaskTitle    = title,
                ReminderDate = date
            };
        }

        // 8. Implicit "add task" triggers (generic phrases) ------------
        if (Has(lowered, "add a task", "create a task", "new task", "add new task"))
        {
            return new NlpResult { Intent = UserIntent.AddTask };
        }

        // No match → let the main chatbot engine handle it
        return new NlpResult { Intent = UserIntent.None };
    }

    // ── Reminder confirmation helpers ─────────────────────────────────────

    /// <summary>
    /// Returns true when the message is a reminder confirmation and sets
    /// <paramref name="days"/> to the parsed or inferred number of days.
    /// </summary>
    public static bool IsReminderConfirmation(string input, out int days)
    {
        days = 0;
        string low = input.Trim().ToLowerInvariant();

        var dm = DaysPattern.Match(low);
        if (dm.Success)     { days = int.Parse(dm.Groups["days"].Value); return true; }
        if (low.Contains("tomorrow"))                                    { days = 1;  return true; }
        if (low.Contains("next week") || low.Contains("in a week"))      { days = 7;  return true; }

        // Plain affirmative — default to 7 days
        if (Has(low, "yes", "yeah", "sure", "ok", "okay", "please", "yes please",
                     "remind me", "set a reminder", "add reminder"))
        {
            days = 7;
            return true;
        }
        return false;
    }

    /// <summary>Returns true when the message clearly declines a reminder.</summary>
    public static bool IsReminderRejection(string input)
    {
        string low = input.Trim().ToLowerInvariant();
        return Has(low, "no", "nope", "no thanks", "not now", "skip",
                        "no reminder", "no need", "don't remind", "without reminder");
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private static bool Has(string input, params string[] terms)
        => terms.Any(t => input.Contains(t, StringComparison.OrdinalIgnoreCase));

    private static DateTime? ExtractReminderDate(string lowered, Match remindMatch)
    {
        if (remindMatch.Groups["days"].Success)
            return DateTime.Now.AddDays(int.Parse(remindMatch.Groups["days"].Value));
        if (lowered.Contains("tomorrow"))
            return DateTime.Now.AddDays(1);
        if (lowered.Contains("next week") || lowered.Contains("in a week"))
            return DateTime.Now.AddDays(7);
        return null;
    }
}
