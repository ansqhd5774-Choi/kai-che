# Kai-Ché Initial Roadmap

## Phase 0 — Repository & Engine Setup
- [x] GitHub repository
- [x] Unity 6.3 LTS selected
- [x] Unity 6000.3.25f1 pinned
- [x] 3D URP baseline selected
- [x] Unity .gitignore / .gitattributes
- [x] C# editor rules
- [x] architecture baseline
- [x] Unity repository project skeleton
- [x] Core / Gameplay / Presentation / Infrastructure / Network assembly boundaries
- [ ] Open project successfully in Unity Editor
- [ ] Generate first bootstrap scene in Editor
- [ ] First Android device build

## Phase 1 — Playable Vertical Slice
- [ ] Player movement
- [ ] Camera
- [ ] One test map
- [ ] One enemy
- [ ] Basic combat
- [ ] HP / damage / death
- [ ] Minimal HUD
- [ ] Restart loop

## Phase 2 — Core RPG Systems
- [ ] Character stats
- [ ] Equipment
- [ ] Inventory
- [ ] Skills
- [ ] NPC interaction
- [ ] Quest/state progression
- [ ] Save/load

## Phase 3 — Mobile Production Base
- [ ] Touch controls
- [ ] Performance budget
- [ ] Android build pipeline
- [ ] iOS build preparation
- [ ] Crash/logging policy
- [ ] Data versioning

## Phase 4 — Online-Ready Refactor Gate
Before adding real networking:
- [ ] Core rules independent from UI/network
- [ ] Authoritative state boundaries documented
- [ ] Local save replaced by repository interfaces
- [ ] Commands/events defined for synchronized state
- [ ] Authentication/session design documented

## Phase 5 — Online Expansion
- [ ] Login/account
- [ ] Backend API
- [ ] Session/lobby
- [ ] Multiplayer transport
- [ ] Server-authoritative gameplay where required
- [ ] Persistence/database
- [ ] Reconnect/error handling

## 원칙
처음부터 서버를 크게 만드는 대신, 싱글플레이의 핵심 루프를 먼저 실제 기기에서 플레이 가능하게 만든다. 다만 온라인 전환을 막는 강한 결합은 초기부터 피한다.
