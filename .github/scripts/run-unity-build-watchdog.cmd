@echo off
setlocal EnableExtensions EnableDelayedExpansion

if "%UNITY_EXE%"=="" exit /b 90
if "%GITHUB_WORKSPACE%"=="" exit /b 91

if not exist Logs mkdir Logs
del /q "Logs\android-build.log" 2>nul
del /q "Logs\unity-exit-code.txt" 2>nul
del /q "Logs\unity-watchdog.txt" 2>nul

if exist "Builds\Android\KaiChe-dev.apk" del /q "Builds\Android\KaiChe-dev.apk"
if exist "%LOCAL_APK_DIR%\KaiChe-dev.apk" del /q "%LOCAL_APK_DIR%\KaiChe-dev.apk"

echo [WATCHDOG] Starting Unity Android build.> "Logs\unity-watchdog.txt"
echo [WATCHDOG] Stall threshold: 180 seconds.>> "Logs\unity-watchdog.txt"

start "" /b cmd /v:on /c ""%UNITY_EXE%" -batchmode -nographics -quit -stackTraceLogType Full -projectPath "%GITHUB_WORKSPACE%" -buildTarget Android -executeMethod KaiChe.EditorTools.KaiCheAndroidBuild.BuildDevelopmentApk -logFile "%GITHUB_WORKSPACE%\Logs\android-build.log" ^& echo !errorlevel!^>"%GITHUB_WORKSPACE%\Logs\unity-exit-code.txt""

set "LAST_SIZE=-1"
set /a STALE_TICKS=0
set /a TICK_SECONDS=30
set /a MAX_STALE_TICKS=6

:watch
if exist "Logs\unity-exit-code.txt" goto finished

set "CURRENT_SIZE=0"
if exist "Logs\android-build.log" for %%A in ("Logs\android-build.log") do set "CURRENT_SIZE=%%~zA"

if not "!CURRENT_SIZE!"=="!LAST_SIZE!" (
  set "LAST_SIZE=!CURRENT_SIZE!"
  echo [HEARTBEAT] Unity log advanced to !CURRENT_SIZE! bytes.
  echo [HEARTBEAT] Unity log advanced to !CURRENT_SIZE! bytes.>> "Logs\unity-watchdog.txt"
) else (
  echo [HEARTBEAT] Unity still running; log unchanged at !CURRENT_SIZE! bytes.
  echo [HEARTBEAT] Unity still running; log unchanged at !CURRENT_SIZE! bytes.>> "Logs\unity-watchdog.txt"
)

ping -n 31 127.0.0.1 >nul
goto watch

:finished
set "RC=125"
if exist "Logs\unity-exit-code.txt" set /p RC=<"Logs\unity-exit-code.txt"

echo [WATCHDOG] Unity exit code: !RC!
echo [WATCHDOG] Unity exit code: !RC!>> "Logs\unity-watchdog.txt"

if exist "Logs\android-build.log" type "Logs\android-build.log"
if exist "Logs\unity-watchdog.txt" type "Logs\unity-watchdog.txt"

if not "!RC!"=="0" (
  echo [ERROR] Unity build failed or was terminated. Code=!RC!
  exit /b !RC!
)

if not exist "Builds\Android\KaiChe-dev.apk" (
  echo [ERROR] Unity exited successfully but APK is missing.
  exit /b 40
)

echo [PASS] Android APK created.
exit /b 0
