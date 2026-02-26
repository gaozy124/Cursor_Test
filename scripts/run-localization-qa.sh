#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 2 ]]; then
  echo "Usage: $0 <UNITY_EXECUTABLE_PATH> <PROJECT_PATH> [OUTPUT_PATH] [FAIL_ON]"
  echo "Example:"
  echo "  $0 \"/opt/Unity/Editor/Unity\" \"/workspace/MyGame\" \"/workspace/reports/lqg.json\" \"error\""
  exit 1
fi

UNITY_BIN="$1"
PROJECT_PATH="$2"
OUTPUT_PATH="${3:-$PROJECT_PATH/lqg-report.json}"
FAIL_ON="${4:-error}"
FORMAT="${LQG_FORMAT:-json}"
CHECK_UNUSED="${LQG_CHECK_UNUSED:-true}"
REFERENCE_LOCALE="${LQG_REFERENCE_LOCALE:-}"

EXTRA_ARGS=()
if [[ -n "$REFERENCE_LOCALE" ]]; then
  EXTRA_ARGS+=("--lqg-reference-locale=$REFERENCE_LOCALE")
fi

mkdir -p "$(dirname "$OUTPUT_PATH")"

"$UNITY_BIN" \
  -batchmode \
  -quit \
  -projectPath "$PROJECT_PATH" \
  -executeMethod LocalizationQAGuard.Editor.Cli.LocalizationQAGuardCli.RunFromCommandLine \
  --lqg-output="$OUTPUT_PATH" \
  --lqg-format="$FORMAT" \
  --lqg-fail-on="$FAIL_ON" \
  --lqg-check-unused="$CHECK_UNUSED" \
  "${EXTRA_ARGS[@]}"

