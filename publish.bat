@echo off
setlocal enabledelayedexpansion
cd /d "%~dp0"

rem ============================================================
rem  Dong goi Game Assistant Pro
rem  -> self-contained, 1 file exe, KHONG can cai .NET runtime
rem  -> giai nen la chay thang giong mau "Train Basic V32"
rem
rem  Cach dung:
rem     publish.bat            (mac dinh win-x64, 64-bit)
rem     publish.bat win-x86    (32-bit)
rem ============================================================

set "RID=%~1"
if "%RID%"=="" set "RID=win-x64"

set "OUTDIR=publish\GameAssistantPro"
set "ZIP=GameAssistantPro-%RID%.zip"

echo.
echo === Publish (%RID%) ===
dotnet publish GameAssistantPro\GameAssistantPro.csproj ^
  -c Release -r %RID% --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:EnableCompressionInSingleFile=true ^
  -p:PublishReadyToRun=true ^
  -p:DebugType=none -p:DebugSymbols=false ^
  -o "%OUTDIR%"

if errorlevel 1 (
  echo.
  echo [LOI] Publish that bai. Kiem tra da cai .NET 8 SDK chua.
  pause
  exit /b 1
)

echo.
echo === Nen thanh ZIP de chia se ===
powershell -NoProfile -Command "Compress-Archive -Path '%OUTDIR%\*' -DestinationPath '%ZIP%' -Force"

echo.
echo ============================================================
echo  XONG!
echo   - Chay truc tiep : %OUTDIR%\GameAssistantPro.exe
echo   - File nen        : %ZIP%
echo  (Cau hinh config.json se nam canh file exe khi chay.)
echo ============================================================
pause
