using System.Drawing;
using CybersecurityAwarenessBot.Gui.Models;
using CybersecurityAwarenessBot.Gui.Services;

namespace CybersecurityAwarenessBot.Gui;

public sealed class MainForm : Form
{
    private readonly CybersecurityChatbotEngine _engine = new();
    private readonly ResponseResolver _resolver;

    private readonly RichTextBox _asciiBox = new();
    private readonly RichTextBox _chatBox = new();
    private readonly TextBox _inputBox = new();
    private readonly Button _sendButton = new();
    private readonly Button _clearButton = new();
    private readonly Label _statusLabel = new();
    private readonly Label _headerLabel = new();
    private readonly Panel _topPanel = new();
    private readonly Panel _bottomPanel = new();
    private readonly FlowLayoutPanel _topicButtons = new();

    public MainForm()
    {
        _resolver = _engine.Process;
        InitializeUi();
        Shown += OnShown;
    }

    private void InitializeUi()
    {
        Text = "Cybersecurity Awareness Assistant";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 780);
        BackColor = Color.FromArgb(16, 24, 38);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        _topPanel.Dock = DockStyle.Top;
        _topPanel.Height = 210;
        _topPanel.Padding = new Padding(16);
        _topPanel.BackColor = Color.FromArgb(23, 34, 53);
        Controls.Add(_topPanel);

        _headerLabel.Dock = DockStyle.Top;
        _headerLabel.Height = 34;
        _headerLabel.Text = "Cybersecurity Awareness Assistant";
        _headerLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        _headerLabel.ForeColor = Color.White;
        _topPanel.Controls.Add(_headerLabel);

        _asciiBox.Dock = DockStyle.Fill;
        _asciiBox.ReadOnly = true;
        _asciiBox.BorderStyle = BorderStyle.None;
        _asciiBox.BackColor = Color.FromArgb(23, 34, 53);
        _asciiBox.ForeColor = Color.LightGreen;
        _asciiBox.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
        _asciiBox.ScrollBars = RichTextBoxScrollBars.None;
        _asciiBox.WordWrap = false;
        _asciiBox.TabStop = false;
        _topPanel.Controls.Add(_asciiBox);

        _chatBox.Dock = DockStyle.Fill;
        _chatBox.ReadOnly = true;
        _chatBox.BorderStyle = BorderStyle.None;
        _chatBox.BackColor = Color.FromArgb(13, 18, 28);
        _chatBox.ForeColor = Color.White;
        _chatBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        _chatBox.HideSelection = false;
        _chatBox.DetectUrls = false;
        _chatBox.Margin = new Padding(0);
        Controls.Add(_chatBox);

        _bottomPanel.Dock = DockStyle.Bottom;
        _bottomPanel.Height = 150;
        _bottomPanel.Padding = new Padding(16, 10, 16, 16);
        _bottomPanel.BackColor = Color.FromArgb(23, 34, 53);
        Controls.Add(_bottomPanel);

        _statusLabel.Dock = DockStyle.Top;
        _statusLabel.Height = 28;
        _statusLabel.Text = "Type your name or ask a cybersecurity question.";
        _statusLabel.ForeColor = Color.Gainsboro;
        _statusLabel.Padding = new Padding(2, 0, 2, 6);
        _bottomPanel.Controls.Add(_statusLabel);

        _inputBox.Dock = DockStyle.Top;
        _inputBox.Height = 34;
        _inputBox.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        _inputBox.BorderStyle = BorderStyle.FixedSingle;
        _inputBox.BackColor = Color.White;
        _inputBox.ForeColor = Color.Black;
        _inputBox.KeyDown += InputBoxOnKeyDown;
        _bottomPanel.Controls.Add(_inputBox);

        _topicButtons.Dock = DockStyle.Top;
        _topicButtons.Height = 42;
        _topicButtons.FlowDirection = FlowDirection.LeftToRight;
        _topicButtons.WrapContents = false;
        _topicButtons.Padding = new Padding(0, 8, 0, 0);
        _topicButtons.BackColor = Color.FromArgb(23, 34, 53);
        _bottomPanel.Controls.Add(_topicButtons);

