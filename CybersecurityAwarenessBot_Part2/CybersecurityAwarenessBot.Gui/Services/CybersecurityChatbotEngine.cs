using System.Drawing;
using System.Text.RegularExpressions;
using CybersecurityAwarenessBot.Gui.Models;

namespace CybersecurityAwarenessBot.Gui.Services;

public sealed class CybersecurityChatbotEngine
{
    private static readonly Regex NameRegex = new(
        @"\b(?:my name is|i am|i'm|call me)\s+(?<name>[a-zA-Z][a-zA-Z'\-\s]{0,35})\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex FavouriteTopicRegex = new(
        @"\b(?:i(?:'m| am)? interested in|i like|my favourite topic is|my favorite topic is|i care about)\s+(?<topic>[a-zA-Z][a-zA-Z\s\-]{1,40})\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly ConversationState _state = new();
    private readonly IReadOnlyDictionary<string, TopicCard> _topics = TopicLibrary.CreateTopics();
    private readonly Random _random = new();
    private readonly ResponseResolver _responseResolver;
    private readonly List<string> _unknownPrompts = new()
    {
        "I am not sure I understand that yet. Try asking about password safety, scams, privacy, suspicious links, malware, or social engineering.",
        "I did not catch that clearly. You can ask me for a phishing tip, password advice, or help with suspicious links.",
        "Try rephrasing your question. I can help with cybersecurity topics such as passwords, privacy, scams, and malware."
    };

    public ConversationState State => _state;

    public CybersecurityChatbotEngine()
    {
        _responseResolver = Resolve;
    }

    public BotResponse Process(string input) => _responseResolver(input);

    private BotResponse Resolve(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return new BotResponse(
                "Input needed",
                "Please type a message so I can help you. You can start with your name or ask about phishing, passwords, privacy, or suspicious links.",
                Color.IndianRed);
        }

        string input = rawInput.Trim();
        string lowered = input.ToLowerInvariant();
        _state.LastUserMessage = input;

        if (TryHandleReset(lowered, out BotResponse? resetResponse))
        {
            return resetResponse!;
        }

        if (TryCaptureName(input, lowered, out BotResponse? nameResponse))
        {
            return nameResponse!;
        }

        if (TryCaptureFavouriteTopic(input, lowered, out BotResponse? topicMemoryResponse))
        {
            return topicMemoryResponse!;
        }

        if (TryHandleFollowUp(lowered, out BotResponse? followUpResponse))
        {
            return followUpResponse!;
        }

        if (IsGreeting(lowered))
        {
            return BuildGreetingResponse();
        }

        string? matchedTopicKey = FindTopicKey(lowered);

        if (!string.IsNullOrWhiteSpace(matchedTopicKey))
        {
            _state.LastTopic = matchedTopicKey;
            TopicCard topic = _topics[matchedTopicKey];
            SentimentLevel sentiment = SentimentDetector.Detect(lowered);

            if (sentiment is SentimentLevel.Worried or SentimentLevel.Frustrated or SentimentLevel.Confused)
            {
                return BuildEmpatheticTopicResponse(topic, sentiment, includeMemory: true);
            }

            return BuildTopicResponse(topic, sentiment, includeMemory: true);
        }

        SentimentLevel detectedSentiment = SentimentDetector.Detect(lowered);
        if (detectedSentiment is SentimentLevel.Worried or SentimentLevel.Frustrated or SentimentLevel.Confused)
        {
            return BuildSentimentOnlyResponse(detectedSentiment);
        }

        if (ContainsAny(lowered, "help", "menu", "topics", "what can you do"))
        {
            return BuildHelpResponse();
        }

        if (ContainsAny(lowered, "who are you", "what do you do", "purpose"))
        {
            return BuildIdentityResponse();
        }

        if (ContainsAny(lowered, "tip", "advice", "staying safe"))
        {
            return BuildGeneralSafetyTip();
        }

        return new BotResponse(
            "Try another question",
            _unknownPrompts[_random.Next(_unknownPrompts.Count)],
            Color.Goldenrod);
    }

    private bool TryHandleReset(string lowered, out BotResponse? response)
    {
        if (ContainsAny(lowered, "forget me", "reset memory", "clear memory", "start over"))
        {
            string oldName = _state.UserName ?? "there";
            _state.Reset();

            response = new BotResponse(
                "Memory cleared",
                $"Okay, {oldName}. I have cleared the remembered details. Please type your name again when you are ready.",
                Color.MediumPurple,
                Topic: null,
                Sentiment: SentimentLevel.Neutral);

            return true;
        }

        response = null;
        return false;
    }

