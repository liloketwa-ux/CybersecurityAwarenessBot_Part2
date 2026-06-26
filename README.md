# 🔐 Cybersecurity Awareness Chatbot — Part 3

> **IIE Assessment — Cybersecurity Awareness Chatbot (POE Part 3/3)**  
> Built with **C# · Windows Forms · .NET 8 · MySQL**

---

## 📽️ Demo Video

[![Watch the demo](https://img.youtube.com/vi/uMUCzcOq9rc/maxresdefault.jpg)](https://youtube.com/shorts/uMUCzcOq9rc?si=QEfnFUjqY97cl4z0)

> *Click the thumbnail above to watch the project demo on YouTube.*

---

## 📋 Overview

This is the final part of the Cybersecurity Awareness Chatbot POE. It builds directly on Parts 1 and 2 by adding four major new features to the existing Windows Forms GUI:

| Task | Feature | Description |
|------|---------|-------------|
| **Task 1** | Task Assistant | Add, view, complete, and delete cybersecurity tasks with optional reminders, stored in MySQL |
| **Task 2** | Quiz Mini-Game | 12-question cybersecurity quiz (Multiple-Choice + True/False) with scoring and feedback |
| **Task 3** | NLP Simulation | Keyword and regex-based intent detection — understands varied phrasings naturally |
| **Task 4** | Activity Log | Session-based log of all chatbot actions, viewable on demand |

All **Part 1 and Part 2 features are fully preserved** — dynamic responses, keyword recognition, sentiment detection, and conversation memory all continue to work in the Chat tab.

---

## 🖥️ GUI Layout

The app uses a **4-tab Windows Forms interface**:

```
┌──────────────────────────────────────────────────┐
│  Cybersecurity Awareness Assistant                │
│  [ASCII Art Banner]                               │
├──────────────────────────────────────────────────┤
│ 💬 Chat  │ ✅ Tasks  │ 🎮 Quiz  │ 📋 Activity Log │
├──────────────────────────────────────────────────┤
│                  [Tab Content]                    │
├──────────────────────────────────────────────────┤
│  [Password] [Phishing] [Privacy] [Quiz] [Tasks]  │
│  > Type a message or command...          [Send]  │
└──────────────────────────────────────────────────┘
```

---

## ⚙️ Setup & Running

### 1. Database Setup (MySQL)

Run the provided SQL script **once** to create the database and table:

```bash
mysql -u root -p < setup_database.sql
```

> If MySQL is not running, the app **automatically falls back to in-memory storage** — it still works, tasks just won't persist between sessions.

### 2. Configure Connection String

Open `Services/DatabaseService.cs` and update line 19 if your MySQL password is not blank:

```csharp
public const string DefaultConnectionString =
    "Server=localhost;Database=cybersecurity_bot;Uid=root;Pwd=YOUR_PASSWORD;";
```

### 3. Build & Run

```bash
dotnet run --project CybersecurityAwarenessBot.Gui
```

Or open `CybersecurityAwarenessBot_Part3.sln` in **Visual Studio 2022** and press `F5`.

---

## 💬 Chat Commands (NLP)

The chatbot understands natural language — these all work even when phrased differently:

| What you type | What happens |
|---|---|
| `add task - Enable two-factor authentication` | Adds a task, asks if you want a reminder |
| `remind me to update my password in 3 days` | Adds task with reminder date set |
| `show tasks` / `my tasks` / `list tasks` | Lists all your tasks in chat |
| `complete task [keyword]` | Marks matching task as done |
| `delete task [keyword]` | Deletes the matching task |
| `start quiz` / `quiz me` / `test my knowledge` | Switches to Quiz tab and starts the quiz |
| `show activity log` / `what have you done for me` | Shows last 10 logged actions |

---

## 🎮 Quiz Topics

The 12-question quiz (shuffled each session) covers:

- 🎣 Phishing & email scams  
- 🔑 Password strength & reuse  
- 🔐 Two-factor authentication (2FA)  
- 🔒 HTTPS and safe browsing  
- 🧠 Social engineering & impersonation  
- 🦠 Malware & antivirus limitations  
- 📶 Public Wi-Fi risks  
- 🔗 Shortened URLs  
- 💾 Data backups & ransomware  
- 📱 App permissions  

Score feedback: `< 50%` Keep learning · `50–69%` Good start · `70–89%` Great effort · `≥ 90%` Cybersecurity pro 🏆

---

## 📁 Project Structure

```
CybersecurityAwarenessBot_Part3/
├── setup_database.sql
├── CybersecurityAwarenessBot_Part3.sln
└── CybersecurityAwarenessBot.Gui/
    ├── Program.cs
    ├── MainForm.cs                        ← 4-tab GUI
    ├── Models/
    │   ├── BotResponse.cs
    │   ├── ConversationState.cs
    │   ├── SentimentLevel.cs
    │   ├── CyberTask.cs                   ← NEW: Task model
    │   ├── QuizQuestion.cs                ← NEW: Quiz model
    │   └── ActivityLogEntry.cs            ← NEW: Log model
    └── Services/
        ├── CybersecurityChatbotEngine.cs  ← Parts 1 & 2 engine (updated)
        ├── TopicLibrary.cs
        ├── SentimentDetector.cs
        ├── AsciiArtProvider.cs
        ├── AudioPlayer.cs
        ├── DatabaseService.cs             ← NEW: MySQL + fallback
        ├── QuizEngine.cs                  ← NEW: 12-question quiz
        ├── ActivityLogger.cs              ← NEW: Session log
        └── NlpProcessor.cs               ← NEW: Intent detection
```

---

## 🔖 Commit History

| # | Commit | Description |
|---|--------|-------------|
| 1 | `chore: initialise Part 3 project scaffold` | `.sln`, `.csproj`, `Program.cs`, `setup_database.sql` |
| 2 | `feat: add all data models for Part 3` | `CyberTask`, `QuizQuestion`, `ActivityLogEntry` + preserved models |
| 3 | `feat: carry forward Part 1 & 2 services with Part 3 awareness` | Engine, TopicLibrary, SentimentDetector, ASCII, Audio |
| 4 | `feat(task-1): add MySQL DatabaseService and ActivityLogger` | DB CRUD with in-memory fallback + session logger |
| 5 | `feat(task-2): add cybersecurity quiz engine with 12 questions` | MC + T/F questions, shuffle, scoring, tiered feedback |
| 6 | `feat(task-3&4): add NLP simulation, Activity Log, and complete four-tab GUI` | NlpProcessor + full MainForm with all 4 tabs |

---

## 🛠️ Technologies

- **Language:** C# 12 / .NET 8
- **UI Framework:** Windows Forms (WinForms) — code-only, no designer files
- **Database:** MySQL via `MySqlConnector` 2.3.7 NuGet package
- **NLP:** Keyword detection + compiled `System.Text.RegularExpressions`
- **Platform:** Windows (net8.0-windows)
