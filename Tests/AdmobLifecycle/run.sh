#!/bin/bash
set -euo pipefail
unity_dir="${UNITY_EDITOR_CONTENTS:-/Applications/Unity/Hub/Editor/6000.0.80f1/Unity.app/Contents}"
test_dir="$(cd "$(dirname "$0")" && pwd)"
output_dir="$(mktemp -d)"
trap 'rm -rf "$output_dir"' EXIT
"$unity_dir/MonoBleedingEdge/bin/mcs" -define:BICUTIL_ADMOB,UNITY_ANDROID -out:"$output_dir/tests.exe" \
  "$test_dir/Stubs.cs" "$test_dir/Tests.cs" "$test_dir/../../Runtime/Ads/Admob/AdmobManager.cs"
"$unity_dir/MonoBleedingEdge/bin/mono" "$output_dir/tests.exe"