    private bool TryCaptureName(string input, string lowered, out BotResponse? response)
    {
        if (_state.HasName)
        {
            response = null;
            return false;
        }

        Match match = NameRegex.Match(input);

        if (match.Success)
        {
            string name = CleanCandidate(match.Groups["name"].Value);
            _state.UserName = FormatName(name);

            response = new BotResponse(
                "Nice to meet you",
                $"Welcome, {_state.UserName}. I will remember your name. You can now ask me about phishing, passwords, privacy, suspicious links, malware, or social engineering.",
                Color.LightGreen);

            return true;
        }

        if (!ContainsAny(lowered, "?", "phishing", "scam", "privacy", "password", "malware", "link", "social", "help"))
        {
            string name = CleanCandidate(input);
            if (LooksLikeAName(name))
            {
                _state.UserName = FormatName(name);

                response = new BotResponse(
                    "Nice to meet you",
                    $"Thanks, {_state.UserName}. I will remember your name. Ask me about cybersecurity topics whenever you are ready.",
                    Color.LightGreen);

                return true;
            }
        }

        response = null;
        return false;
    }

    private bool TryCaptureFavouriteTopic(string input, string lowered, out BotResponse? response)
    {
        Match match = FavouriteTopicRegex.Match(input);

        if (!match.Success)
        {
            response = null;
            return false;
        }

        string topicText = CleanCandidate(match.Groups["topic"].Value).ToLowerInvariant();
        string? topicKey = FindTopicKey(topicText);

        if (string.IsNullOrWhiteSpace(topicKey))
        {
            topicKey = topicText;
        }

        _state.FavouriteTopic = topicKey;
        _state.LastTopic = topicKey;

        string display = DisplayTopic(topicKey);
        string namePart = _state.HasName ? $"{_state.UserName}, " : string.Empty;

        response = new BotResponse(
            "Memory saved",
            $"{namePart}great! I'll remember that you are interested in {display}. {GetMemoryFollowUp(topicKey)}",
            Color.LightCyan,
            Topic: topicKey);

        return true;
    }

    private bool TryHandleFollowUp(string lowered, out BotResponse? response)
    {
        if (!IsFollowUp(lowered))
        {
            response = null;
            return false;
        }

        if (string.IsNullOrWhiteSpace(_state.LastTopic))
        {
            response = new BotResponse(
                "Follow-up",
                "I can continue the last topic, but I need a topic first. Try asking about phishing, passwords, privacy, suspicious links, malware, or social engineering.",
                Color.Khaki);
            return true;
        }

        TopicCard topic = _topics[GetCanonicalTopicKey(_state.LastTopic)!];
        string tip = PickRandom(topic.DeepDiveTips);
        string intro = ComposeAddressingPrefix();

        response = new BotResponse(
            $"More on {topic.DisplayName}",
            $"{intro}{tip}\n\n{topic.MemoryReminder}",
            Color.DeepSkyBlue,
            Topic: topic.Key,
            ShowFollowUpHint: true);

        return true;
    }

    private BotResponse BuildGreetingResponse()
    {
        string name = _state.HasName ? $"{_state.UserName}, " : string.Empty;
        return new BotResponse(
            "Hello",
            $"{name}welcome! I am your Cybersecurity Awareness Assistant. You can ask me about phishing, passwords, privacy, suspicious links, malware, or social engineering.",
            Color.Cyan,
            ShowFollowUpHint: true);
    }

    private BotResponse BuildHelpResponse()
    {
        return new BotResponse(
            "Topics you can ask about",
            "Try asking about password safety, phishing scams, privacy protection, suspicious links, malware, or social engineering. You can also say 'tell me more' after any topic.",
            Color.LightSkyBlue,
            ShowFollowUpHint: true);
    }

    private BotResponse BuildIdentityResponse()
    {
        string namePart = _state.HasName ? $"Hello {_state.UserName}, " : string.Empty;
        string memoryPart = string.IsNullOrWhiteSpace(_state.FavouriteTopic)
            ? "I also remember details you share during the conversation."
            : $"I remember that you are interested in {DisplayTopic(_state.FavouriteTopic!)}.";

        return new BotResponse(
            "About me",
            $"{namePart}I am a cybersecurity awareness chatbot built to help people spot scams, protect accounts, and browse more safely. {memoryPart}",
            Color.CornflowerBlue);
    }

    private BotResponse BuildGeneralSafetyTip()
    {
        string? memoryTopic = GetMemoryTopicKey();
        if (!string.IsNullOrWhiteSpace(memoryTopic) && _topics.ContainsKey(memoryTopic))
        {
            TopicCard topic = _topics[memoryTopic];
            return new BotResponse(
                $"A tip for {topic.DisplayName}",
                $"{ComposeAddressingPrefix()}{PickRandom(topic.QuickTips)}\n\n{topic.MemoryReminder}",
                Color.MediumSeaGreen,
                Topic: topic.Key,
                ShowFollowUpHint: true);
        }

        string[] generalTips =
        {
            "Pause before you click or share anything sensitive.",
            "Keep your software updated and use multi-factor authentication.",
            "Verify unexpected messages through official channels."
        };

        return new BotResponse(
            "General safety tip",
            PickRandom(generalTips),
            Color.MediumSeaGreen,
            ShowFollowUpHint: true);
    }

