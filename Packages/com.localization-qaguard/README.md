# Localization QA Guard

Localization QA Guard is an Editor-focused Unity package that scans localization assets and surfaces quality issues before release.

## MVP Features

- Missing translation detection per locale
- Empty translation value checks
- Duplicate key detection
- Placeholder consistency checks (`{0}`, `{name}`)
- Smart string syntax checks
- Unused key detection (text-based project scan)
- Safe auto-fix mode for empty translations (copy from reference locale)
- JSON and CSV report export
- Command-line execution for CI gating

## Install

1. Open `Packages/manifest.json` in your Unity project.
2. Add this package as a local dependency:

```json
{
  "dependencies": {
    "com.localization-qaguard": "file:../path/to/com.localization-qaguard"
  }
}
```

## Open Tool

- `Tools/Localization QA Guard/Open`

## Safe Fixes

- Use **Apply Safe Fixes (Empty → Reference)** in the tool window.
- Supports **dry run** mode to preview candidate fixes before writing assets.
- Current safe fix behavior:
  - If a locale value exists but is empty, copy from reference locale text.

## Report Metadata

Both JSON and CSV exports include metadata:

- generation timestamp (UTC)
- project name/path
- collection and entry totals
- issue summary counts

## CLI

Run Unity in batch mode with:

```bash
Unity -batchmode -quit \
  -projectPath /path/to/project \
  -executeMethod LocalizationQAGuard.Editor.Cli.LocalizationQAGuardCli.RunFromCommandLine \
  --lqg-output=/path/to/report.json \
  --lqg-format=json \
  --lqg-fail-on=error
```

### CLI Args

- `--lqg-output=...`
- `--lqg-format=json|csv`
- `--lqg-fail-on=none|error|warning`
- `--lqg-check-unused=true|false`
- `--lqg-reference-locale=en`

