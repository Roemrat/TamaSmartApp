@echo off
echo Building TamaSmartApp...
dotnet build -c Release
if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful!
    echo Executable is at: bin\Release\net6.0-windows\TamaSmartApp.exe
) else (
    echo.
    echo Build failed!
    pause
)
