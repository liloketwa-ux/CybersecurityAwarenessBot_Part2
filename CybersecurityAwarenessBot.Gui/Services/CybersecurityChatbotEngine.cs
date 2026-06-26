using System.Drawing;
using System.Text.RegularExpressions;
using CybersecurityAwarenessBot.Gui.Models;

namespace CybersecurityAwarenessBot.Gui.Services;

/// <summary>
/// Core cybersecurity chatbot engine (Parts 1 + 2 functionality preserved).
/// Handles cybersecurity topic responses, sentiment detection, follow-up tips,
/// and name/favourite-topic memory.
/// Part 3: Unknown-prompt messages and help text updated to mention new features.
/// </summary>
public sealed class CybersecurityChatbotEngine
{
    // ── Compiled patterns ────────────────────────────────────────────────────

    private static readonly Regex NameRegex = new(
        @"\b(?:my name is|i am|i'm|call me)\s+(?<name>[a-zA-Z][a-zA-Z'\-\s]{0,35})\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex FavouriteTopicRegex = new(
        @"\b(?:i(?:'m| am)? interested in|i like|my favourite topic is|my favorite topic is|i care about)" +
        @"\s+(?<topic>[a-zA-Z][a-zA-Z\s\-]{1,40})\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // ── State ────────────────────────────────────────────────────────────────

    private readonly ConversationState _state = new();
    private readonly IReadOnlyDictionary<string, TopicCard> _topics = TopicLibrary.CreateTopics();
    private readonly Random _random = new();

    private readonly List<string> _unknownPrompts = new()
    {
        "I am not sure I understand that. Ask about passwords, phishing, privacy, or links — " +
        "or try 'start quiz', 'add task', or 'show log'.",

        "I did not catch that clearly. You can ask about cybersecurity topics such as malware, " +
        "scams, or privacy, or type 'start quiz' to test your knowledge.",

        "Try rephrasing. I can help with cybersecurity topics, or use commands like " +
        "'add task [title]', 'show tasks', 'start quiz', or 'show activity log'."
    };

    // ── Public API ────────────────────────────────────────────────────────────

    public ConversationState State => _state;

    public BotResponse Process(string input) => Resolve(input);

    // ── Resolution pipeline ──────────────────────────────────────────────────

    private BotResponse Resolve(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return new BotResponse(
                "Input needed",
                "Please type a message. Ask about phishing, passwords, or privacy — or try " +
                "'start quiz', 'add task', or 'show activity log'.",
                Color.IndianRed);
        }

        string input  = rawInput.Trim();
        string lowered = input.ToLowerInvariant();
        _state.LastUserMessage = input;

        if (TryHandleReset(lowered, out BotResponse? r1))         return r1!;
        if (TryCaptureName(input, lowered, out BotResponse? r2))  return r2!;
        if (TryCaptureFavouriteTopic(input, lowered, out BotResponse? r3)) return r3!;
        if (TryHandleFollowUp(lowered, out BotResponse? r4))      return r4!;
        if (IsGreeting(lowered))                                   return BuildGreetingResponse();

        string? key = FindTopicKey(lowered);
        if (!string.IsNullOrWhiteSpace(key))
        {
            _state.LastTopic = key;
            TopicCard topic  = _topics[key];
            SentimentLevel s = SentimentDetector.Detect(lowered);

            return s is SentimentLevel.Worried or SentimentLevel.Frustrated or SentimentLevel.Confused
                ? BuildEmpatheticTopicResponse(topic, s, includeMemory: true)
                : BuildTopicResponse(topic, s, includeMemory: true);
        }

        SentimentLevel sentiment = SentimentDetector.Detect(lowered);
        if (sentiment is SentimentLevel.Worried or SentimentLevel.Frustrated or SentimentLevel.Confused)
            return BuildSentimentOnlyResponse(sentiment);

        if (Has(lowered, "help", "menu", "topics", "what can you do"))
            return BuildHelpResponse();

        if (Has(lowered, "who are you", "what do you do", "purpose"))
            return BuildIdentityResponse();

        if (Has(lowered, "tip", "advice", "staying safe"))
            return BuildGeneralSafetyTip();

        return new BotResponse(
            "Try another question",
            _unknownPrompts[_random.Next(_unknownPrompts.Count)],
            Color.Goldenrod);
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private bool TryHandleReset(string lowered, out BotResponse? response)
    {
        if (!Has(lowered, "forget me", "reset memory", "clear memory", "start over"))
        {
            response = null;
            return false;
        }

        string oldName = _state.UserName ?? "there";
        _state.Reset();
        response = new BotResponse(
            "Memory cleared",
            $"Okay, {oldName}. I have cleared your remembered details. Please type your name again when you are ready.",
            Color.MediumPurple);
        return true;
    }

    private bool TryCaptureName(string input, string lowered, out BotResponse? response)
    {
        if (_state.HasName) { response = null; return false; }

        Match m = NameRegex.Match(input);
        if (m.Success)
        {
            _state.UserName = FormatName(CleanCandidate(m.Groups["name"].Value));
            response = new BotResponse(
                "Nice to meet you",
                $"Welcome, {_state.UserName}! I will remember your name. Ask me about phishing, " +
                "passwords, privacy, links, malware, or social engineering — or try 'start quiz'.",
                Color.LightGreen);
            return true;
        }

        if (!Has(lowered, "?", "phishing", "scam", "privacy", "password",
                            "malware", "link", "social", "help", "task", "quiz"))
        {
            string candidate = CleanCandidate(input);
            if (LooksLikeAName(candidate))
            {
                _state.UserName = FormatName(candidate);
                response = new BotResponse(
                    "Nice to meet you",
                    $"Thanks, {_state.UserName}! I will remember your name. Ask me anything about cybersecurity whenever you are ready.",
                    Color.LightGreen);
                return true;
            }
        }

        response = null;
        return false;
    }

    private bool TryCaptureFavouriteTopic(string input, string lowered, out BotResponse? response)
    {
        Match m = FavouriteTopicRegex.Match(input);
        if (!m.Success) { response = null; return false; }

        string topicText = CleanCandidate(m.Groups["topic"].Value).ToLowerInvariant();
        string? topicKey = FindTopicKey(topicText) ?? topicText;

        _state.FavouriteTopic = topicKey;
        _state.LastTopic      = topicKey;

        string display   = DisplayTopic(topicKey);
        string namePart  = _state.HasName ? $"{_state.UserName}, " : string.Empty;
        string followUp  = GetMemoryFollowUp(topicKey);

        response = new BotResponse(
            "Memory saved",
            $"{namePart}great! I'll remember that you are interested in {display}. {followUp}",
            Color.LightCyan,
            Topic: topicKey);
        return true;
    }

    private bool TryHandleFollowUp(string lowered, out BotResponse? response)
    {
        if (!IsFollowUp(lowered)) { response = null; return false; }

        if (string.IsNullOrWhiteSpace(_state.LastTopic))
        {
            response = new BotResponse(
                "Follow-up",
                "I can continue the last topic, but I need a topic first. Ask about phishing, " +
                "passwords, privacy, suspicious links, malware, or social engineering.",
                Color.Khaki);
            return true;
        }

        string? canonKey = GetCanonicalTopicKey(_state.LastTopic);
        if (string.IsNullOrWhiteSpace(canonKey) || !_topics.ContainsKey(canonKey))
        {
            response = new BotResponse(
                "Follow-up",
                "I lost track of the last topic. Try asking about a specific cybersecurity subject.",
                Color.Khaki);
            return true;
        }

        TopicCard topic = _topics[canonKey];
        string    intro = ComposeAddressingPrefix();
        string    tip   = PickRandom(topic.DeepDiveTips);

        response = new BotResponse(
            $"More on {topic.DisplayName}",
            $"{intro}{tip}\n\n{topic.MemoryReminder}",
            Color.DeepSkyBlue,
            Topic: topic.Key,
            ShowFollowUpHint: true);
        return true;
    }

    // ── Response builders ────────────────────────────────────────────────────

    private BotResponse BuildGreetingResponse()
    {
        string name = _state.HasName ? $"{_state.UserName}, " : string.Empty;
        return new BotResponse(
            "Hello",
            $"{name}welcome! I am your Cybersecurity Awareness Assistant. Ask about phishing, passwords, " +
            "privacy, links, malware, or social engineering. You can also 'start quiz', 'add task', " +
            "or 'show activity log'.",
            Color.Cyan,
            ShowFollowUpHint: true);
    }

    private BotResponse BuildHelpResponse()
    {
        return new BotResponse(
            "What I can do",
            "Ask about: password safety, phishing scams, privacy protection, suspicious links, " +
            "malware, or social engineering.\n\n" +
            "Part 3 commands:\n" +
            "• add task [title]  —  save a cybersecurity task\n" +
            "• remind me to [task] in [N] days  —  add task with reminder\n" +
            "• show tasks  —  list your tasks\n" +
            "• start quiz  —  test your cybersecurity knowledge\n" +
            "• show activity log  —  see recent chatbot actions",
            Color.LightSkyBlue,
            ShowFollowUpHint: true);
    }

    private BotResponse BuildIdentityResponse()
    {
        string namePart   = _state.HasName ? $"Hello {_state.UserName}, " : string.Empty;
        string memoryPart = string.IsNullOrWhiteSpace(_state.FavouriteTopic)
            ? "I also remember details you share during the conversation."
            : $"I remember that you are interested in {DisplayTopic(_state.FavouriteTopic!)}.";
        return new BotResponse(
            "About me",
            $"{namePart}I am a cybersecurity awareness chatbot. I help people spot scams, protect " +
            $"accounts, and browse safely. {memoryPart} In Part 3 I also manage tasks, run quizzes, " +
            "and keep an activity log.",
            Color.CornflowerBlue);
    }

    private BotResponse BuildGeneralSafetyTip()
    {
        string? memKey = GetMemoryTopicKey();
        if (!string.IsNullOrWhiteSpace(memKey) && _topics.ContainsKey(memKey))
        {
            TopicCard t = _topics[memKey];
            return new BotResponse(
                $"Tip for {t.DisplayName}",
                $"{ComposeAddressingPrefix()}{PickRandom(t.QuickTips)}\n\n{t.MemoryReminder}",
                Color.MediumSeaGreen, Topic: t.Key, ShowFollowUpHint: true);
        }

        string[] tips =
        {
            "Pause before you click or share anything sensitive.",
            "Keep your software updated and use multi-factor authentication wherever possible.",
            "Verify unexpected messages through official channels before acting."
        };
        return new BotResponse("General safety tip", PickRandom(tips), Color.MediumSeaGreen, ShowFollowUpHint: true);
    }

    private BotResponse BuildSentimentOnlyResponse(SentimentLevel s) => s switch
    {
        SentimentLevel.Worried    => new BotResponse("You are not alone",
            "It is completely understandable to feel worried about online scams. I can help by " +
            "giving you simple steps to check messages, links, and requests more safely.", Color.Orange),
        SentimentLevel.Frustrated => new BotResponse("Let us make it simpler",
            "I understand the frustration. Let us take it one step at a time: stop, verify, and " +
            "only then act.", Color.OrangeRed),
        SentimentLevel.Confused   => new BotResponse("I can explain more",
            "No problem. I can explain any topic in simpler steps. Try asking for a phishing tip, " +
            "password advice, or privacy guidance.", Color.Khaki),
        _                         => new BotResponse("I am here to help",
            "Ask me anything about online safety and I will guide you through it.", Color.LightBlue)
    };

    private BotResponse BuildTopicResponse(TopicCard topic, SentimentLevel s, bool includeMemory)
    {
        string intro  = s == SentimentLevel.Curious ? "Good question. " : s == SentimentLevel.Positive ? "Great! " : string.Empty;
        string body   = PickRandom(topic.QuickTips);
        string prefix = includeMemory ? ComposeAddressingPrefix() : string.Empty;
        if (_state.HasName && !string.IsNullOrWhiteSpace(_state.FavouriteTopic) && IsRelatedToFavouriteTopic(topic.Key))
            prefix += $"As someone interested in {DisplayTopic(_state.FavouriteTopic!)}, ";
        return new BotResponse(topic.DisplayName,
            $"{intro}{prefix}{body}\n\n{topic.MemoryReminder}",
            Color.LightGreen, Topic: topic.Key, Sentiment: s, ShowFollowUpHint: true);
    }

    private BotResponse BuildEmpatheticTopicResponse(TopicCard topic, SentimentLevel s, bool includeMemory)
    {
        string support = s == SentimentLevel.Worried ? "It is okay to feel worried. " :
                          s == SentimentLevel.Frustrated ? "I understand the frustration. " :
                          "Let us keep this simple. ";
        string prefix  = includeMemory ? ComposeAddressingPrefix() : string.Empty;
        return new BotResponse(topic.DisplayName,
            $"{support}{prefix}{PickRandom(topic.QuickTips)}\n\n{topic.MemoryReminder}",
            Color.Orange, Topic: topic.Key, Sentiment: s, ShowFollowUpHint: true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string ComposeAddressingPrefix() => _state.HasName ? $"{_state.UserName}, " : string.Empty;
    private bool   IsGreeting(string l)   => Has(l, "hello", "hi", "hey", "good morning", "good afternoon", "good evening");
    private bool   IsFollowUp(string l)   => Has(l, "tell me more", "more info", "another tip", "explain more", "go on", "continue", "elaborate", "why");

    private string? FindTopicKey(string lowered)
    {
        foreach (var kv in _topics)
        {
            if (kv.Value.Key.Equals(lowered, StringComparison.OrdinalIgnoreCase)) return kv.Key;
            if (kv.Value.Keywords.Any(k => lowered.Contains(k, StringComparison.OrdinalIgnoreCase))) return kv.Key;
        }
        return null;
    }

    private string? GetCanonicalTopicKey(string? t) => string.IsNullOrWhiteSpace(t) ? null : FindTopicKey(t);

    private bool IsRelatedToFavouriteTopic(string key)
        => !string.IsNullOrWhiteSpace(_state.FavouriteTopic) &&
           string.Equals(_state.FavouriteTopic, key, StringComparison.OrdinalIgnoreCase);

    private string? GetMemoryTopicKey()
        => !string.IsNullOrWhiteSpace(_state.FavouriteTopic) && _topics.ContainsKey(_state.FavouriteTopic)
            ? _state.FavouriteTopic : null;

    private string GetMemoryFollowUp(string key)
    {
        string? canon = GetCanonicalTopicKey(key);
        return canon != null && _topics.TryGetValue(canon, out var t) ? t.MemoryReminder : string.Empty;
    }

    private string DisplayTopic(string key)
        => _topics.TryGetValue(key, out var t) ? t.DisplayName.ToLowerInvariant() : key.ToLowerInvariant();

    private string PickRandom(IReadOnlyList<string> list) => list[_random.Next(list.Count)];
    private static bool Has(string s, params string[] terms) => terms.Any(t => s.Contains(t, StringComparison.OrdinalIgnoreCase));
    private static string CleanCandidate(string v) => v.Trim().Trim('.', ',', '!', '?', '"', '\'');
    private static string FormatName(string v)
    {
        v = Regex.Replace(v, @"\s+", " ");
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(v.ToLowerInvariant());
    }
    private static bool LooksLikeAName(string v)
        => !string.IsNullOrWhiteSpace(v) && v.Length <= 35 && v.All(c => char.IsLetter(c) || c is ' ' or '\'' or '-');
}
