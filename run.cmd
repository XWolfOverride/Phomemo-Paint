@echo off
rem PhomemoPaint - lanza la app en modo desarrollo (doble clic desde el Explorador).
rem Requiere el SDK de .NET instalado. Para el .exe final, usa build.ps1.
cd /d "%~dp0"
dotnet run --project "src\PhomemoPaint\PhomemoPaint.csproj" %*
if errorlevel 1 (
    echo.
    echo Fallo al ejecutar. Revisa que el SDK de .NET este instalado.
    pause
)
