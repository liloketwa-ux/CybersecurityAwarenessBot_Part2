namespace CybersecurityAwarenessBot.Gui.Services;

public sealed record TopicCard(
    string Key,
    string DisplayName,
    IReadOnlyList<string> Keywords,
    IReadOnlyList<string> QuickTips,
    IReadOnlyList<string> DeepDiveTips,
    string MemoryReminder
);

public static class TopicLibrary
{
    public static IReadOnlyDictionary<string, TopicCard> CreateTopics()
    {
        return new Dictionary<string, TopicCard>(StringComparer.OrdinalIgnoreCase)
        {
            ["password"] = new TopicCard(
                "password",
                "Password safety",
                new[] { "password", "passphrase", "mfa", "2fa", "multi-factor", "two-factor", "login" },
                new[]
                {
                    "Use a long passphrase with four or more unrelated words.",
                    "Never reuse the same password across important accounts.",
                    "Turn on multi-factor authentication wherever possible."
                },
                new[]
                {
                    "A strong password should be unique, long, and difficult to guess.",
                    "Avoid birthdays, names, sports teams, and other personal details.",
                    "A password manager can help you create and store unique passwords safely."
                },
                "As a reminder, strong password habits reduce the risk of account takeover."
            ),

            ["phishing"] = new TopicCard(
                "phishing",
                "Phishing and scams",
                new[] { "phishing", "scam", "fake email", "email scam", "fraud", "spoof" },
                new[]
                {
                    "Check the sender address carefully before clicking anything.",
                    "Do not trust urgent messages that pressure you to act immediately.",
                    "Verify suspicious requests through an official website or phone number."
                },
                new[]
                {
                    "Phishing works by pretending to be a trusted company or person.",
                    "Scammers often ask for passwords, OTPs, banking details, or personal data.",
                    "A healthy habit is to pause, verify, and then act only through official channels."
                },
                "If you stay alert, you can reduce the chances of falling for a scam."
            ),

            ["privacy"] = new TopicCard(
                "privacy",
                "Privacy protection",
                new[] { "privacy", "personal information", "data", "oversharing", "permission" },
                new[]
                {
                    "Share less personal information online when you can.",
                    "Review app permissions and remove access you do not need.",
                    "Check social media privacy settings regularly."
                },
                new[]
                {
                    "Privacy is about controlling what information others can see or use.",
                    "Be careful with ID numbers, location data, and account recovery answers.",
                    "Limit oversharing because small details can help attackers profile you."
                },
                "Protecting privacy helps reduce identity theft and unwanted exposure."
            ),

            ["link"] = new TopicCard(
                "link",
                "Suspicious links",
                new[] { "link", "url", "website", "web address", "click", "shortened" },
                new[]
                {
                    "Hover over a link before clicking it.",
                    "Look for strange spellings or unusual domains.",
                    "When unsure, type the official address into the browser yourself."
                },
                new[]
                {
                    "A suspicious link may send you to a fake site that looks legitimate.",
                    "Shortened links hide the real destination, so inspect them carefully.",
                    "HTTPS is useful, but it does not automatically make a site safe."
                },
                "Careful link checking is one of the simplest ways to avoid malware and scams."
            ),

            ["malware"] = new TopicCard(
                "malware",
                "Malware awareness",
                new[] { "malware", "virus", "trojan", "ransomware", "spyware" },
                new[]
                {
                    "Only install apps from trusted sources.",
                    "Keep your system and apps updated.",
                    "Back up important files regularly."
                },
                new[]
                {
                    "Malware is software designed to harm, spy on, or lock your device or files.",
                    "It can arrive through infected attachments, fake downloads, or unsafe websites.",
                    "Good patching, backups, and antivirus protection reduce the damage."
                },
                "Updating software regularly is one of the best defences against malware."
            ),

            ["social"] = new TopicCard(
                "social",
                "Social engineering",
                new[] { "social engineering", "impersonation", "pretend", "support", "manipulate" },
                new[]
                {
                    "Pause when someone pressures you to share private information.",
                    "Always verify a caller or sender through an official channel.",
                    "Never reveal OTPs, passwords, or PINs to unverified people."
                },
                new[]
                {
                    "Social engineering attacks people through trust, urgency, or fear.",
                    "Attackers may pretend to be bank staff, IT support, or a delivery company.",
                    "A calm verification step can stop many attacks before they start."
                },
                "Verifying identity before sharing information is a powerful security habit."
            )
        };
    }
}