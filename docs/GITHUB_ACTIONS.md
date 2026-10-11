# GitHub-hosted Android builds

The Unity Android Build (GitHub-hosted) workflow uses a standard Ubuntu 24.04
GitHub runner. No runner registration or always-on developer PC is required.
Unity's exact version comes from ProjectSettings/ProjectVersion.txt.

## Unity activation

GitHub runner availability does not include a Unity license. Set these repository
Actions secrets under Settings > Secrets and variables > Actions:

- UNITY_EMAIL: Unity account email.
- UNITY_PASSWORD: Unity account password.
- Personal license: UNITY_LICENSE, containing the activated .ulf license file.
- Pro license: UNITY_SERIAL instead of UNITY_LICENSE.

Follow https://game.ci/docs/github/activation/ for your license type. Never commit
credentials or license files, and do not send account passwords through chat.
Enter credentials directly in GitHub's encrypted Actions settings.
The workflow stops before downloading Unity if required secrets are missing.

## Running and downloading

Push relevant project changes to develop to start a build. For manual runs,
choose the workflow in Actions, select develop and Run workflow. Until this
workflow exists on the default branch, the manual dispatch button may not appear;
pushes to develop still trigger it.

Successful builds upload KaiChe-Android-<commit> for 14 days: the development APK,
its source commit and a SHA-256 checksum. Download and extract the artifact for
local installation. The existing custom build method preserves the package ID,
scenes and development settings.

## Local device workflows

Legacy self-hosted workflows remain for explicit local device testing. The hosted
build has a distinct workflow name so the old default-branch device installer is
not automatically triggered: it expects APK files on the developer PC.
No local runner is registered or started by this migration.
Device tests require a separately configured local runner and paired device.
