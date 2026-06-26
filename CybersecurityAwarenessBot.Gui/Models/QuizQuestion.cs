namespace CybersecurityAwarenessBot.Gui.Models;

/// <summary>
/// A single cybersecurity quiz question.
/// Supports both multiple-choice (Options A–D) and True/False formats.
/// </summary>
public sealed record QuizQuestion(
    string   Text,
    string[] Options,
    string   CorrectAnswer,
    string   Explanation,
    bool     IsTrueFalse = false
);
