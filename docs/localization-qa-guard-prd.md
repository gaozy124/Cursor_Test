# Localization QA Guard PRD (v1)

## Problem
Unity teams with multiple locales frequently ship text regressions:

- Missing translations for one or more locales
- Empty values that render blank UI
- Placeholder mismatches (`{0}`, `{name}`) across languages
- Broken brace formatting in smart strings
- Localization keys that linger after refactors

Most teams discover these late, manually, or not at all.

## Product Goal
Deliver an Editor-first QA tool that detects localization issues before release and supports basic CI gating.

## Target Users
- Indie and small/mid-size Unity teams
- Teams using Unity Localization package
- Teams that need a lightweight, low-setup quality gate

## v1 Scope (Must Have)
1. Scan localization collections and locales from project assets
2. Rules:
   - Duplicate keys
   - Missing translations
   - Empty translations
   - Placeholder consistency
   - Smart string brace balance
   - Unused key hints (text-reference scan)
3. Report model with severity and fix guidance
4. Export reports to JSON and CSV
5. Editor UI:
   - Run scan
   - Filter issues by severity/search text
   - Export results
   - Ping related asset
6. CLI/headless entrypoint with fail-on policy

## Non-Goals (v1)
- Runtime translation services / machine translation
- Deep semantic language quality checks
- Auto-editing all issue classes
- Full support for every third-party localization framework

## Inputs and Outputs
### Inputs
- Unity localization assets in project (`SharedTableData`, `StringTable`)
- Optional scan configuration (reference locale, enabled checks)

### Outputs
- In-editor issue list
- Report file (`.json` or `.csv`)
- CI exit code for pass/fail

## Severity Model
- **Error**: High confidence release blockers (missing translation, duplicate key, placeholder mismatch, broken braces)
- **Warning**: Potential quality defects (empty translation)
- **Info**: Cleanup candidates (unused key)

## Success Criteria (v1)
- Teams can run one scan and receive actionable issues in < 3 clicks
- CI can fail builds on localization errors
- Reports are human-readable and script-consumable

