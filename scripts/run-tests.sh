#!/usr/bin/env bash
set -euo pipefail
KEY="${1:-}";
if [ -z "$KEY" ]; then
  if [ -n "${GEMINI_API_KEY:-}" ]; then KEY="$GEMINI_API_KEY"; fi
fi
if [ -z "$KEY" ] && [ -f .env ]; then
  while IFS='=' read -r k v; do
    if [ "$k" = "GEMINI_API_KEY" ]; then KEY="$v"; fi
  done < .env
fi
if [ -z "$KEY" ]; then
  echo "No API key provided. Arg1, GEMINI_API_KEY env var or .env file required." >&2
  exit 1
fi
export GEMINI_API_KEY="$KEY"
echo "Running tests with key length: ${#KEY}" >&2
exec dotnet test Nayvid.Gemini.slnx --nologo
