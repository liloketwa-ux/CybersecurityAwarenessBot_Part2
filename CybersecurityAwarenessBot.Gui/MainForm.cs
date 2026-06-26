using System.Drawing;
using CybersecurityAwarenessBot.Gui.Models;
using CybersecurityAwarenessBot.Gui.Services;

namespace CybersecurityAwarenessBot.Gui;

/// <summary>
/// Main application window for the Cybersecurity Awareness Chatbot – Part 3.
/// Adds four tabbed sections: Chat, Task Assistant, Quiz, and Activity Log.
/// All Part 1 and Part 2 features are preserved in the Chat tab.
/// </summary>
public sealed class MainForm : Form
{
    // ── Services ──────────────────────────────────────────────────────────────
    private readonly CybersecurityChatbotEngine _engine  = new();
    private readonly DatabaseService            _db      = new();
    private readonly QuizEngine                 _quiz    = new();
    private readonly ActivityLogger             _logger  = new();

    // ── Reminder flow state ───────────────────────────────────────────────────
    private int?    _pendingReminderTaskId    = null;
    private string? _pendingReminderTaskTitle = null;

    // ── Palette ───────────────────────────────────────────────────────────────
    private static readonly Color CBg      = Color.FromArgb(16,  24,  38);
    private static readonly Color CPanel   = Color.FromArgb(23,  34,  53);
    private static readonly Color CCard    = Color.FromArgb(28,  40,  62);
    private static readonly Color CAccent  = Color.FromArgb(0,  150, 220);
    private static readonly Color CGreen   = Color.FromArgb(40, 180,  99);
    private static readonly Color CRed     = Color.FromArgb(210,  60,  60);
    private static readonly Color CGold    = Color.FromArgb(220, 180,  50);
    private static readonly Color CText    = Color.FromArgb(220, 230, 245);

    // ── Shared top / bottom panels ────────────────────────────────────────────
    private readonly Panel        _topPanel       = new();
    private readonly Label        _headerLabel    = new();
    private readonly RichTextBox  _asciiBox       = new();
    private readonly Panel        _bottomPanel    = new();
    private readonly Label        _statusLabel    = new();
    private readonly TextBox      _inputBox       = new();
    private readonly FlowLayoutPanel _topicBar    = new();
    private readonly Button       _sendBtn        = new();
    private readonly Button       _clearBtn       = new();
    private readonly Label        _dbStatusLabel  = new();

    // ── Tab control ───────────────────────────────────────────────────────────
    private readonly TabControl _tabs     = new();
    private readonly TabPage    _tabChat  = new("  💬  Chat  ");
    private readonly TabPage    _tabTasks = new("  ✅  Tasks  ");
    private readonly TabPage    _tabQuiz  = new("  🎮  Quiz  ");
    private readonly TabPage    _tabLog   = new("  📋  Activity Log  ");

    // ── Chat tab ──────────────────────────────────────────────────────────────
    private readonly RichTextBox _chatBox = new();

    // ── Tasks tab ────────────────────────────────────────────────────────────
    private readonly ListView   _taskList        = new();
    private readonly Panel      _taskFormPanel   = new();
    private readonly TextBox    _taskTitleInput  = new();
    private readonly TextBox    _taskDescInput   = new();
    private readonly CheckBox   _reminderChk     = new();
    private readonly DateTimePicker _reminderDtp = new();
    private readonly Button     _addTaskBtn      = new();
    private readonly Button     _completeBtn     = new();
    private readonly Button     _deleteBtn       = new();

    // ── Quiz tab ─────────────────────────────────────────────────────────────
    private readonly Panel  _quizWrap        = new();
    private readonly Label  _quizProgressLbl = new();
    private readonly Label  _quizScoreLbl    = new();
    private readonly Label  _quizQuestionLbl = new();
    private readonly Panel  _quizOptionsArea = new();
    private readonly Panel  _quizFeedbackBox = new();
    private readonly Label  _quizFeedbackLbl = new();
    private readonly Button _quizStartBtn    = new();
    private readonly Button _quizNextBtn     = new();

    // ── Activity Log tab ─────────────────────────────────────────────────────
    private readonly RichTextBox _logBox      = new();
    private readonly Button      _refreshBtn  = new();
    private readonly Button      _clearLogBtn = new();

    // =========================================================================
    //  Constructor
    // =========================================================================

    public MainForm()
    {
        _db.Initialise();
        InitUi();
        Shown += OnShown;
    }

    // =========================================================================
    //  UI Initialisation
    // =========================================================================

    private void InitUi()
    {
        // ── Form shell ───────────────────────────────────────────────────────
        Text            = "Cybersecurity Awareness Assistant – Part 3";
        StartPosition   = FormStartPosition.CenterScreen;
        MinimumSize     = new Size(1180, 820);
        BackColor       = CBg;
        ForeColor       = CText;
        Font            = new Font("Segoe UI", 10F);

        BuildTopPanel();
        BuildBottomPanel();
        BuildTabControl();

        // Chat tab must be on top so the chatbox fills the remaining space
        // correctly when dock order is resolved.
        Controls.Add(_chatBox);   // placeholder — will be moved into tab below

        BuildChatTab();
        BuildTasksTab();
        BuildQuizTab();
        BuildActivityLogTab();

        // ── Initial chat messages ─────────────────────────────────────────────
        AppendSystem("Hello! Welcome to the Cybersecurity Awareness Assistant – Part 3.",
                     Color.LightGreen);
        AppendSystem(
            "Type your name to get started, or ask about phishing, passwords, privacy, " +
            "links, malware, or social engineering.\n" +
            "New commands: 'start quiz' · 'add task [title]' · " +
            "'remind me to [task] in [N] days' · 'show tasks' · 'show activity log'",
            Color.Gainsboro);

        _logger.Log("Session started");
        CheckDueReminders();
    }

