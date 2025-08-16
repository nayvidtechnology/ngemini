Param(
    [string]$Key
)

if(-not $Key){
  if($env:GEMINI_API_KEY){ $Key = $env:GEMINI_API_KEY }
  elseif(Test-Path .env){
    Get-Content .env | ForEach-Object { if($_ -match '^[ ]*GEMINI_API_KEY=(.*)$'){ $Key = $Matches[1] } }
  }
}
if(-not $Key){ Write-Error 'No API key provided. Pass -Key, set GEMINI_API_KEY env var, or add to .env.'; exit 1 }
$env:GEMINI_API_KEY = $Key
Write-Host "Running tests with key length: $($Key.Length)" -ForegroundColor Cyan
# Run only tests (no build) for speed
& dotnet test Nayvid.Gemini.slnx --nologo
