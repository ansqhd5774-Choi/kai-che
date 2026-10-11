# Kai-Che 35 Library Integration Registry

Branch: `feature/35-libs-integration-r1`
Base branch: `develop`
Unity: `6000.6.4f1`

## Goal

All 35 candidates must contribute through one of these controlled paths without breaking the playable mobile top-down roguelike loop.

- `ACTIVE-PACKAGE`: pinned dependency is present in the Unity project.
- `ACTIVE-PORT`: selected reusable logic has been implemented in Kai-Che-owned code.
- `ADOPTED-ARCH`: useful architecture was adopted without importing the whole template/project.
- `READY-CONTRACT`: a stable Kai-Che interface now exists so the library can be swapped in without changing gameplay code.
- `READY-OPTIONAL`: exact activation path is known, but the package remains off until its feature is needed.
- `READY-ONLINE`: isolated behind transport/backend contracts; activation requires an explicit online/server architecture decision.

`READY-*` is intentionally not the same as runtime activation. Competing pool/event/network/backend frameworks are not allowed to control the same responsibility simultaneously.

## 35/35 application map

| # | Repository | Kai-Che role | Current application path | Status |
|---:|---|---|---|---|
| 1 | Roo-Roo-Roo/survivors-roguelike-kit | survivors loop | current kill → XP → level → 3-choice → boss loop and modular extraction reference | ADOPTED-ARCH |
| 2 | annulusgames/uPools | enemy/projectile/XP pooling | pinned UPM package; preferred pool backend | ACTIVE-PACKAGE |
| 3 | Inspiaaa/UnityHFSM | elite/boss state machine | pinned UPM package; preferred complex enemy FSM | ACTIVE-PACKAGE |
| 4 | getsentry/sentaur-survivors | survivors architecture comparison | current runtime loop compared/adopted selectively; no whole-project import | ADOPTED-ARCH |
| 5 | annulusgames/LitMotion | UI/hit/camera feedback | pinned UPM package + top-down impact feedback integration | ACTIVE-PACKAGE |
| 6 | adammyhre/Unity-Event-Bus | event separation | `IGameEventBus` seam; MessagePipe is current backend | READY-CONTRACT |
| 7 | JohnVonDrashek/benedict-bullethell-patterns | boss projectile patterns | ring/spread/aim/spiral/rotation math in `RuntimeBulletPatternMath.cs` | ACTIVE-PORT |
| 8 | No78Vino/gameplay-ability-system-for-unity | ability/buff design | ability stat/modifier primitives in `RuntimeRoguelikeSupport.cs` | ACTIVE-PORT |
| 9 | shapedbyrainstudios/save-load-system | save/load design | versioned `RuntimeRunSnapshot` JSON primitive | ACTIVE-PORT |
| 10 | mesut-atakan/Virtual-Joystick-For-Unity | mobile input | existing runtime touch joystick + dead-zone helper | ACTIVE-PORT |
| 11 | nvjob/nvjob-boids | crowd separation | separation-force helper for dense enemy groups | ACTIVE-PORT |
| 12 | adag21044/Unity-Top-Down-Shooter-Roguelike | top-down shooter reference | current top-down camera/movement/projectile architecture | ADOPTED-ARCH |
| 13 | noisybass/Top-Down-Shooter | top-down structure reference | current shooter loop structure; no obsolete project import | ADOPTED-ARCH |
| 14 | sjai013/unity-gameplay-ability-system | ability architecture reference | ability modifier seam; archived dependency itself stays out | ACTIVE-PORT |
| 15 | Kink3d/kPooling | pool alternative | `IGamePoolAdapter<T>` / `PoolBackend.KPooling` switch point | READY-CONTRACT |
| 16 | mackysoft/XPool | pool alternative | `IGamePoolAdapter<T>` / `PoolBackend.XPool` switch point | READY-CONTRACT |
| 17 | Cysharp/UniTask | allocation-conscious async | pinned UPM package | ACTIVE-PACKAGE |
| 18 | Cysharp/R3 | reactive UI/events | `EventBackend.R3` switch point; NuGet activation recipe retained | READY-OPTIONAL |
| 19 | Cysharp/MemoryPack | high-speed serialization | `IStateSerializer` seam; NuGet activation remains isolated | READY-OPTIONAL |
| 20 | Cysharp/MessagePipe | message bus | pinned UPM package; preferred event backend | ACTIVE-PACKAGE |
| 21 | hadashiA/VContainer | dependency injection | pinned UPM package + MessagePipe integration package | ACTIVE-PACKAGE |
| 22 | Haruma-K/UnityScreenNavigator | uGUI lifecycle/navigation | pinned UPM package at Unity-6-compatible commit | ACTIVE-PACKAGE |
| 23 | gasgiant/Camera-Shake | camera feedback reference | functionality implemented through LitMotion in top-down camera; no redundant shake framework | ACTIVE-PORT |
| 24 | SharlatanY/NavMeshSurface2DBaker | obstacle navigation reference | grid-navigation seam/helper retained for map-obstacle phase | ACTIVE-PORT |
| 25 | RonenNess/Unity-2d-pathfinding | A* reference | allocation-bounded `RuntimeGridPathfinder` implementation | ACTIVE-PORT |
| 26 | Tencent/behaviac | advanced AI alternative | `IEnemyDecisionBackend` / `AiBackend.Behaviac` switch point | READY-CONTRACT |
| 27 | scellecs/morpeh | ECS crowd alternative | `AiBackend.Morpeh` performance-gated switch point; GameObject loop remains default | READY-OPTIONAL |
| 28 | MirrorNetworking/Mirror | multiplayer framework | `IRealtimeTransport` + `NetworkBackend.Mirror` isolated path | READY-ONLINE |
| 29 | RevenantX/LiteNetLib | UDP transport | `IRealtimeTransport` + `NetworkBackend.LiteNetLib` isolated path | READY-ONLINE |
| 30 | Cysharp/MagicOnion | realtime/API RPC | `IRealtimeTransport` + `NetworkBackend.MagicOnion` isolated path | READY-ONLINE |
| 31 | PlayFab/UnitySDK | account/cloud backend | `IRemoteProfileStore` + `CloudBackend.PlayFab` isolated path | READY-ONLINE |
| 32 | supabase/supabase-csharp | account/character/inventory/save backend | `IRemoteProfileStore` + `CloudBackend.Supabase` isolated path | READY-ONLINE |
| 33 | Cysharp/ZString | low-allocation strings | pinned UPM package | ACTIVE-PACKAGE |
| 34 | mackysoft/Unity-SerializeReferenceExtensions | polymorphic inspector | Unity 6000.6 compatibility fork pinned after upstream `children` compile failure | ACTIVE-PACKAGE |
| 35 | dbrizov/NaughtyAttributes | inspector productivity | pinned UPM package | ACTIVE-PACKAGE |

