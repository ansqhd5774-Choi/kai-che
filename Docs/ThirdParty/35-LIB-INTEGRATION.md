# Kai-Che 35 Library Integration Registry

Branch: `feature/35-libs-integration-r1`
Base: `feature/2d-8dir-r1@490256f8b457a79cffbe88d57253f02f1aebb1ea`
Unity: `6000.6.4f1`

## Rules

- Do not install overlapping frameworks blindly.
- Every dependency must pass Unity compile, Android build, install, launch, and runtime smoke before promotion.
- Pin Git UPM packages to immutable commit SHAs.
- Whole-game/template repositories are source-reference/code-port candidates, not package dependencies.
- Art/audio/font licenses inside sample repositories are not inherited from source-code licenses; verify separately before copying assets.
- Network/backend libraries remain isolated until the local top-down roguelike loop is stable.

## Status legend

- `INSTALLED-R1`: added to `Packages/manifest.json`, awaiting/under CI verification.
- `NEXT`: next integration batch after current CI passes.
- `PORT`: inspect and port selected code only.
- `REFERENCE`: architecture/algorithm reference only unless a concrete need appears.
- `LATE`: deferred until online/backend phase.
- `HOLD`: do not integrate until a conflict/need review is complete.

| # | Repository | Role | Method | Status |
|---:|---|---|---|---|
| 1 | Roo-Roo-Roo/survivors-roguelike-kit | survivors loop reference | selective code port | PORT |
| 2 | annulusgames/uPools | enemy/projectile/XP pooling | UPM Git | INSTALLED-R1 |
| 3 | Inspiaaa/UnityHFSM | elite/boss FSM | UPM Git | NEXT |
| 4 | getsentry/sentaur-survivors | survivors architecture comparison | selective code port | PORT |
| 5 | annulusgames/LitMotion | UI/hit/camera/game-feel motion | UPM Git | NEXT |
| 6 | adammyhre/Unity-Event-Bus | event separation reference | compare with MessagePipe | HOLD |
| 7 | JohnVonDrashek/benedict-bullethell-patterns | boss bullet patterns | selective code port | PORT |
| 8 | No78Vino/gameplay-ability-system-for-unity | skill/buff/status design | selective code port | PORT |
| 9 | shapedbyrainstudios/save-load-system | local save/load reference | selective code port | REFERENCE |
| 10 | mesut-atakan/Virtual-Joystick-For-Unity | mobile joystick reference | selective code port | REFERENCE |
| 11 | nvjob/nvjob-boids | crowd separation/flocking reference | selective code port | REFERENCE |
| 12 | adag21044/Unity-Top-Down-Shooter-Roguelike | top-down gameplay reference | selective code port | PORT |
| 13 | noisybass/Top-Down-Shooter | top-down structure reference | selective code port | REFERENCE |
| 14 | sjai013/unity-gameplay-ability-system | ability architecture reference | archived/reference only | REFERENCE |
| 15 | Kink3d/kPooling | pooling alternative | compare with uPools | HOLD |
| 16 | mackysoft/XPool | pooling alternative | compare with uPools | HOLD |
| 17 | Cysharp/UniTask | allocation-conscious async | UPM Git | INSTALLED-R1 |
| 18 | Cysharp/R3 | reactive events/UI | NuGet + Unity package review | HOLD |
| 19 | Cysharp/MemoryPack | high-speed serialization | package integration later | LATE |
| 20 | Cysharp/MessagePipe | event/message bus | UPM Git | INSTALLED-R1 |
| 21 | hadashiA/VContainer | DI/system modularization | UPM Git | INSTALLED-R1 |
| 22 | Haruma-K/UnityScreenNavigator | uGUI screen lifecycle/navigation | UPM Git review | NEXT |
| 23 | gasgiant/Camera-Shake | hit/shoot/boss camera feedback | selective package/code integration | NEXT |
| 24 | SharlatanY/NavMeshSurface2DBaker | 2D obstacle navigation | conditional integration | REFERENCE |
| 25 | RonenNess/Unity-2d-pathfinding | simple tile A* reference | algorithm reference | REFERENCE |
| 26 | Tencent/behaviac | BT/FSM/HTN AI | BSD-3-Clause, compare with UnityHFSM | HOLD |
| 27 | scellecs/morpeh | ECS for large crowds | performance-gated | HOLD |
| 28 | MirrorNetworking/Mirror | multiplayer networking | isolated online branch later | LATE |
| 29 | RevenantX/LiteNetLib | reliable UDP transport | isolated online branch later | LATE |
| 30 | Cysharp/MagicOnion | realtime/API RPC | isolated online/server branch later | LATE |
| 31 | PlayFab/UnitySDK | account/cloud backend alternative | backend decision gate | LATE |
| 32 | supabase/supabase-csharp | account/character/inventory/save backend | backend phase | LATE |
| 33 | Cysharp/ZString | low-allocation strings | UPM/package review | NEXT |
| 34 | mackysoft/Unity-SerializeReferenceExtensions | skill/effect polymorphic inspector | editor-only integration | NEXT |
| 35 | dbrizov/NaughtyAttributes | inspector productivity | editor-only integration | NEXT |

## R1 package pins

- uPools: `e11ce9c12c65c12f3bc35ae2ef7cacde6826e503`
- UniTask: `a77a1b4a89824fb6df293099de3060daf2a432c1`
- MessagePipe core: `47f036c55c378a3f0ba9f89c4380a500e32a7379`
- MessagePipe VContainer integration: `47f036c55c378a3f0ba9f89c4380a500e32a7379`
- VContainer: `fb24d752968c5cabe0cfb4fcf8861c74772c5b14`

## Promotion gates

For each batch:

1. Package/source license verified.
2. Package/source drift pinned.
3. Unity package restore succeeds.
4. C# compile succeeds.
5. Android APK build succeeds.
6. APK stages on runner.
7. Device ADB online.
8. APK install succeeds.
9. Game process launches.
10. Existing top-down runtime smoke still passes.
11. No regression in current survivor loop.
12. Only then mark the batch `VERIFIED`.

## Current execution order

- R1 Foundation: uPools + UniTask + VContainer + MessagePipe.
- R2 Combat/AI/Feedback: UnityHFSM + LitMotion + selected bullet-pattern code + camera feedback.
- R3 Editor/Data Productivity: SerializeReferenceExtensions + NaughtyAttributes + ZString; evaluate R3 separately because it requires the .NET R3 package via NuGet.
- R4 Architecture Port: survivors kit, sentaur-survivors, top-down roguelike, ability systems; port only reusable systems into Kai-Che-owned namespaces.
- R5 Performance: crowd separation; only evaluate Morpeh/ECS after profiler evidence shows GameObject architecture is insufficient.
- R6 Online/Backend: Mirror/LiteNetLib/MagicOnion/PlayFab/Supabase/MemoryPack, after explicit architecture choice; do not combine competing networking stacks by default.
