using CybersecurityAwarenessBot.Gui.Models;

namespace CybersecurityAwarenessBot.Gui.Services;

/// <summary>
/// Manages the cybersecurity mini-game quiz session.
/// Questions are shuffled and presented one at a time with immediate feedback.
/// Supports multiple-choice (A–D) and True/False question types.
/// </summary>
public sealed class QuizEngine
{
    // ── Question bank (12 questions — 4 True/False, 8 Multiple-Choice) ──────

    private static readonly List<QuizQuestion> AllQuestions = new()
    {
        // ── 1. Phishing — MC ────────────────────────────────────────────────
        new QuizQuestion(
            "What should you do if you receive an email asking for your password?",
            new[] { "A) Reply with your password",
                    "B) Delete the email without reading it",
                    "C) Report the email as phishing",
                    "D) Ignore it and do nothing" },
            "C",
            "Reporting phishing emails helps your provider block similar attacks and " +
            "protects other users. Simply deleting or ignoring the email still leaves " +
            "the threat active for others."
        ),

        // ── 2. Password Strength — MC ────────────────────────────────────────
        new QuizQuestion(
            "Which of the following is the strongest password?",
            new[] { "A) password123",
                    "B) MyName1990!",
                    "C) T!ger$unset@Maple#99",
                    "D) 12345678" },
            "C",
            "Strong passwords mix uppercase, lowercase, digits, and special characters " +
            "without using common words or personal information. Longer is always stronger."
        ),

        // ── 3. Two-Factor Auth — T/F ─────────────────────────────────────────
        new QuizQuestion(
            "Two-factor authentication (2FA) makes your account significantly harder " +
            "to hack, even if an attacker already has your password.",
            new[] { "True", "False" },
            "True",
            "2FA adds a second verification step (such as a code sent to your phone). " +
            "An attacker would need both your password AND physical access to your second " +
            "factor — a much harder combination to beat.",
            IsTrueFalse: true
        ),

        // ── 4. HTTPS — MC ────────────────────────────────────────────────────
        new QuizQuestion(
            "What does the HTTPS prefix in a website address indicate?",
            new[] { "A) The website is completely safe to use",
                    "B) Your connection to the website is encrypted",
                    "C) The website has been approved by a government body",
                    "D) The website cannot collect your personal data" },
            "B",
            "HTTPS encrypts the data travelling between your browser and the server. " +
            "It does NOT mean the site itself is trustworthy — criminals also use HTTPS " +
            "on fake websites."
        ),

        // ── 5. Social Engineering — MC ───────────────────────────────────────
        new QuizQuestion(
            "An IT support agent calls and asks for your password to fix a problem. " +
            "What is the correct response?",
            new[] { "A) Give the password — they are from IT",
                    "B) Refuse and verify through your company's official IT channel",
                    "C) Give a fake password to test them",
                    "D) Hang up immediately without explanation" },
            "B",
            "Legitimate IT staff will NEVER ask for your password. Politely decline " +
            "and verify the request through an official channel (e.g., call the help-" +
            "desk number listed on your intranet, not a number the caller provides)."
        ),

        // ── 6. Malware & Antivirus — T/F ────────────────────────────────────
        new QuizQuestion(
            "Installing software from any website is safe as long as your antivirus " +
            "is running and up to date.",
            new[] { "True", "False" },
            "False",
            "Antivirus software cannot catch every threat, especially brand-new ('zero-" +
            "day') malware. Always download software from official or well-known trusted " +
            "sources to minimise the risk.",
            IsTrueFalse: true
        ),

        // ── 7. Public Wi-Fi — MC ─────────────────────────────────────────────
        new QuizQuestion(
            "You are at a coffee shop and need to check your bank account. " +
            "Which option is safest?",
            new[] { "A) Use the coffee shop's open Wi-Fi",
                    "B) Use a VPN on the open Wi-Fi",
                    "C) Use your mobile data connection",
                    "D) Both B and C are equally good options" },
            "D",
            "Mobile data is encrypted by your carrier, making it safer than open Wi-Fi. " +
            "A VPN on public Wi-Fi also encrypts your traffic. Either approach is far " +
            "better than using open Wi-Fi without any protection."
        ),

        // ── 8. Shortened URLs — T/F ──────────────────────────────────────────
        new QuizQuestion(
            "A shortened URL (e.g. bit.ly) is always safe to click because it hides " +
            "any suspicious-looking web address.",
            new[] { "True", "False" },
            "False",
            "Shortened URLs can disguise the real destination. If you are unsure of the " +
            "source, use a link-expansion tool (e.g. checkshorturl.com) to preview " +
            "where the link actually leads before clicking.",
            IsTrueFalse: true
        ),

        // ── 9. Password Reuse — MC ───────────────────────────────────────────
        new QuizQuestion(
            "Why is reusing the same password across multiple websites dangerous?",
            new[] { "A) It is not dangerous — convenience matters more",
                    "B) A single data breach can expose all accounts sharing that password",
                    "C) Websites share password databases with each other",
                    "D) It makes your internet connection slower" },
            "B",
            "If one site is breached, attackers test the stolen credentials on banking, " +
            "email, and social-media sites. This is called credential stuffing and it " +
            "works because so many people reuse passwords."
        ),

        // ── 10. Software Updates — MC ────────────────────────────────────────
        new QuizQuestion(
            "Why is it important to keep your operating system and applications updated?",
            new[] { "A) Updates are only for performance improvements",
                    "B) Updates patch known security vulnerabilities",
                    "C) Updating is optional and does not affect security",
                    "D) Updates only change the visual appearance" },
            "B",
            "Security patches in updates fix vulnerabilities that attackers could exploit. " +
            "Delaying updates leaves your system exposed to threats that vendors have " +
            "already identified and fixed."
        ),

        // ── 11. Ransomware & Backups — T/F ──────────────────────────────────
        new QuizQuestion(
            "Backing up your data to an offline location can protect you from " +
            "ransomware attacks.",
            new[] { "True", "False" },
            "True",
            "Ransomware encrypts your files and demands payment for the decryption key. " +
            "A recent offline (or offsite) backup lets you restore your data without " +
            "paying the ransom — removing the attacker's leverage entirely.",
            IsTrueFalse: true
        ),

        // ── 12. App Permissions — MC ─────────────────────────────────────────
        new QuizQuestion(
            "A flashlight app requests access to your contacts, microphone, camera, " +
            "and location. What is the correct response?",
            new[] { "A) Grant all permissions — the app probably needs them",
                    "B) Deny all permissions and uninstall the app",
                    "C) Grant only camera access (for the flash) and deny the rest",
                    "D) Contact the developer to ask why those permissions are needed" },
            "C",
            "A flashlight app only needs camera access (to activate the torch). " +
            "Requests for contacts, microphone, or location are red flags that the app " +
            "may be harvesting data. Grant only what the app genuinely needs to function."
        )
    };

