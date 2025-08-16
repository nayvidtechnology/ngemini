@echo off
setlocal ENABLEDELAYEDEXPANSION
set KEY=%1
if "%KEY%"=="" (
  if not "%GEMINI_API_KEY%"=="" set KEY=%GEMINI_API_KEY%
  if exist .env for /f "usebackq tokens=1,2 delims==" %%A in (".env") do (
    if /I "%%A"=="GEMINI_API_KEY" set KEY=%%B
  )
)
if "%KEY%"=="" (
  echo No API key provided. Pass as first arg, set GEMINI_API_KEY, or add to .env.
  exit /b 1
)
set GEMINI_API_KEY=%KEY%
echo Running tests with key length: %KEY:~0,0%%=%%
REM Run tests
 dotnet test Nayvid.Gemini.slnx --nologo
endlocal
