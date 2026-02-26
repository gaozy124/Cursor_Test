# Asset Store Listing Draft

## Title

Localization QA Guard – Validate, Report, and Gate Translation Quality

## Short Description

Catch missing, empty, and malformed localization content before release. Generate reports, run checks in CI, and safely autofix empty values from a reference locale.

## Long Description

Localization QA Guard is an Editor-focused validation toolkit for Unity Localization workflows.

Instead of finding localization defects late in QA or after release, run one scan and get actionable, severity-ranked issues with exportable reports.

### What it checks

- Duplicate localization keys
- Missing translations by locale
- Empty translation values
- Placeholder consistency (`{0}`, `{name}`) across locales
- Smart string brace-balance issues
- Unused key candidates (project text-reference scan)

### What it exports

- JSON report
- CSV report with metadata header (project + summary stats)

### CI-ready

- Batch mode CLI entrypoint
- Configurable fail policy (`none`, `error`, `warning`)

### Safe fix support

- Deterministic safe-fix mode for empty translations:
  - copy reference-locale value into empty locale values
- Dry-run preview before writing assets

## Key Benefits

1. Prevent shipping obvious localization regressions.
2. Give QA and localization vendors machine-readable reports.
3. Introduce localization quality gates into CI without heavy setup.

## Known Limitations (v1)

- Primary support is Unity Localization assets (`SharedTableData` + `StringTable`).
- Unused key detection is text-reference based and may flag dynamic runtime keys.
- Safe auto-fix currently targets empty translation values only.

