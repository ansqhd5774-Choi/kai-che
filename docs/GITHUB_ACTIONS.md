# GitHub Actions and PC runner

The project uses two kinds of runners:

- `Unity Project Preflight` runs lightweight project checks on GitHub-hosted
  Ubuntu runners. Standard hosted runners are free for this public repository.
- `Unity Android Build` runs the Unity 6000.6.4f1 Android build on the Windows
  PC runner, where Unity Personal is already activated in Unity Hub. No Unity
  account password, serial, or license file is stored in GitHub Actions secrets.

## PC runner requirements

Register the repository runner at **Settings > Actions > Runners > New
self-hosted runner** with the `kaise-unity` label. Configure it in
`C:\actions-runner-kaise` and run `run.cmd` while the PC is on and connected to
the internet. The project also needs Unity 6000.6.4f1 with Android Build Support,
SDK, NDK, and OpenJDK, plus Git and Git LFS.

The APK is uploaded to the workflow run as `KaiChe-Android-<commit>` and copied
to `C:\actions-runner-kaise\local-builds\KaiChe-dev.apk`. The commit SHA is
stored next to the APK so the device-install workflow can verify it.

## Triggers and security

The APK build triggers only for relevant pushes to `develop`, or when manually
dispatched on `develop`. It has no `pull_request` trigger, and the job itself
rejects refs other than `develop`. Do not add public fork pull-request jobs to
the self-hosted runner: workflow code runs directly on the PC. Keep the runner
available only to this repository and shut it down when PC builds are not
needed.

The self-hosted runner itself has no GitHub Actions execution charge for this
public repository; the PC's electricity, internet, and maintenance remain local
costs. GitHub-hosted standard runner usage is also free for public repositories.