    private BotResponse BuildSentimentOnlyResponse(SentimentLevel sentiment)
    {
        return sentiment switch
        {
            SentimentLevel.Worried => new BotResponse(
                "You are not alone",
                "It is completely understandable to feel worried about scams. I can help by giving you simple steps to check messages, links, and requests more safely.",
                Color.Orange),
            SentimentLevel.Frustrated => new BotResponse(
                "Let us make it simpler",
                "I understand that this can feel frustrating. Let us take it one step at a time: stop, verify, and only then act.",
                Color.OrangeRed),
            SentimentLevel.Confused => new BotResponse(
                "I can explain more",
                "No problem. I can explain the topic in simpler steps. Try asking for a phishing tip, password advice, or privacy help.",
                Color.Khaki),
            _ => new BotResponse(
                "I am here to help",
                "Ask me anything about online safety and I will guide you through it.",
                Color.LightBlue)
        };
    }

    private BotResponse BuildTopicResponse(TopicCard topic, SentimentLevel sentiment, bool includeMemory)
    {
        string intro = sentiment switch
        {
            SentimentLevel.Curious => "That is a good question. ",
            SentimentLevel.Positive => "Great question! ",
            _ => string.Empty
        };

        string body = PickRandom(topic.QuickTips);
        string prefix = includeMemory ? ComposeAddressingPrefix() : string.Empty;

        if (_state.HasName && !string.IsNullOrWhiteSpace(_state.FavouriteTopic) && IsRelatedToFavouriteTopic(topic.Key))
        {
            prefix += $"As someone interested in {DisplayTopic(_state.FavouriteTopic!)}, ";
        }

        return new BotResponse(
            topic.DisplayName,
            $"{intro}{prefix}{body}\n\n{topic.MemoryReminder}",
            Color.LightGreen,
            Topic: topic.Key,
            Sentiment: sentiment,
            ShowFollowUpHint: true);
    }

    private BotResponse BuildEmpatheticTopicResponse(TopicCard topic, SentimentLevel sentiment, bool includeMemory)
    {
        string supportiveSentence = sentiment switch
        {
            SentimentLevel.Worried => "It is okay to feel worried. ",
            SentimentLevel.Frustrated => "I understand the frustration. ",
            SentimentLevel.Confused => "Let us keep this simple. ",
            _ => string.Empty
        };

        string tip = PickRandom(topic.QuickTips);
        string prefix = includeMemory ? ComposeAddressingPrefix() : string.Empty;

        return new BotResponse(
            topic.DisplayName,
            $"{supportiveSentence}{prefix}{tip}\n\n{topic.MemoryReminder}",
            Color.Orange,
            Topic: topic.Key,
            Sentiment: sentiment,
            ShowFollowUpHint: true);
    }

    private string ComposeAddressingPrefix()
    {
        return _state.HasName ? $"{_state.UserName}, " : string.Empty;
    }

    private bool IsGreeting(string lowered)
        => ContainsAny(lowered, "hello", "hi", "hey", "good morning", "good afternoon", "good evening");

    private bool IsFollowUp(string lowered)
        => ContainsAny(lowered, "tell me more", "more info", "another tip", "explain more", "go on", "continue", "elaborate", "why");

    private string? FindTopicKey(string lowered)
    {
        foreach (KeyValuePair<string, TopicCard> pair in _topics)
        {
            if (pair.Value.Key.Equals(lowered, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Key;
            }

            if (pair.Value.Keywords.Any(keyword => lowered.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return pair.Key;
            }
        }

        return null;
    }

    private string? GetCanonicalTopicKey(string? maybeTopic)
    {
        if (string.IsNullOrWhiteSpace(maybeTopic))
        {
            return null;
        }

        return FindTopicKey(maybeTopic);
    }

    private bool IsRelatedToFavouriteTopic(string topicKey)
    {
        return !string.IsNullOrWhiteSpace(_state.FavouriteTopic)
               && string.Equals(_state.FavouriteTopic, topicKey, StringComparison.OrdinalIgnoreCase);
    }

    private string? GetMemoryTopicKey()
    {
        if (!string.IsNullOrWhiteSpace(_state.FavouriteTopic) && _topics.ContainsKey(_state.FavouriteTopic))
        {
            return _state.FavouriteTopic;
        }

        return null;
    }

    private string GetMemoryFollowUp(string topicKey)
    {
        TopicCard topic = _topics[GetCanonicalTopicKey(topicKey)!];
        return topic.MemoryReminder;
    }

    private string DisplayTopic(string topicKey)
    {
        if (_topics.TryGetValue(topicKey, out TopicCard? topic))
        {
            return topic.DisplayName.ToLowerInvariant();
        }

        return topicKey.ToLowerInvariant();
    }

    private string PickRandom(IReadOnlyList<string> options)
        => options[_random.Next(options.Count)];

    private bool ContainsAny(string input, params string[] terms)
        => terms.Any(term => input.Contains(term, StringComparison.OrdinalIgnoreCase));

    private static string CleanCandidate(string value)
    {
        return value.Trim().Trim('.', ',', '!', '?', '"', '\'');
    }

    private static string FormatName(string value)
    {
        value = Regex.Replace(value, @"\s+", " ");
        var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(value.ToLowerInvariant());
    }

    private static bool LooksLikeAName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.Length > 35)
        {
            return false;
        }

        return value.All(ch => char.IsLetter(ch) || ch is ' ' or '\'' or '-');
    }
}