        AddTopicButton("Password");
        AddTopicButton("Phishing");
        AddTopicButton("Privacy");
        AddTopicButton("Links");

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0),
            BackColor = Color.FromArgb(23, 34, 53)
        };
        _bottomPanel.Controls.Add(buttonRow);

        _sendButton.Text = "Send";
        _sendButton.Width = 120;
        _sendButton.Height = 34;
        _sendButton.BackColor = Color.FromArgb(0, 120, 215);
        _sendButton.ForeColor = Color.White;
        _sendButton.FlatStyle = FlatStyle.Flat;
        _sendButton.Click += (_, _) => SubmitMessage();
        buttonRow.Controls.Add(_sendButton);

        _clearButton.Text = "Clear";
        _clearButton.Width = 120;
        _clearButton.Height = 34;
        _clearButton.BackColor = Color.FromArgb(80, 92, 110);
        _clearButton.ForeColor = Color.White;
        _clearButton.FlatStyle = FlatStyle.Flat;
        _clearButton.Click += (_, _) => ClearConversation();
        buttonRow.Controls.Add(_clearButton);

        AcceptButton = _sendButton;

        AppendSystemMessage(
            "Hello! Welcome to the Cybersecurity Awareness Assistant.",
            Color.LightGreen);

        AppendSystemMessage(
            "Please type your name or say something like 'my name is Lilo'. I can help with phishing, password safety, privacy, suspicious links, malware, and social engineering.",
            Color.Gainsboro);
    }

    private void OnShown(object? sender, EventArgs e)
    {
        string audioPath = Path.Combine(AppContext.BaseDirectory, "Assets", "welcome.wav");
        AudioPlayer.TryPlayGreeting(audioPath);
        _inputBox.Focus();
    }

    private void AddTopicButton(string topic)
    {
        Button button = new()
        {
            Text = topic,
            Width = 120,
            Height = 28,
            Margin = new Padding(0, 0, 8, 0),
            BackColor = Color.FromArgb(38, 54, 82),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        button.Click += (_, _) =>
        {
            _inputBox.Text = topic;
            _inputBox.SelectionStart = _inputBox.Text.Length;
            _inputBox.Focus();
        };

        _topicButtons.Controls.Add(button);
    }

    private void InputBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            SubmitMessage();
        }
    }

    private void SubmitMessage()
    {
        string input = _inputBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            AppendSystemMessage("Please type a message before sending.", Color.IndianRed);
            return;
        }

        _inputBox.Clear();

        AppendChatMessage("You", input, Color.LightSkyBlue);
        BotResponse response = _resolver(input);
        AppendBotResponse(response);

        _statusLabel.Text = response.ShowFollowUpHint
            ? "You can say 'tell me more', 'another tip', or ask about a different topic."
            : "Type another question or keep the conversation going.";
    }

    private void ClearConversation()
    {
        _chatBox.Clear();
        AppendSystemMessage("Conversation cleared. You can continue chatting.", Color.Gainsboro);
    }

    private void AppendBotResponse(BotResponse response)
    {
        AppendChatMessage(response.Title, response.Message, response.AccentColor);
    }

    private void AppendSystemMessage(string message, Color colour)
    {
        AppendChatMessage("System", message, colour);
    }

    private void AppendChatMessage(string speaker, string message, Color colour)
    {
        _chatBox.SelectionStart = _chatBox.TextLength;
        _chatBox.SelectionLength = 0;
        _chatBox.SelectionColor = colour;
        _chatBox.SelectionFont = new Font(_chatBox.Font, FontStyle.Bold);
        _chatBox.AppendText($"{speaker}: ");
        _chatBox.SelectionFont = new Font(_chatBox.Font, FontStyle.Regular);
        _chatBox.AppendText($"{message}{Environment.NewLine}{Environment.NewLine}");
        _chatBox.SelectionColor = _chatBox.ForeColor;
        _chatBox.ScrollToCaret();
    }
}