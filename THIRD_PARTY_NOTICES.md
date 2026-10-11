# Third-Party Notices

This file tracks third-party source/package dependencies that are currently installed or directly influence Kai-Che-owned integration code on `feature/35-libs-integration-r1`.

Do not assume art, audio, fonts, demo assets, or other bundled media inherit the source-code license. Kai-Che does not import those assets unless their license is checked separately.

## Installed Unity packages

| Project | Revision | License | Kai-Che use |
|---|---|---|---|
| annulusgames/uPools | `e11ce9c12c65c12f3bc35ae2ef7cacde6826e503` | MIT | pooling backend candidate/current package |
| Inspiaaa/UnityHFSM | `f8fdc2efb862522acd26fab6cd940886c84cde37` | MIT | elite/boss state-machine package |
| annulusgames/LitMotion | `422eb124051c81a9bc3f422ebf190703c5053514` | MIT | camera/UI/game-feel motion |
| Cysharp/UniTask | `a77a1b4a89824fb6df293099de3060daf2a432c1` | MIT | allocation-conscious async |
| Cysharp/MessagePipe | `47f036c55c378a3f0ba9f89c4380a500e32a7379` | MIT | message/event bus and VContainer integration |
| hadashiA/VContainer | `fb24d752968c5cabe0cfb4fcf8861c74772c5b14` | MIT | dependency injection |
| Haruma-K/UnityScreenNavigator | `c7c0c367f82811f6077cc27bf7739c3c058a4a6b` | MIT | uGUI screen lifecycle/navigation |
| Cysharp/ZString | `b9c1d8a8a10d37f344b63384d3c44ed814fda1d7` | MIT | low-allocation string support |
| mackysoft/Unity-SerializeReferenceExtensions | compatibility source in `zuryansh/Demon-of-the-Dungeon-3@ee8452dbc43373ef991707089b7d7e66ee5227fb` | MIT | polymorphic inspector; compatibility source retains MackySoft authorship/package metadata |
| dbrizov/NaughtyAttributes | `b11d62b28c5fcbe242988f9f8d451deb925cf23b` | MIT | editor inspector productivity |

The full MIT copyright/license text for each package remains in its source repository/package where shipped. If a package is vendored into this repository later, its upstream LICENSE file must be vendored with it.

## Selective algorithm / architecture references

These repositories are not wholesale dependencies. Kai-Che uses selected ideas or independently maintained Kai-Che-owned code and keeps the original source reference for traceability.

| Project | Use |
|---|---|
| Roo-Roo-Roo/survivors-roguelike-kit | survivors-loop and modular gameplay architecture reference |
| getsentry/sentaur-survivors | survivors architecture comparison |
| JohnVonDrashek/benedict-bullethell-patterns | bullet-pattern design reference for `RuntimeBulletPatternMath.cs` |
| No78Vino/gameplay-ability-system-for-unity | ability/buff/status architecture reference |
| shapedbyrainstudios/save-load-system | save/load structure reference |
| mesut-atakan/Virtual-Joystick-For-Unity | mobile joystick behavior reference |
| nvjob/nvjob-boids | crowd-separation/flocking reference |
| adag21044/Unity-Top-Down-Shooter-Roguelike | top-down roguelike gameplay reference |
| noisybass/Top-Down-Shooter | top-down shooter structure reference |
| sjai013/unity-gameplay-ability-system | ability architecture reference only; archived project is not an active dependency |
| gasgiant/Camera-Shake | camera feedback reference only; Kai-Che implementation uses LitMotion instead |
| SharlatanY/NavMeshSurface2DBaker | 2D navigation reference |
| RonenNess/Unity-2d-pathfinding | grid pathfinding reference |

## Prepared but not activated

The following projects currently have Kai-Che integration contracts or activation gates, but their packages/source are not active runtime dependencies. This prevents overlapping frameworks from becoming simultaneous authorities.

- adammyhre/Unity-Event-Bus
- Kink3d/kPooling
- mackysoft/XPool
- Cysharp/R3
- Cysharp/MemoryPack
- Tencent/behaviac
- scellecs/morpeh
- MirrorNetworking/Mirror
- RevenantX/LiteNetLib
- Cysharp/MagicOnion
- PlayFab/UnitySDK
- supabase/supabase-csharp

Before activation, verify the exact source revision, license, Unity/Android compatibility, dependency tree, and CI smoke result again.
