# API Key & Secrets Setup

## Environment Variable Resolution Order
The SDK looks for an API key automatically if you do **not** set `GeminiClientOptions.ApiKey` explicitly.
It checks these environment variables in order:
1. `GEMINI_API_KEY`
2. `GOOGLE_GEMINI_API_KEY`
3. `GEMINI_KEY`

If none are set and you do not assign `ApiKey`, requests that require auth will fail.

## Local Development
Set in your shell (PowerShell example):
```powershell
$env:GEMINI_API_KEY = "YOUR_REAL_KEY"
```
Or in *bash*:
```bash
export GEMINI_API_KEY=YOUR_REAL_KEY
```

## GitHub Actions (CI)
Add the key as an encrypted repository secret.
1. In GitHub UI: Settings -> Secrets and variables -> Actions -> New repository secret.
2. Name: `GEMINI_API_KEY` Value: (paste key) Save.
3. Workflow already exports it for test steps (`env: GEMINI_API_KEY: ${{ secrets.GEMINI_API_KEY }}`).

For package publishing also set a `NUGET_API_KEY` secret if you want automatic pushes on commits whose message starts with `release:`.

## Do NOT Commit Keys
Never hard‑code keys in code or commit them to the repo. The example placeholder `YOUR_API_KEY` in samples is safe; replace via environment when running locally.

## Rotating Keys
Update the secret in GitHub then invalidate the old key in the provider console. No workflow change needed as long as the secret name stays the same.

## Troubleshooting
- Key not picked up: Ensure no leading/trailing spaces and that your terminal session was restarted (or variable exported in current session).
- CI tests failing with auth errors: Confirm secret name matches exactly (`GEMINI_API_KEY`) and that the workflow run log shows `GEMINI_API_KEY` present (but value masked).
