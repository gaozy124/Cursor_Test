# Localization QA Guard Prototype

This repository contains a Unity package prototype for **Localization QA Guard**, a localization quality validation tool focused on:

- Missing and empty translations
- Duplicate keys
- Placeholder consistency checks
- Smart string brace validation
- Unused key discovery
- JSON/CSV export and CI/headless scanning

## Package Location

- `Packages/com.localization-qaguard`

## Editor Menu

- `Tools/Localization QA Guard/Open`
- `Tools/Localization QA Guard/Run CLI Scan`

## CLI Entry Method

`LocalizationQAGuard.Editor.Cli.LocalizationQAGuardCli.RunFromCommandLine`

Arguments:

- `--lqg-output=/absolute/path/report.json`
- `--lqg-format=json|csv`
- `--lqg-fail-on=none|error|warning`
- `--lqg-check-unused=true|false`
- `--lqg-reference-locale=en`

## CI Helper Script

Use:

```bash
scripts/run-localization-qa.sh "/path/to/Unity" "/path/to/UnityProject" "/path/to/report.json" "error"
```

## Core Logic Tests

Run:

```bash
dotnet test tests/LocalizationQAGuard.Core.Tests/LocalizationQAGuard.Core.Tests.csproj
```