    // ── Runtime state ─────────────────────────────────────────────────────────

    private List<QuizQuestion> _session  = new();
    private int                _index;
    private int                _score;
    private bool               _waitNext;   // true = answered, waiting for "Next"

    // ── Public properties ──────────────────────────────────────────────────

    public bool          IsActive      { get; private set; }
    public bool          IsAwaitingNext => IsActive && _waitNext;
    public bool          IsFinished     => IsActive && _waitNext && _index >= _session.Count;
    public QuizQuestion? Current        => (IsActive && _index < _session.Count) ? _session[_index] : null;
    public int           CurrentNumber  => _index + 1;
    public int           Total          => _session.Count;
    public int           Score          => _score;

    // ── Actions ────────────────────────────────────────────────────────────

    /// <summary>Shuffles all questions and starts a new session.</summary>
    public void Start(int count = 12)
    {
        _session  = AllQuestions.OrderBy(_ => Guid.NewGuid())
                                 .Take(Math.Min(count, AllQuestions.Count))
                                 .ToList();
        _index    = 0;
        _score    = 0;
        _waitNext = false;
        IsActive  = true;
    }

    /// <summary>
    /// Evaluates the user's answer and returns (isCorrect, explanation).
    /// Returns null if there is no active question or already waiting for Next.
    /// </summary>
    public (bool IsCorrect, string Explanation)? SubmitAnswer(string answer)
    {
        if (!IsActive || _waitNext || Current == null) return null;

        string cleaned = answer.Trim();
        bool correct;

        if (Current.IsTrueFalse)
        {
            correct = string.Equals(cleaned, Current.CorrectAnswer,
                                    StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // Accept "A", "A)", "A) full text…", or "A." — just compare first character
            string letter = cleaned.Length > 0
                ? cleaned[0].ToString().ToUpperInvariant()
                : string.Empty;
            correct = string.Equals(letter, Current.CorrectAnswer,
                                    StringComparison.OrdinalIgnoreCase);
        }

        if (correct) _score++;
        _waitNext = true;
        return (correct, Current.Explanation);
    }

    /// <summary>
    /// Advances to the next question. Returns false when the quiz is complete.
    /// </summary>
    public bool MoveNext()
    {
        if (!IsActive || !_waitNext) return false;
        _index++;
        _waitNext = false;
        return _index < _session.Count;
    }

    /// <summary>Ends the quiz session.</summary>
    public void End()
    {
        IsActive = false;
    }

    /// <summary>Returns a score-appropriate feedback message for the results screen.</summary>
    public string GetFinalFeedback()
    {
        double pct = _session.Count == 0 ? 0 : (double)_score / _session.Count * 100;
        return pct >= 90
            ? $"🏆  Outstanding! {_score}/{_session.Count} — You are a cybersecurity pro!"
            : pct >= 70
                ? $"👍  Great effort! {_score}/{_session.Count} — Keep building your knowledge."
                : pct >= 50
                    ? $"📚  Good start! {_score}/{_session.Count} — Review the topics you missed."
                    : $"💡  {_score}/{_session.Count} — Keep learning to stay safe online!";
    }
}