    // ── Top panel (header + ASCII art) ──────────────────────────────────────

    private void BuildTopPanel()
    {
        _topPanel.Dock      = DockStyle.Top;
        _topPanel.Height    = 190;
        _topPanel.Padding   = new Padding(14);
        _topPanel.BackColor = CPanel;
        Controls.Add(_topPanel);

        _headerLabel.Dock      = DockStyle.Top;
        _headerLabel.Height    = 34;
        _headerLabel.Text      = "Cybersecurity Awareness Assistant";
        _headerLabel.Font      = new Font("Segoe UI", 16F, FontStyle.Bold);
        _headerLabel.ForeColor = Color.White;
        _topPanel.Controls.Add(_headerLabel);

        _asciiBox.Dock        = DockStyle.Fill;
        _asciiBox.ReadOnly    = true;
        _asciiBox.BorderStyle = BorderStyle.None;
        _asciiBox.BackColor   = CPanel;
        _asciiBox.ForeColor   = Color.LightGreen;
        _asciiBox.Font        = new Font("Consolas", 9F);
        _asciiBox.ScrollBars  = RichTextBoxScrollBars.None;
        _asciiBox.WordWrap    = false;
        _asciiBox.TabStop     = false;
        _asciiBox.Text        = AsciiArtProvider.Load();
        _topPanel.Controls.Add(_asciiBox);
    }

    // ── Bottom panel (input + status) ────────────────────────────────────────

