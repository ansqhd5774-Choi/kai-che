from pathlib import Path

p = Path('.github/workflows/android-device-install.yml')
s = p.read_text(encoding='utf-8')

old_name = 'name: Verify R24-R30 progression, persistence, and novice base character'
new_name = 'name: Verify R24-R30 progression, persistence, and Sprite Foundry character'
if old_name not in s:
    raise SystemExit('step name not found')
s = s.replace(old_name, new_name, 1)

old = '''          "%ADB_EXE%" logcat -d -t 6000 > Logs\\r30-character-logcat.txt 2>&1
          findstr /C:"KAI_CHE_R30_NOVICE_BASE_BODY_ACTIVE" Logs\\r30-character-logcat.txt >nul
          if errorlevel 1 (
            echo [ERROR] R30 novice base body activation marker missing.
            exit /b 122
          )
          findstr /C:"KAI_CHE_R30_NOVICE_BASE_BODY_READY" Logs\\r30-character-logcat.txt >nul
          if errorlevel 1 (
            echo [ERROR] R30 novice base body ready marker missing.
            exit /b 123
          )
          "%ADB_EXE%" exec-out screencap -p > Logs\\r30-character-game-screen.png 2>nul
          echo [PASS] R30 novice human base character active on device.
'''
new = '''          "%ADB_EXE%" logcat -d -t 6000 > Logs\\sprite-foundry-character-logcat.txt 2>&1
          findstr /C:"KAI_CHE_SPRITE_FOUNDRY_MAGE_READY_8DIR" Logs\\sprite-foundry-character-logcat.txt >nul
          if errorlevel 1 (
            "%ADB_EXE%" exec-out screencap -p > Logs\\sprite-foundry-character-failure.png 2>nul
            echo [ERROR] Sprite Foundry 8-direction ready marker missing.
            exit /b 122
          )
          findstr /C:"KAI_CHE_SPRITE_FOUNDRY_MAGE_ACTIVE" Logs\\sprite-foundry-character-logcat.txt >nul
          if errorlevel 1 (
            echo [ERROR] Sprite Foundry active marker missing.
            exit /b 123
          )
          "%ADB_EXE%" exec-out screencap -p > Logs\\sprite-foundry-character-game-screen.png 2>nul
          echo [PASS] Sprite Foundry mage active with 8-direction runtime on device.
'''
if old not in s:
    raise SystemExit('R30 validation block not found')
s = s.replace(old, new, 1)

old_artifacts = '            Logs/r30-character-logcat.txt\n            Logs/r30-character-game-screen.png'
new_artifacts = '            Logs/sprite-foundry-character-logcat.txt\n            Logs/sprite-foundry-character-game-screen.png\n            Logs/sprite-foundry-character-failure.png'
if old_artifacts not in s:
    raise SystemExit('R30 artifact block not found')
s = s.replace(old_artifacts, new_artifacts, 1)

p.write_text(s, encoding='utf-8')
print('Sprite Foundry device gate patched')
