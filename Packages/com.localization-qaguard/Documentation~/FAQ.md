# Localization QA Guard FAQ

## What localization systems are supported?

v1 focuses on Unity Localization package assets (`SharedTableData` + `StringTable`).

## Why do I see "unused key" issues that are actually used?

Unused key detection relies on text-based reference scanning in project files.  
If a key is built dynamically at runtime, it may be reported as unused. Treat these as review candidates.

## What does the safe auto-fix do?

Current safe auto-fix targets one deterministic case:

- Empty translation value in a locale, while the same key has non-empty text in the reference locale.

The tool can copy the reference text into the empty value.

## Can I preview fixes before writing?

Yes. Enable **Dry run safe fix** in the window before applying.

## Can this run in CI?

Yes. Use:

- execute method: `LocalizationQAGuard.Editor.Cli.LocalizationQAGuardCli.RunFromCommandLine`
- `--lqg-fail-on=error|warning` for gating

## Does it auto-delete unused keys?

Not in v1. Unused keys are reported as informational cleanup candidates.

