@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "REGISTRY=Docs\ThirdParty\35-LIB-INTEGRATION.md"
set "MANIFEST=Packages\manifest.json"
set "FAIL=0"

if not exist "%REGISTRY%" (
  echo [ERROR] Missing 35-lib registry: %REGISTRY%
  exit /b 81
)
if not exist "%MANIFEST%" (
  echo [ERROR] Missing Unity package manifest.
  exit /b 82
)
if not exist "THIRD_PARTY_NOTICES.md" (
  echo [ERROR] Missing THIRD_PARTY_NOTICES.md.
  exit /b 83
)

for /L %%N in (1,1,35) do (
  findstr /L /C:"| %%N |" "%REGISTRY%" >nul
  if errorlevel 1 (
    echo [ERROR] Registry row %%N is missing.
    set "FAIL=1"
  )
)

for %%P in (
  com.annulusgames.lit-motion
  com.annulusgames.u-pools
  com.cysharp.messagepipe
  com.cysharp.messagepipe.vcontainer
  com.cysharp.unitask
  com.cysharp.zstring
  com.dbrizov.naughtyattributes
  com.harumak.unityscreennavigator
  com.inspiaaa.unityhfsm
  com.mackysoft.serializereference-extensions
  jp.hadashikick.vcontainer
) do (
  findstr /L /C:"%%P" "%MANIFEST%" >nul
  if errorlevel 1 (
    echo [ERROR] Active package pin missing: %%P
    set "FAIL=1"
  )
)

for %%F in (
  "Assets\_Project\Gameplay\Survivor\RuntimeBulletPatternMath.cs"
  "Assets\_Project\Gameplay\Survivor\RuntimeRoguelikeSupport.cs"
  "Assets\_Project\Gameplay\Survivor\RuntimeBossStateMachine.cs"
  "Assets\_Project\Gameplay\Survivor\RuntimeBossPatternConfig.cs"
  "Assets\_Project\Infrastructure\ThirdPartyIntegrationContracts.cs"
  "Assets\_Project\Infrastructure\ThirdPartyRuntimeAdapters.cs"
  "Assets\_Project\Presentation\ThirdPartyPresentationAdapters.cs"
) do (
  if not exist %%F (
    echo [ERROR] Integration file missing: %%~F
    set "FAIL=1"
  )
)

findstr /L /C:"LitMotion" "Assets\_Project\Gameplay\KaiChe.Gameplay.asmdef" >nul || set "FAIL=1"
findstr /L /C:"UnityHFSM" "Assets\_Project\Gameplay\KaiChe.Gameplay.asmdef" >nul || set "FAIL=1"
findstr /L /C:"MackySoft.SerializeReferenceExtensions" "Assets\_Project\Gameplay\KaiChe.Gameplay.asmdef" >nul || set "FAIL=1"
findstr /L /C:"NaughtyAttributes.Core" "Assets\_Project\Gameplay\KaiChe.Gameplay.asmdef" >nul || set "FAIL=1"
findstr /L /C:"uPools" "Assets\_Project\Infrastructure\KaiChe.Infrastructure.asmdef" >nul || set "FAIL=1"
findstr /L /C:"MessagePipe" "Assets\_Project\Infrastructure\KaiChe.Infrastructure.asmdef" >nul || set "FAIL=1"
findstr /L /C:"VContainer" "Assets\_Project\Infrastructure\KaiChe.Infrastructure.asmdef" >nul || set "FAIL=1"
findstr /L /C:"UniTask" "Assets\_Project\Infrastructure\KaiChe.Infrastructure.asmdef" >nul || set "FAIL=1"
findstr /L /C:"UnityScreenNavigator" "Assets\_Project\Presentation\KaiChe.Presentation.asmdef" >nul || set "FAIL=1"
findstr /L /C:"ZString" "Assets\_Project\Presentation\KaiChe.Presentation.asmdef" >nul || set "FAIL=1"

if "%FAIL%"=="1" (
  echo [ERROR] 35-library integration registry validation failed.
  exit /b 84
)

echo [PASS] 35/35 library candidates have a tracked Kai-Che application path.
echo [PASS] Active package pins and integration seams are present.
exit /b 0
