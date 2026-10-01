# Windows Local Setup

## Required editor
- Unity 6000.3.25f1 (Unity 6.3 LTS)
- Template/render pipeline: 3D URP

## Unity Hub modules
Install these with the editor:
- Android Build Support
  - Android SDK & NDK Tools
  - OpenJDK
- Windows Build Support is optional for local desktop test builds.
- iOS Build Support can be added later; final iOS builds require macOS/Xcode.

## Repository workflow
1. Clone `ansqhd5774-Choi/kai-che`.
2. Checkout `develop`.
3. Open the repository root as the Unity project.
4. Let Unity resolve packages and generate missing `.meta` / project settings files.
5. Do not commit `Library`, `Temp`, `Logs`, `Obj`, or build outputs.
6. Commit Unity-generated source assets and their matching `.meta` files.

## First editor validation
After the project opens:
- Confirm editor version is 6000.3.25f1.
- Confirm Universal Render Pipeline resolves successfully.
- Confirm there are no compile errors.
- Create the first bootstrap scene under `Assets/_Project/Scenes`.
- Switch build target to Android.
- Perform a development build on a physical Android device.

## Branch policy
- `main`: stable milestones only.
- `develop`: integrated development.
- `feature/*`: isolated feature work.

## Important
Do not hand-edit Unity-generated binary assets. Text-based Unity YAML may be reviewed in Git, but scenes, prefabs, settings, and render assets should normally be created or migrated by the matching Unity editor version.
