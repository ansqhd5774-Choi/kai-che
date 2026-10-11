# Kai-Che 35-Library Integration Validation — 2026-10-12

## Final status

**PASS — feature/35-libs-integration-r1 has completed the defined 35-library validation gate.**

No merge to `develop` or `main` is included in this validation record.

## Gate results

| Gate | Result | Evidence |
|---|---|---|
| 35/35 application trace | PASS | Run `38163189721`, job `114540223123`; `Tools\validate-35-libs.cmd` emitted `35/35 library candidates have a tracked Kai-Che application path` and `Active package pins and integration seams are present` |
| Optional R3 + MemoryPack Unity compatibility compile | PASS | Run `38161552932`; head `16fac03a4fa5fcdfdb1ce49a76cac2d93ca645da` |
| Android Unity build | PASS | Run `38160053702`; gameplay APK source `dd6c7c16a0931cbaec30f235073242562d56220e` |
| APK source-drift check | PASS | `dd6c7c16...` → pre-document head `fed9bab5...` changes only `.github/workflows/35-libs-optional-compat.yml` and `Tools/OptionalCompatProject/packages.config`; no `Assets`, `Packages`, or `ProjectSettings` gameplay build input changed |
| Physical-device ADB online/install/launch | PASS | Run `38162268692`, job `114537487856`; runner `KAISE-UNITY-DESKTOP-N62L0NG`, device `SM_G998N`, package `com.kaiche.rpg` |
| R20 battlefield/town route | PASS | Run `38162268692` |
| Top-down/perspective camera runtime | PASS | Device log markers `KAI_CHE_R19_PERSPECTIVE_CAMERA_ACTIVE`, `KAI_CHE_R20_XZ_WORLD_ACTIVE`, `KAI_CHE_TOPDOWN_SHOOTER_CAMERA_ACTIVE` |
| Touch movement + camera follow | PASS | Run `38163451463`; actual Android swipe input captured before/after. Central world profile displaced about 448 px while the player remained camera-centered; normalized vertical-profile correlation at that displacement was about 0.953 |
| Automatic shooting | PASS | Device markers `KAI_CHE_FIREBALL_FIRST_SHOT`, `KAI_CHE_ICEBALL_FIRST_SHOT` plus active-skill first-shot markers |
| XP → level-up | PASS | Deterministic physical-device gate run `38162985466`, job `114539616789`; `KAI_CHE_LEVEL_POWER_...` observed after live combat XP |
| 3-choice skill generation | PASS | Run `38162985466`; `KAI_CHE_SKILL_CHOICE_READY` and `KAI_CHE_R23_CHOICES_...|...|...` observed; choices then resolved and combat resumed |
| Boss wave 3 spawn → defeat → reward | PASS | Run `38162985466`; `KAI_CHE_BOSS_SPAWN_FIRST`, `KAI_CHE_BOSS_DEFEATED_FIRST`, `KAI_CHE_BOSS_DEFEATED_WAVE_3`, and persisted `PendingBossRewards > 0` verified |
| R24 skill persistence | PASS | Run `38162268692` |
| R25 boss reward persistence | PASS | Run `38162268692` |
| R26 non-zero skill mastery persistence | PASS | Run `38162268692` |
| R27 health checkpoint persistence | PASS | Run `38162268692` |
| R28 zero-health recovery | PASS | Run `38162268692` |
| R29 potion consume/restart persistence | PASS | Run `38162268692` |
| R30 novice base character runtime | PASS | Run `38162268692` |
| PlayerPrefs safety after deterministic tests | PASS | Test-only seeded state was backed up first and original PlayerPrefs was restored byte-for-byte after the runtime gates |

## Evidence artifacts

### Android device regression

- Run: `38162268692`
- Artifact: `KaiChe-Android-Device-Evidence`
- Artifact ID: `11709928396`
- Files: 41
- Size: 1,226,457 bytes
- SHA-256: `204e743e4974ebd6ca86d88442f1cc4f5c44b3094a1f81a097dea3a2b6fdefeb`

### Deterministic combat core gates

- Run: `38162985466`
- Job: `114539616789`
- Artifact: `KaiChe-R35-Core-Runtime-Gates-V3`
- Artifact ID: `11710969407`
- Files: 16
- Size: 489,289 bytes
- SHA-256: `6e60aadb0e519be98459daac818abf37a799f9315684a083225a3e8a68043323`
- Direct PASS log lines:
  - `Shooting XP level-up and three-choice generation verified.`
  - `Level-up choices resolved.`
  - `Boss wave 3 spawn defeat and reward persistence verified.`
  - `Original PlayerPrefs restored byte-for-byte.`

### Touch movement

- Run: `38163451463`
- Artifact: `KaiChe-Touch-Movement-Evidence`
- Artifact ID: `11711127234`
- Size: 264,295 bytes
- SHA-256: `adaaf3967d0484d180e911fc112e2e6eba81e450a93c691cb5d3683c5c66fbbf`
- Captures: `movement-before.png`, `movement-after.png`, runtime logcat, original/restored PlayerPrefs comparison

## Source authority

- Gameplay APK validated here: `dd6c7c16a0931cbaec30f235073242562d56220e`
- Optional compatibility compile validated at: `16fac03a4fa5fcdfdb1ce49a76cac2d93ca645da`
- Since the APK commit, the feature branch changed only the optional compatibility workflow/project dependency declarations before this evidence document; gameplay build inputs did not drift.

## Non-blocking CI hygiene

These do not invalidate the completed runtime gate but remain cleanup work:

1. Permanent `.github/workflows/android-device-install.yml` has several brittle quoted-XML `findstr` checks that can emit `FINDSTR: Cannot open >nul`. The later deterministic gates used safer checks and passed cleanly.
2. Windows checkout cleanup can warn about Unity `Library/Bee` paths exceeding legacy path length. Actions recovered by recreating the checkout and validation completed successfully. A later CI cleanup can enable/standardize Git long-path handling.

## Promotion decision

Validation is complete on `feature/35-libs-integration-r1`. Promotion/merge is intentionally left separate and requires an explicit merge decision.