    private void BuildBottomPanel()
    {
        _bottomPanel.Dock      = DockStyle.Bottom;
        _bottomPanel.Height    = 155;
        _bottomPanel.Padding   = new Padding(14, 8, 14, 14);
        _bottomPanel.BackColor = CPanel;
        Controls.Add(_bottomPanel);

        // DB status label (far right of status row)
        _dbStatusLabel.AutoSize   = false;
        _dbStatusLabel.Dock       = DockStyle.Right;
        _dbStatusLabel.Width      = 230;
        _dbStatusLabel.Height     = 24;
        _dbStatusLabel.TextAlign  = ContentAlignment.MiddleRight;
        _dbStatusLabel.Font       = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        _dbStatusLabel.ForeColor  = Color.FromArgb(140, 160, 190);
        _dbStatusLabel.Text       = _db.IsUsingDatabase ? "📊 MySQL connected" : "⚠ In-memory mode (MySQL not found)";
        _bottomPanel.Controls.Add(_dbStatusLabel);

        _statusLabel.Dock      = DockStyle.Top;
        _statusLabel.Height    = 24;
        _statusLabel.Text      = "Type your name or ask a cybersecurity question.";
        _statusLabel.ForeColor = Color.Gainsboro;
        _statusLabel.Padding   = new Padding(2, 0, 2, 4);
        _bottomPanel.Controls.Add(_statusLabel);

        _inputBox.Dock        = DockStyle.Top;
        _inputBox.Height      = 34;
        _inputBox.Font        = new Font("Segoe UI", 11F);
        _inputBox.BorderStyle = BorderStyle.FixedSingle;
        _inputBox.BackColor   = Color.White;
        _inputBox.ForeColor   = Color.Black;
        _inputBox.KeyDown    += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SubmitMessage(); } };
        _bottomPanel.Controls.Add(_inputBox);

        // Topic shortcut buttons row
        _topicBar.Dock          = DockStyle.Top;
        _topicBar.Height        = 40;
        _topicBar.Padding       = new Padding(0, 6, 0, 0);
        _topicBar.BackColor     = CPanel;
        _topicBar.FlowDirection = FlowDirection.LeftToRight;
        _topicBar.WrapContents  = false;
        _bottomPanel.Controls.Add(_topicBar);

        foreach (string t in new[] { "Password", "Phishing", "Privacy", "Links" })
            AddTopicButton(t, t);
        AddTopicButton("Quiz 🎮", "start quiz");
        AddTopicButton("Tasks ✅", "show tasks");
        AddTopicButton("Log 📋",  "show activity log");

        // Send / Clear row
        var btnRow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding       = new Padding(0, 4, 0, 0),
            BackColor     = CPanel
        };
        _bottomPanel.Controls.Add(btnRow);

        StyleBtn(_sendBtn,  "Send",  CAccent,                120);
        StyleBtn(_clearBtn, "Clear", Color.FromArgb(70,85,108), 100);
        _sendBtn.Click  += (_, _) => SubmitMessage();
        _clearBtn.Click += (_, _) => ClearChat();
        btnRow.Controls.Add(_sendBtn);
        btnRow.Controls.Add(_clearBtn);

        AcceptButton = _sendBtn;
    }

    private void AddTopicButton(string label, string command)
    {
        var btn = new Button
        {
            Text      = label,
            Width     = label.Length > 8 ? 130 : 100,
            Height    = 26,
            Margin    = new Padding(0, 0, 6, 0),
            BackColor = CCard,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btn.FlatAppearance.BorderColor = Color.FromArgb(50, 70, 100);
        btn.Click += (_, _) =>
        {
            _inputBox.Text = command;
            _inputBox.SelectionStart = _inputBox.Text.Length;
            _inputBox.Focus();
        };
        _topicBar.Controls.Add(btn);
    }

    // ── Tab control ──────────────────────────────────────────────────────────

    private void BuildTabControl()
    {
        _tabs.Dock          = DockStyle.Fill;
        _tabs.Font          = new Font("Segoe UI", 10F);
        _tabs.BackColor     = CBg;
        _tabs.Padding       = new Point(12, 6);
        _tabs.DrawMode      = TabDrawMode.OwnerDrawFixed;
        _tabs.DrawItem     += OnDrawTab;
        _tabs.SelectedIndexChanged += OnTabChanged;

        _tabs.TabPages.Add(_tabChat);
        _tabs.TabPages.Add(_tabTasks);
        _tabs.TabPages.Add(_tabQuiz);
        _tabs.TabPages.Add(_tabLog);

        foreach (TabPage p in _tabs.TabPages)
        {
            p.BackColor = CBg;
            p.ForeColor = CText;
        }

        Controls.Add(_tabs);
    }

    private void OnDrawTab(object? sender, DrawItemEventArgs e)
    {
        bool selected = (e.State & DrawItemState.Selected) != 0;
        using var bg  = new SolidBrush(selected ? CBg : CPanel);
        using var fg  = new SolidBrush(selected ? Color.White : Color.FromArgb(160, 185, 215));
        e.Graphics.FillRectangle(bg, e.Bounds);
        StringFormat sf = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        e.Graphics.DrawString(_tabs.TabPages[e.Index].Text, e.Font!, fg, e.Bounds, sf);
        if (selected)
        {
            using var bar = new SolidBrush(CAccent);
            e.Graphics.FillRectangle(bar, e.Bounds.Left, e.Bounds.Bottom - 3, e.Bounds.Width, 3);
        }
    }

    private void OnTabChanged(object? sender, EventArgs e)
    {
        _statusLabel.Text = _tabs.SelectedTab == _tabChat  ? "Type a message or command below." :
                            _tabs.SelectedTab == _tabTasks ? "Select a task to manage it, or add a new one on the right." :
                            _tabs.SelectedTab == _tabQuiz  ? "Click 'Start Quiz' to begin the cybersecurity quiz." :
                                                             "Your session activity log. Type 'show activity log' to refresh.";
        if (_tabs.SelectedTab == _tabLog)
            RefreshLogBox();
    }

    // =========================================================================
    //  Chat Tab
    // =========================================================================

    private void BuildChatTab()
    {
        _chatBox.Dock        = DockStyle.Fill;
        _chatBox.ReadOnly    = true;
        _chatBox.BorderStyle = BorderStyle.None;
        _chatBox.BackColor   = Color.FromArgb(13, 18, 28);
        _chatBox.ForeColor   = CText;
        _chatBox.Font        = new Font("Segoe UI", 10F);
        _chatBox.HideSelection = false;
        _chatBox.DetectUrls  = false;
        _chatBox.Margin      = Padding.Empty;

        _tabChat.Controls.Add(_chatBox);
    }

    // =========================================================================
    //  Tasks Tab
    // =========================================================================

    private void BuildTasksTab()
    {
        // ── Right panel: add-task form ───────────────────────────────────────
        _taskFormPanel.Dock      = DockStyle.Right;
        _taskFormPanel.Width     = 320;
        _taskFormPanel.Padding   = new Padding(16);
        _taskFormPanel.BackColor = CPanel;

        var formTitle = MakeLabel("Add New Task", 13F, FontStyle.Bold, Color.White);
        formTitle.Dock   = DockStyle.Top;
        formTitle.Height = 30;

        var sep1 = MakeSeparator();

        var lblTitle = MakeLabel("Title:");
        lblTitle.Dock = DockStyle.Top;

        _taskTitleInput.Dock        = DockStyle.Top;
        _taskTitleInput.Height      = 30;
        _taskTitleInput.BackColor   = CCard;
        _taskTitleInput.ForeColor   = CText;
        _taskTitleInput.BorderStyle = BorderStyle.FixedSingle;
        _taskTitleInput.Font        = new Font("Segoe UI", 10F);
        _taskTitleInput.Margin      = new Padding(0, 0, 0, 6);

        var lblDesc = MakeLabel("Description (optional):");
        lblDesc.Dock = DockStyle.Top;

        _taskDescInput.Dock        = DockStyle.Top;
        _taskDescInput.Height      = 60;
        _taskDescInput.Multiline   = true;
        _taskDescInput.BackColor   = CCard;
        _taskDescInput.ForeColor   = CText;
        _taskDescInput.BorderStyle = BorderStyle.FixedSingle;
        _taskDescInput.Font        = new Font("Segoe UI", 10F);
        _taskDescInput.ScrollBars  = ScrollBars.Vertical;
        _taskDescInput.Margin      = new Padding(0, 0, 0, 6);

        _reminderChk.Dock      = DockStyle.Top;
        _reminderChk.Text      = "Set a reminder date";
        _reminderChk.Height    = 26;
        _reminderChk.ForeColor = CText;
        _reminderChk.BackColor = CPanel;
        _reminderChk.CheckedChanged += (_, _) => _reminderDtp.Enabled = _reminderChk.Checked;

        _reminderDtp.Dock      = DockStyle.Top;
        _reminderDtp.Height    = 30;
        _reminderDtp.Format    = DateTimePickerFormat.Short;
        _reminderDtp.MinDate   = DateTime.Today;
        _reminderDtp.Value     = DateTime.Today.AddDays(7);
        _reminderDtp.Enabled   = false;
        _reminderDtp.BackColor = CCard;
        _reminderDtp.ForeColor = Color.Black;

        StyleBtn(_addTaskBtn, "➕  Add Task", CGreen, 0);
        _addTaskBtn.Dock   = DockStyle.Top;
        _addTaskBtn.Height = 36;
        _addTaskBtn.Margin = new Padding(0, 10, 0, 0);
        _addTaskBtn.Click += OnAddTaskClicked;

        var sep2      = MakeSeparator();
        var lblManage = MakeLabel("Manage selected task:", 9.5F, FontStyle.Italic, Color.FromArgb(150, 170, 200));
        lblManage.Dock = DockStyle.Top;

        StyleBtn(_completeBtn, "✓  Mark Complete", CGreen, 0);
        _completeBtn.Dock   = DockStyle.Top;
        _completeBtn.Height = 34;
        _completeBtn.Margin = new Padding(0, 4, 0, 4);
        _completeBtn.Click += OnCompleteTaskClicked;

        StyleBtn(_deleteBtn, "🗑  Delete Task", CRed, 0);
        _deleteBtn.Dock   = DockStyle.Top;
        _deleteBtn.Height = 34;
        _deleteBtn.Click  += OnDeleteTaskClicked;

        // Controls are added bottom-up because DockStyle.Top stacks in reverse
        _taskFormPanel.Controls.AddRange(new Control[]
        {
            _deleteBtn, _completeBtn, lblManage, sep2,
            _addTaskBtn, _reminderDtp, _reminderChk,
            _taskDescInput, lblDesc, _taskTitleInput, lblTitle, sep1, formTitle
        });

        // ── Left: task list view ─────────────────────────────────────────────
        _taskList.Dock          = DockStyle.Fill;
        _taskList.View          = View.Details;
        _taskList.FullRowSelect = true;
        _taskList.GridLines     = true;
        _taskList.BackColor     = Color.FromArgb(13, 18, 28);
        _taskList.ForeColor     = CText;
        _taskList.BorderStyle   = BorderStyle.None;
        _taskList.Font          = new Font("Segoe UI", 10F);
        _taskList.HeaderStyle   = ColumnHeaderStyle.Nonclickable;

        _taskList.Columns.Add("#",           40);
        _taskList.Columns.Add("Title",       220);
        _taskList.Columns.Add("Description", 200);
        _taskList.Columns.Add("Reminder",    110);
        _taskList.Columns.Add("Status",      80);

        _tabTasks.Controls.Add(_taskList);
        _tabTasks.Controls.Add(_taskFormPanel);

        RefreshTaskList();
    }

    // ── Task list refresh ────────────────────────────────────────────────────

    private void RefreshTaskList()
    {
        _taskList.Items.Clear();
        var tasks = _db.GetAllTasks();
        int row   = 1;
        foreach (var t in tasks)
        {
            string desc = t.Description.Length > 30
                ? t.Description[..30] + "…"
                : t.Description;
            var item = new ListViewItem(row.ToString())
            {
                ForeColor = t.IsCompleted ? Color.Gray : CText
            };
            item.SubItems.Add(t.Title);
            item.SubItems.Add(desc);
            item.SubItems.Add(t.ReminderDisplay);
            item.SubItems.Add(t.StatusDisplay);
            item.Tag = t.Id;
            _taskList.Items.Add(item);
            row++;
        }
    }

    // ── Task button handlers ─────────────────────────────────────────────────

    private void OnAddTaskClicked(object? s, EventArgs e)
    {
        string title = _taskTitleInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Please enter a task title.", "Title required",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var task = new CyberTask
        {
            Title       = title,
            Description = _taskDescInput.Text.Trim(),
            ReminderDate = _reminderChk.Checked ? _reminderDtp.Value.Date : null
        };

        int id = _db.AddTask(task);
        string logDetail = _reminderChk.Checked
            ? $"'{title}' — reminder {_reminderDtp.Value:dd MMM yyyy}"
            : $"'{title}'";

        _logger.Log("Task added", logDetail);
        RefreshTaskList();

        _taskTitleInput.Clear();
        _taskDescInput.Clear();
        _reminderChk.Checked = false;

        // Confirm in chat tab
        AppendBot("Task Manager",
            $"Task added: '{title}'." +
            (_reminderChk.Checked ? $" Reminder set for {_reminderDtp.Value:dd MMM yyyy}." : string.Empty),
            CGreen);

        _tabs.SelectedTab = _tabTasks;
    }

    private void OnCompleteTaskClicked(object? s, EventArgs e)
    {
        if (_taskList.SelectedItems.Count == 0)
        {
            MessageBox.Show("Select a task first.", "No selection",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        int id    = (int)_taskList.SelectedItems[0].Tag!;
        string title = _taskList.SelectedItems[0].SubItems[1].Text;
        _db.CompleteTask(id);
        _logger.Log("Task completed", $"'{title}'");
        RefreshTaskList();
        AppendBot("Task Manager", $"'{title}' marked as complete. Well done!", CGreen);
    }

    private void OnDeleteTaskClicked(object? s, EventArgs e)
    {
        if (_taskList.SelectedItems.Count == 0)
        {
            MessageBox.Show("Select a task first.", "No selection",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string title = _taskList.SelectedItems[0].SubItems[1].Text;
        if (MessageBox.Show($"Delete task '{title}'?", "Confirm Delete",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

        int id = (int)_taskList.SelectedItems[0].Tag!;
        _db.DeleteTask(id);
        _logger.Log("Task deleted", $"'{title}'");
        RefreshTaskList();
        AppendBot("Task Manager", $"Task '{title}' has been deleted.", Color.Goldenrod);
    }

    // =========================================================================
    //  Quiz Tab
    // =========================================================================

    private void BuildQuizTab()
    {
        _quizWrap.Dock      = DockStyle.Fill;
        _quizWrap.BackColor = CBg;
        _quizWrap.Padding   = new Padding(40, 30, 40, 20);

        // Header row: progress + score
        var headerRow = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = CBg };

        _quizProgressLbl.AutoSize  = false;
        _quizProgressLbl.Dock      = DockStyle.Left;
        _quizProgressLbl.Width     = 260;
        _quizProgressLbl.Text      = "Cybersecurity Quiz";
        _quizProgressLbl.Font      = new Font("Segoe UI", 13F, FontStyle.Bold);
        _quizProgressLbl.ForeColor = Color.White;
        _quizProgressLbl.TextAlign = ContentAlignment.MiddleLeft;

        _quizScoreLbl.AutoSize  = false;
        _quizScoreLbl.Dock      = DockStyle.Right;
        _quizScoreLbl.Width     = 200;
        _quizScoreLbl.Text      = string.Empty;
        _quizScoreLbl.Font      = new Font("Segoe UI", 12F, FontStyle.Bold);
        _quizScoreLbl.ForeColor = CGold;
        _quizScoreLbl.TextAlign = ContentAlignment.MiddleRight;

        headerRow.Controls.Add(_quizProgressLbl);
        headerRow.Controls.Add(_quizScoreLbl);

        // Question label
        _quizQuestionLbl.Dock      = DockStyle.Top;
        _quizQuestionLbl.Height    = 90;
        _quizQuestionLbl.Text      = "Press 'Start Quiz' to begin. Answer 12 questions covering\n" +
                                      "phishing, passwords, privacy, malware, and more.";
        _quizQuestionLbl.Font      = new Font("Segoe UI", 11.5F);
        _quizQuestionLbl.ForeColor = CText;
        _quizQuestionLbl.Padding   = new Padding(0, 14, 0, 0);

        // Options area (rebuilt per-question)
        _quizOptionsArea.Dock      = DockStyle.Top;
        _quizOptionsArea.Height    = 0;
        _quizOptionsArea.BackColor = CBg;

        // Feedback box
        _quizFeedbackBox.Dock      = DockStyle.Top;
        _quizFeedbackBox.Height    = 0;
        _quizFeedbackBox.Padding   = new Padding(0, 8, 0, 0);
        _quizFeedbackBox.BackColor = CBg;

        _quizFeedbackLbl.Dock      = DockStyle.Fill;
        _quizFeedbackLbl.Font      = new Font("Segoe UI", 10F, FontStyle.Italic);
        _quizFeedbackLbl.ForeColor = Color.Gainsboro;
        _quizFeedbackBox.Controls.Add(_quizFeedbackLbl);

        // Control buttons
        var ctrlRow = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = CBg, Padding = new Padding(0, 12, 0, 0) };

        StyleBtn(_quizStartBtn, "▶  Start Quiz", CAccent, 160);
        _quizStartBtn.Height = 40;
        _quizStartBtn.Click += (_, _) => StartQuiz();
        ctrlRow.Controls.Add(_quizStartBtn);

        StyleBtn(_quizNextBtn, "Next Question  →", CGreen, 180);
        _quizNextBtn.Height  = 40;
        _quizNextBtn.Left    = 0;
        _quizNextBtn.Visible = false;
        _quizNextBtn.Click  += (_, _) => QuizNext();
        ctrlRow.Controls.Add(_quizNextBtn);

        _quizWrap.Controls.Add(ctrlRow);
        _quizWrap.Controls.Add(_quizFeedbackBox);
        _quizWrap.Controls.Add(_quizOptionsArea);
        _quizWrap.Controls.Add(_quizQuestionLbl);
        _quizWrap.Controls.Add(headerRow);

        _tabQuiz.Controls.Add(_quizWrap);
    }

    // ── Quiz flow ─────────────────────────────────────────────────────────────

    private void StartQuiz()
    {
        _quiz.Start();
        _logger.Log("Quiz started");
        _quizStartBtn.Visible = false;
        _quizNextBtn.Visible  = false;
        ShowCurrentQuestion();
        _tabs.SelectedTab = _tabQuiz;
    }

    private void ShowCurrentQuestion()
    {
        QuizQuestion? q = _quiz.Current;
        if (q == null) return;

        _quizProgressLbl.Text = $"Question  {_quiz.CurrentNumber}  of  {_quiz.Total}";
        _quizScoreLbl.Text    = $"Score:  {_quiz.Score} / {_quiz.CurrentNumber - 1}";
        _quizQuestionLbl.Text = q.Text;

        // Rebuild options panel
        _quizOptionsArea.Controls.Clear();
        _quizOptionsArea.Height = q.IsTrueFalse ? 56 : 120;

        var optFlow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = true,
            BackColor     = CBg
        };

        foreach (string opt in q.Options)
        {
            string optCopy = opt;
            var btn = new Button
            {
                Text      = optCopy,
                Width     = q.IsTrueFalse ? 160 : 440,
                Height    = 44,
                Margin    = new Padding(0, 0, 12, 10),
                BackColor = CCard,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(10, 0, 0, 0)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(50, 80, 120);
            btn.Click += (_, _) => OnQuizOptionClicked(optCopy);
            optFlow.Controls.Add(btn);
        }

        _quizOptionsArea.Controls.Add(optFlow);

        // Hide feedback and next button
        _quizFeedbackBox.Height = 0;
        _quizFeedbackLbl.Text   = string.Empty;
        _quizNextBtn.Visible    = false;
    }

    private void OnQuizOptionClicked(string option)
    {
        // Extract the bare answer letter/word
        string answer = option.Split(')')[0].Trim();

        var result = _quiz.SubmitAnswer(answer);
        if (result == null) return;

        bool correct = result.Value.IsCorrect;
        string expl  = result.Value.Explanation;

        // Colour the buttons
        foreach (Control c in _quizOptionsArea.Controls[0].Controls)
        {
            if (c is not Button btn) continue;
            bool thisIsCorrect = btn.Text.StartsWith(_quiz.Current!.CorrectAnswer,
                                     StringComparison.OrdinalIgnoreCase);
            bool thisWasClicked = btn.Text == option;
            btn.BackColor = thisIsCorrect  ? Color.FromArgb(30, 140, 70) :
                            thisWasClicked ? Color.FromArgb(160, 40, 40) :
                            CCard;
            btn.Enabled   = false;
        }

        // Show feedback
        _quizFeedbackBox.Height = 80;
        _quizFeedbackLbl.Text   = correct
            ? $"✅  Correct!\n{expl}"
            : $"❌  Incorrect. The correct answer is {_quiz.Current!.CorrectAnswer}.\n{expl}";
        _quizFeedbackLbl.ForeColor = correct ? Color.FromArgb(100, 210, 130) : Color.FromArgb(220, 120, 120);

        _logger.Log(correct ? "Quiz answer correct" : "Quiz answer incorrect",
                    $"Q{_quiz.CurrentNumber}: {option}");

        // Show appropriate next control
        if (_quiz.IsFinished)
            ShowQuizResults();
        else
            _quizNextBtn.Visible = true;
    }

    private void QuizNext()
    {
        bool more = _quiz.MoveNext();
        if (more)
        {
            ShowCurrentQuestion();
        }
        else
        {
            ShowQuizResults();
        }
    }

    private void ShowQuizResults()
    {
        string feedback = _quiz.GetFinalFeedback();
        _quizProgressLbl.Text = "Quiz Complete!";
        _quizScoreLbl.Text    = $"Final: {_quiz.Score} / {_quiz.Total}";
        _quizQuestionLbl.Text = feedback;

        _quizOptionsArea.Controls.Clear();
        _quizOptionsArea.Height  = 0;
        _quizFeedbackBox.Height  = 0;
        _quizNextBtn.Visible     = false;

        StyleBtn(_quizStartBtn, "▶  Play Again", CAccent, 160);
        _quizStartBtn.Visible = true;

        _logger.Log("Quiz completed", $"Score: {_quiz.Score}/{_quiz.Total}");
        _quiz.End();

        AppendBot("Quiz Result", feedback, CGold);
        _tabs.SelectedTab = _tabChat;
    }

    // =========================================================================
    //  Activity Log Tab
    // =========================================================================

    private void BuildActivityLogTab()
    {
        var topBar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 52,
            Padding   = new Padding(14, 10, 14, 0),
            BackColor = CPanel
        };

        StyleBtn(_refreshBtn, "⟳  Refresh", CAccent, 120);
        _refreshBtn.Height = 32;
        _refreshBtn.Click += (_, _) => RefreshLogBox();

        StyleBtn(_clearLogBtn, "🗑  Clear Log", Color.FromArgb(120, 50, 50), 120);
        _clearLogBtn.Height = 32;
        _clearLogBtn.Left  = 132;
        _clearLogBtn.Click += (_, _) =>
        {
            _logger.Clear();
            _logger.Log("Activity log cleared");
            RefreshLogBox();
        };

        topBar.Controls.Add(_clearLogBtn);
        topBar.Controls.Add(_refreshBtn);

        _logBox.Dock        = DockStyle.Fill;
        _logBox.ReadOnly    = true;
        _logBox.BorderStyle = BorderStyle.None;
        _logBox.BackColor   = Color.FromArgb(13, 18, 28);
        _logBox.ForeColor   = CText;
        _logBox.Font        = new Font("Consolas", 9.5F);
        _logBox.ScrollBars  = RichTextBoxScrollBars.Vertical;

        _tabLog.Controls.Add(_logBox);
        _tabLog.Controls.Add(topBar);
    }

    private void RefreshLogBox()
    {
        _logBox.Clear();
        var entries = _logger.GetRecent(10);
        if (entries.Count == 0)
        {
            AppendToRtb(_logBox, "  No activity recorded yet.", Color.Gray);
            return;
        }

        int total = _logger.Count;
        if (total > 10)
            AppendToRtb(_logBox, $"  Showing last 10 of {total} actions.\n", Color.Gray);

        foreach (var e in entries)
        {
            AppendToRtb(_logBox, $"[{e.Timestamp:HH:mm:ss}]", Color.FromArgb(100,140,180));
            AppendToRtb(_logBox, $"  {e.Action}", CText);
            if (!string.IsNullOrWhiteSpace(e.Detail))
                AppendToRtb(_logBox, $"  →  {e.Detail}", Color.FromArgb(150,180,220));
            AppendToRtb(_logBox, "\n", CText);
        }
    }

    // =========================================================================
    //  Message Submission & NLP Routing
    // =========================================================================

    private void SubmitMessage()
    {
        string input = _inputBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            AppendSystem("Please type a message before sending.", Color.IndianRed);
            return;
        }
        _inputBox.Clear();
        _tabs.SelectedTab = _tabChat;

        AppendUser(input);

        // ── Pending reminder flow ────────────────────────────────────────────
        if (_pendingReminderTaskId.HasValue)
        {
            HandlePendingReminder(input);
            return;
        }

        // ── NLP intent detection ─────────────────────────────────────────────
        NlpResult nlp = NlpProcessor.Analyse(input);

        switch (nlp.Intent)
        {
            case UserIntent.StartQuiz:
                HandleStartQuiz();
                return;

            case UserIntent.ShowActivityLog:
                HandleShowLog();
                return;

            case UserIntent.ViewTasks:
                HandleViewTasks();
                return;

            case UserIntent.AddTask:
                HandleAddTask(nlp);
                return;

            case UserIntent.CompleteTask:
                HandleCompleteTask(nlp.TargetKeyword);
                return;

            case UserIntent.DeleteTask:
                HandleDeleteTask(nlp.TargetKeyword);
                return;
        }

        // ── Fall through to cybersecurity chatbot engine (Parts 1 & 2) ───────
        BotResponse response = _engine.Process(input);
        AppendBot(response.Title, response.Message, response.AccentColor);
        _logger.Log("Chat response", response.Title);

        _statusLabel.Text = response.ShowFollowUpHint
            ? "You can say 'tell me more', 'another tip', or ask about a different topic."
            : "Type another question or keep the conversation going.";
    }

    // ── Reminder confirmation handler ─────────────────────────────────────────

    private void HandlePendingReminder(string input)
    {
        if (NlpProcessor.IsReminderRejection(input))
        {
            _logger.Log("Reminder skipped", _pendingReminderTaskTitle);
            AppendBot("Task Manager", "Okay — no reminder set. Your task has been saved.", Color.Gainsboro);
            _pendingReminderTaskId    = null;
            _pendingReminderTaskTitle = null;
        }
        else if (NlpProcessor.IsReminderConfirmation(input, out int days))
        {
            DateTime remDate = DateTime.Now.AddDays(days);
            _db.SetReminder(_pendingReminderTaskId!.Value, remDate);
            _logger.Log("Reminder set", $"'{_pendingReminderTaskTitle}' — {remDate:dd MMM yyyy}");
            AppendBot("Task Manager",
                $"Got it! Reminder set for '{_pendingReminderTaskTitle}' on {remDate:dd MMM yyyy}. ✅",
                CGreen);
            RefreshTaskList();
            _pendingReminderTaskId    = null;
            _pendingReminderTaskTitle = null;
        }
        else
        {
            AppendBot("Task Manager",
                "I am not sure about that. Say 'yes, in 3 days', 'tomorrow', 'in a week', or 'no'.",
                Color.Goldenrod);
        }
    }

    // ── NLP command handlers ──────────────────────────────────────────────────

    private void HandleStartQuiz()
    {
        _logger.Log("Quiz requested via NLP");
        AppendBot("Quiz 🎮",
            "Starting the cybersecurity quiz! Switching to the Quiz tab now. Good luck! 🎯",
            CAccent);
        StartQuiz();
    }

    private void HandleShowLog()
    {
        _logger.Log("Activity log viewed");
        var entries = _logger.GetRecent(10);

        if (entries.Count == 0)
        {
            AppendBot("Activity Log", "No activity recorded yet.", Color.Gray);
            return;
        }

        string lines = string.Join("\n",
            entries.Select((e, i) =>
                $"{i + 1}. [{e.Timestamp:HH:mm:ss}] {e.Action}" +
                (string.IsNullOrWhiteSpace(e.Detail) ? string.Empty : $"  →  {e.Detail}")));

        int total = _logger.Count;
        string header = total > 10 ? $"Showing last 10 of {total} actions:\n" : "Recent actions:\n";
        AppendBot("Activity Log", header + lines, Color.FromArgb(130, 180, 230));
    }

    private void HandleViewTasks()
    {
        var tasks = _db.GetAllTasks();
        _logger.Log("Tasks viewed");

        if (tasks.Count == 0)
        {
            AppendBot("Task Manager",
                "You have no tasks yet. Say 'add task [title]' or use the Tasks tab to create one.",
                Color.Gainsboro);
            return;
        }

        string lines = string.Join("\n", tasks.Select((t, i) =>
        {
            string reminder = t.ReminderDate.HasValue ? $" | Reminder: {t.ReminderDate.Value:dd MMM}" : string.Empty;
            string status   = t.IsCompleted ? " ✓" : string.Empty;
            return $"{i + 1}. {t.Title}{reminder}{status}";
        }));

        AppendBot("Your Tasks", $"{tasks.Count} task(s):\n{lines}", Color.FromArgb(130, 200, 160));
    }

    private void HandleAddTask(NlpResult nlp)
    {
        string title = string.IsNullOrWhiteSpace(nlp.TaskTitle)
            ? "Unnamed cybersecurity task"
            : nlp.TaskTitle;

        // Generate a smart description based on cybersecurity context
        string desc = GenerateTaskDescription(title);

        var task = new CyberTask { Title = title, Description = desc, ReminderDate = nlp.ReminderDate };
        int id   = _db.AddTask(task);

        _logger.Log("Task added via NLP", $"'{title}'");
        RefreshTaskList();

        if (nlp.ReminderDate.HasValue)
        {
            AppendBot("Task Manager",
                $"Task added: '{title}'\nDescription: {desc}\n" +
                $"Reminder set for {nlp.ReminderDate.Value:dd MMM yyyy}. ✅",
                CGreen);
            _logger.Log("Reminder set", $"'{title}' — {nlp.ReminderDate.Value:dd MMM yyyy}");
        }
        else
        {
            AppendBot("Task Manager",
                $"Task added: '{title}'\nDescription: {desc}\n\nWould you like a reminder? " +
                "Say 'yes, in 3 days', 'tomorrow', 'next week', or 'no'.",
                CGreen);
            _pendingReminderTaskId    = id;
            _pendingReminderTaskTitle = title;
        }
    }

    private void HandleCompleteTask(string? keyword)
    {
        var tasks = _db.GetAllTasks();
        CyberTask? match = string.IsNullOrWhiteSpace(keyword)
            ? null
            : tasks.FirstOrDefault(t =>
                t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) && !t.IsCompleted);

        if (match == null)
        {
            AppendBot("Task Manager",
                "I could not find a matching pending task. Use the Tasks tab to view and manage your tasks.",
                Color.Goldenrod);
            return;
        }

        _db.CompleteTask(match.Id);
        _logger.Log("Task completed via NLP", $"'{match.Title}'");
        RefreshTaskList();
        AppendBot("Task Manager", $"'{match.Title}' marked as complete. Well done! ✅", CGreen);
    }

    private void HandleDeleteTask(string? keyword)
    {
        var tasks = _db.GetAllTasks();
        CyberTask? match = string.IsNullOrWhiteSpace(keyword)
            ? null
            : tasks.FirstOrDefault(t => t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (match == null)
        {
            AppendBot("Task Manager",
                "I could not find a task matching that description. Use the Tasks tab to view all tasks.",
                Color.Goldenrod);
            return;
        }

        _db.DeleteTask(match.Id);
        _logger.Log("Task deleted via NLP", $"'{match.Title}'");
        RefreshTaskList();
        AppendBot("Task Manager", $"Task '{match.Title}' has been deleted.", Color.Goldenrod);
    }

    // ── Smart task description generator ─────────────────────────────────────

    private static string GenerateTaskDescription(string title)
    {
        string l = title.ToLowerInvariant();
        if (l.Contains("2fa") || l.Contains("two-factor") || l.Contains("two factor") || l.Contains("authenticat"))
            return "Enable two-factor authentication to add an extra layer of security to your accounts.";
        if (l.Contains("password"))
            return "Update your password to a strong, unique passphrase and store it in a password manager.";
        if (l.Contains("privacy") || l.Contains("settings"))
            return "Review your account privacy settings to ensure only the right people can see your information.";
        if (l.Contains("backup"))
            return "Back up important files to an offline or cloud location to protect against data loss and ransomware.";
        if (l.Contains("update") || l.Contains("patch"))
            return "Apply pending software updates to patch known security vulnerabilities.";
        if (l.Contains("antivirus") || l.Contains("malware"))
            return "Run a malware scan and ensure your antivirus software is active and up to date.";
        if (l.Contains("vpn"))
            return "Set up a VPN for safer browsing on public or untrusted networks.";
        return $"Complete the cybersecurity task: '{title}' to improve your digital security posture.";
    }

    // =========================================================================
    //  Startup helpers
    // =========================================================================

    private void OnShown(object? sender, EventArgs e)
    {
        string audioPath = Path.Combine(AppContext.BaseDirectory, "Assets", "welcome.wav");
        AudioPlayer.TryPlayGreeting(audioPath);
        _inputBox.Focus();
    }

    private void CheckDueReminders()
    {
        try
        {
            var due = _db.GetAllTasks()
                         .Where(t => !t.IsCompleted && t.ReminderDate.HasValue && t.ReminderDate <= DateTime.Now)
                         .ToList();
            if (due.Count == 0) return;

            AppendSystem($"⏰  You have {due.Count} task(s) with reminders due:", Color.Orange);
            foreach (var t in due)
                AppendSystem($"   • {t.Title}", Color.Orange);
        }
        catch { /* non-critical */ }
    }

    // =========================================================================
    //  Chat rendering helpers
    // =========================================================================

    private void AppendUser(string message)
        => AppendMsg("You", message, Color.LightSkyBlue);

    private void AppendBot(string title, string message, Color colour)
        => AppendMsg(title, message, colour);

    private void AppendSystem(string message, Color colour)
        => AppendMsg("System", message, colour);

    private void AppendMsg(string speaker, string message, Color colour)
    {
        _chatBox.SuspendLayout();
        _chatBox.SelectionStart  = _chatBox.TextLength;
        _chatBox.SelectionLength = 0;
        _chatBox.SelectionColor  = colour;
        _chatBox.SelectionFont   = new Font(_chatBox.Font, FontStyle.Bold);
        _chatBox.AppendText($"{speaker}: ");
        _chatBox.SelectionFont   = new Font(_chatBox.Font, FontStyle.Regular);
        _chatBox.SelectionColor  = Color.FromArgb(210, 225, 245);
        _chatBox.AppendText($"{message}{Environment.NewLine}{Environment.NewLine}");
        _chatBox.SelectionColor  = _chatBox.ForeColor;
        _chatBox.ResumeLayout();
        _chatBox.ScrollToCaret();
    }

    private void ClearChat()
    {
        _chatBox.Clear();
        AppendSystem("Conversation cleared. You can continue chatting.", Color.Gainsboro);
    }

    // =========================================================================
    //  UI factory helpers
    // =========================================================================

    private static void StyleBtn(Button btn, string text, Color back, int width)
    {
        btn.Text      = text;
        if (width > 0) btn.Width = width;
        btn.BackColor = back;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize  = 0;
        btn.Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btn.Cursor    = Cursors.Hand;
    }

    private static Label MakeLabel(string text, float size = 9.5F,
        FontStyle style = FontStyle.Regular, Color? colour = null)
    {
        return new Label
        {
            Text      = text,
            AutoSize  = false,
            Height    = 22,
            ForeColor = colour ?? Color.FromArgb(190, 205, 225),
            Font      = new Font("Segoe UI", size, style),
            Dock      = DockStyle.Top
        };
    }

    private static Panel MakeSeparator()
    {
        return new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 10,
            BackColor = Color.FromArgb(40, 56, 80)
        };
    }

    private static void AppendToRtb(RichTextBox rtb, string text, Color colour)
    {
        rtb.SelectionStart  = rtb.TextLength;
        rtb.SelectionLength = 0;
        rtb.SelectionColor  = colour;
        rtb.AppendText(text);
        rtb.SelectionColor = rtb.ForeColor;
    }
}
