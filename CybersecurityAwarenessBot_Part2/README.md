# Cybersecurity Awareness Assistant – Part 2

A WinForms upgrade of the Cybersecurity Awareness Chatbot for South African citizens.  
This version adds a GUI, keyword recognition, random responses, memory, sentiment detection, follow-up conversation flow, and a cleaner object-oriented structure.

## Features

- Voice greeting on startup
- ASCII art shown inside the GUI
- Neat WinForms layout with colours and spacing
- Personalised chat using the user’s name
- Memory for name and favourite cybersecurity topic
- Keyword recognition for cybersecurity topics
- Randomised phishing responses
- Follow-up prompts such as “tell me more”
- Simple sentiment detection for worried, frustrated, curious, and confused users
- Default response for unknown inputs
- Uses lists, dictionaries, methods, classes, and a custom delegate

## Project Structure

- `CybersecurityAwarenessBot.Gui/` – WinForms application
- `Assets/` – ASCII art and WAV greeting
- `.github/workflows/dotnet.yml` – GitHub Actions CI

## How to Run

1. Open the solution file: `CybersecurityAwarenessBot_Part2.sln`
2. Open in Visual Studio or VS Code with the C# extension
3. Restore, build, and run the WinForms project
4. Make sure `Assets/welcome.wav` and `Assets/ascii_art.txt` are present

## Required Submission Items

- GitHub repository link
- At least six meaningful commits
- Two GitHub releases/tags
- Unlisted YouTube presentation link
- README screenshots of commits and CI success

## Presentation Video

Paste your unlisted YouTube link here:

`https://youtube.com/your-unlisted-video-link`

## Screenshots

Add your screenshots to a folder named `screenshots/` in the repository root, then reference them like this:

```md
![Commit History](screenshots/commits.png)
![GitHub Actions Passed](screenshots/actions-pass.png)
```

## Suggested Git Tags / Releases

- `v1.0.0` – GUI chatbot working version
- `v1.1.0` – Final submission version

## Notes

If you change the greeting file name, update the code path in `MainForm.cs` so it still points to `Assets/welcome.wav`.