# AvaloniaTests

A cross-platform desktop app for creating multiple-choice tests, taking them and reviewing the results.
Built with [Avalonia UI] 11 and .NET 8 following the MVVM pattern.

> The user interface is in Russian.

## Features

- **Test editor** – create and edit tests with a title, a description and any number of questions.
  Each question needs at least two answer options and exactly one correct answer.
- **Test list** – browse, edit and delete saved tests.
- **Taking a test** – step through the questions with *Back* / *Next*, change answers at any time
  and finish to see the score and the percentage of correct answers.
- **Results** – every attempt is saved with the user name (the current OS user), date, score and percentage;
  results can be opened in detail or deleted.
- **Local storage** – everything is kept in JSON files, no database or server required.
- **Sample data** – two sample tests are available on first launch.

### Main menu

| Button | Action |
|---|---|
| **НАЧАТЬ** (Start) | Choose a test from the list and take it |
| **СОЗДАТЬ** (Create) | Create a new test |
| **СПИСОК** (List) | Browse, edit and delete tests |
| **РЕЗУЛЬТАТЫ** (Results) | View and delete saved results |

## Tech stack

| Area | Technology |
|---|---|
| UI | Avalonia UI 11.3 (Fluent theme) |
| Platform | .NET 8 |
| MVVM | ReactiveUI 20, CommunityToolkit.Mvvm 8 |
| Dependency injection | Microsoft.Extensions.DependencyInjection |
| Storage | System.Text.Json |
| Tests | xUnit, Avalonia.Headless.XUnit |
| CI | GitHub Actions |

## Getting started

### Prerequisites

- [.NET SDK] 8.0 or newer (the app targets `net8.0`,
  so the .NET 8 runtime must be installed to run it)
- Windows, Linux (X11 or XWayland session) or macOS

### Run the app

```bash
git clone <repository-url>
cd Avalonia-Test
dotnet run --project AvaloniaTests/AvaloniaTests.csproj
```

### Run the tests

```bash
dotnet test TestsAvaloniaMVVM.sln
```

## Data storage

Data is stored as JSON in the user's application data folder:

| OS | Folder |
|---|---|
| Windows | `%APPDATA%\AvaloniaTests\` |
| Linux / macOS | `~/.config/AvaloniaTests/` |

| File | Content |
|---|---|
| `tests.json` | Tests with their questions and answers |
| `results.json` | Test attempts: user, date, score and the chosen answers |

On first launch `tests.json` is seeded with the sample tests shipped next to the executable
(and, when the app is started from the source folder, with sample results from `AvaloniaTests/results.json`).
Delete the folder to reset the app to its initial state.

<details>
<summary>Example of <code>tests.json</code></summary>

```json
[
  {
    "Id": "550e8400-e29b-41d4-a716-446655440000",
    "Title": "Primer testa",
    "Description": "Testovyj primer dlya proverki raboty",
    "Questions": [
      {
        "Id": "550e8400-e29b-41d4-a716-446655440001",
        "Text": "Chto takoe Avalonia?",
        "Answers": [
          { "Id": "550e8400-e29b-41d4-a716-446655440002", "Text": "UI framework dlya .NET" },
          { "Id": "550e8400-e29b-41d4-a716-446655440003", "Text": "Baza dannyh" }
        ],
        "CorrectAnswerId": "550e8400-e29b-41d4-a716-446655440002"
      }
    ]
  }
]
```

</details>

## Project structure

```
Avalonia-Test/
├── .github/workflows/ci.yml   # CI: build and tests on Ubuntu and Windows
├── TestsAvaloniaMVVM.sln
├── AvaloniaTests/             # Desktop application
│   ├── Program.cs             # Entry point
│   ├── App.axaml(.cs)         # Global styles and DI container setup
│   ├── ViewLocator.cs
│   ├── Models/                # Test, Question, Answer, TestResult
│   ├── ViewModels/            # One view model per window
│   ├── Views/                 # Windows and dialogs (.axaml)
│   ├── Services/              # JSON storage, window and dialog services
│   ├── Converters/            # XAML value converters
│   ├── Assets/                # Icon and background images
│   ├── tests.json             # Sample tests
│   └── results.json           # Sample results
└── AvaloniaTests.Tests/       # xUnit tests
    ├── Fakes/                 # In-memory fakes of the services
    ├── Models/
    ├── Services/
    ├── Converters/
    ├── ViewModels/
    └── Views/                 # Headless UI smoke tests
```

## Architecture

The app follows the MVVM pattern:

- **Models** are plain classes serialized with System.Text.Json. `Test` and `Question` keep a serializable
  list (`QuestionsData`, `AnswersData`) next to a bindable `ObservableCollection` (`Questions`, `Answers`);
  `FixCollections()` rebuilds the observable collections after loading.
- **Services** are registered in the DI container (`ServiceProvider` in `App.axaml.cs`):
  - `ITestService` / `IResultService` – persistence (`JsonTestService`, `JsonResultService`);
  - `IWindowService` / `IDialogService` – open windows and modal dialogs, so view models never reference views;
  - `IErrorDialogService` – error message box.
- **View models** receive services through their constructors, expose ReactiveUI commands and ask their
  window to close through a `CloseRequested` event.
- **Views** are Avalonia windows (`.axaml`) that bind to the view models.

## Testing and CI

`AvaloniaTests.Tests` contains 100+ tests:

- **Models and services** – collection syncing, JSON save/load round-trips and validation of the bundled
  `tests.json`. Storage tests run in a temporary folder, so real user data is never touched.
- **View models** – scoring, navigation, validation and command logic, using in-memory fakes of the services.
- **Converters** – the value converters used by the views.
- **Views** – headless smoke tests ([Avalonia.Headless.XUnit])

The GitHub Actions workflow in [`.github/workflows/ci.yml`] runs on every push to
`main`/`master`, on pull requests and on demand. It builds the solution in Release mode, runs all tests on
Ubuntu and Windows and uploads the `.trx` test results as a build artifact.

## License

MIT