## Current package pins

- uPools: `e11ce9c12c65c12f3bc35ae2ef7cacde6826e503`
- UnityHFSM: `f8fdc2efb862522acd26fab6cd940886c84cde37`
- LitMotion: `422eb124051c81a9bc3f422ebf190703c5053514`
- UniTask: `a77a1b4a89824fb6df293099de3060daf2a432c1`
- MessagePipe core/VContainer: `47f036c55c378a3f0ba9f89c4380a500e32a7379`
- VContainer: `fb24d752968c5cabe0cfb4fcf8861c74772c5b14`
- UnityScreenNavigator: `c7c0c367f82811f6077cc27bf7739c3c058a4a6b`
- ZString: `b9c1d8a8a10d37f344b63384d3c44ed814fda1d7`
- SerializeReferenceExtensions compatibility source: `zuryansh/Demon-of-the-Dungeon-3@ee8452dbc43373ef991707089b7d7e66ee5227fb`
- NaughtyAttributes: `b11d62b28c5fcbe242988f9f8d451deb925cf23b`

## Implemented Kai-Che-owned integration code

- `Assets/_Project/Gameplay/Survivor/RuntimeBulletPatternMath.cs`
  - ring / spread / aimed / spiral / rotation pattern math.
- `Assets/_Project/Gameplay/Survivor/RuntimeTopDownShooterCamera.cs`
  - LitMotion-powered impact feedback while preserving top-down camera authority.
- `Assets/_Project/Gameplay/Survivor/RuntimeRoguelikeSupport.cs`
  - joystick dead zone, crowd separation, ability modifiers, run snapshot, grid A*.
- `Assets/_Project/Infrastructure/ThirdPartyIntegrationContracts.cs`
  - pool/event/AI/network/cloud/serializer seams for competing or future libraries.

## NuGet-dependent candidates

R3, MemoryPack and MagicOnion require a controlled NuGetForUnity path rather than ad-hoc DLL copying. NuGetForUnity documents CLI restore as the CI/CD-safe path. Before any of these is activated:

1. Pin NuGetForUnity tooling.
2. Commit deterministic `packages.config` / NuGet configuration.
3. Restore packages before Unity starts in self-hosted CI.
4. Put each optional integration in its own assembly boundary.
5. Re-run Android build/install/runtime smoke.

They are intentionally not activated in the playable client until the feature requires them.

## Online/backend rule

Mirror, LiteNetLib and MagicOnion are alternatives at different layers, not three simultaneous authorities. PlayFab and Supabase are also backend choices. The contracts are ready, but `NetworkBackend.None` and `CloudBackend.None` remain the default until the server architecture is explicitly selected.

This preserves the project rule that core game direction/server structure must not be changed implicitly.

## Promotion gates

For every activation batch:

1. Source/package license verified.
2. Immutable source revision pinned.
3. Unity package/NuGet restore succeeds.
4. C# compile succeeds.
5. Android APK build succeeds.
6. APK stages on runner.
7. Device ADB online.
8. APK install succeeds.
9. Game process launches.
10. Existing top-down runtime smoke passes.
11. No regression in movement/shooting/XP/level-up/boss loop.
12. Only then mark runtime activation verified.

## Current execution order

1. Repair and validate R1-R4 package baseline on Unity `6000.6.4f1`.
2. Validate UnityScreenNavigator + Unity 6.6 SerializeReference compatibility source.
3. Wire support primitives into gameplay only where profiler/playtest evidence justifies them.
4. Add deterministic NuGet CI lane for R3/MemoryPack before activating either package.
5. Keep Morpeh and all online/backend libraries isolated until an explicit performance/server decision.
