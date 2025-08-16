# Nayvid.RealApiSample

Demonstrates running the SDK against a real Gemini API key supplied via the GEMINI_API_KEY environment variable. Current library methods still contain placeholder stub logic for generation; this project is a staging spot to replace those stubs with real HTTP calls when implemented.

## Usage
```powershell
$Env:GEMINI_API_KEY = '<your_real_key>'
dotnet run --project samples/Nayvid.RealApiSample/Nayvid.RealApiSample.csproj
```

Output shows operation IDs / statuses and byte counts for speech.
