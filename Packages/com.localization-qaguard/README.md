# Localization QA Guard

Localization QA Guard is an Editor-focused Unity package that scans localization assets and surfaces quality issues before release.

## MVP Features

- Missing translation detection per locale
- Empty translation value checks
- Duplicate key detection
- Placeholder consistency checks (`{0}`, `{name}`)
- Smart string syntax checks
- Unused key detection (text-based project scan)
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

