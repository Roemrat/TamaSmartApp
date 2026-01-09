@echo off
echo Publishing TamaSmartApp...
dotnet publish -c Release -r win-x86 --self-contained false
if %ERRORLEVEL% EQU 0 (
    echo.
    echo Publish successful!
    echo Executable is at: bin\Release\net48\win-x86\publish\TamaSmartApp.exe
) else (
    echo.
    echo Publish failed!
    pause
)
