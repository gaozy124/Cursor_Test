# AGENTS.md

## Cursor Cloud specific instructions

### Overview

Unity Editor package for localization QA validation (**Localization QA Guard**). The core business logic (rules, models, services) is pure C# with no Unity dependencies and is testable via .NET 8.0 + xUnit. The Unity-specific code (Editor UI, asset scanners) requires Unity 2021.3+ and cannot be run outside Unity.

### Prerequisites

- .NET 8.0 SDK (installed at `$HOME/.dotnet`, added to PATH via `~/.bashrc`)

### Key commands

| Task | Command |
|------|---------|
| **Build tests** | `dotnet build tests/LocalizationQAGuard.Core.Tests/LocalizationQAGuard.Core.Tests.csproj` |
| **Run tests** | `dotnet test tests/LocalizationQAGuard.Core.Tests/LocalizationQAGuard.Core.Tests.csproj` |
| **Restore deps** | `dotnet restore tests/LocalizationQAGuard.Core.Tests/LocalizationQAGuard.Core.Tests.csproj` |

### Architecture notes

- The test project at `tests/LocalizationQAGuard.Core.Tests/` compiles source files directly from `Packages/com.localization-qaguard/Editor/Core/` via a wildcard `<Compile>` include — there is no separate class library project.
- No `.sln` file exists; use the `.csproj` path directly with `dotnet` commands.
- There is no linter configured (no `.editorconfig`, `dotnet format` config, or Roslyn analyzers beyond defaults). `dotnet build` with zero warnings is the closest lint equivalent.
- The Editor UI (`LocalizationQAGuardWindow.cs`), CLI (`LocalizationQAGuardCli.cs`), and Unity scanners depend on `UnityEditor`/`UnityEngine` assemblies and cannot compile or run outside Unity.

### Gotchas

- The .NET SDK is installed per-user at `$HOME/.dotnet`. If `dotnet` is not found, run: `export DOTNET_ROOT=$HOME/.dotnet && export PATH=$DOTNET_ROOT:$PATH`
- NuGet restore requires network access on first run; subsequent builds use cached packages.
