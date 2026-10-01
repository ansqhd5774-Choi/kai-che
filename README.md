# kai-che

카이셰(Kai-Ché) 모바일 게임 프로젝트.

## 기술 스택
- Engine: Unity 6.3 LTS — 6000.3.25f1
- Language: C#
- Rendering: Universal Render Pipeline (URP 17.3)
- Target: Mobile (Android / iOS)
- Repository: Git + GitHub

## 개발 방향
1. 싱글플레이 코어 게임플레이를 먼저 완성한다.
2. 게임 규칙과 데이터는 네트워크 코드와 분리한다.
3. 저장/계정/통신 계층은 교체 가능한 인터페이스로 둔다.
4. 온라인 전환 시 기존 코어 로직을 최대한 재사용한다.
5. 모바일 성능과 실제 기기 테스트를 초기 단계부터 포함한다.

## 브랜치
- `main`: 안정 버전
- `develop`: 통합 개발 브랜치
- 기능 작업은 `feature/*` 브랜치로 분리

## 현재 프로젝트 경계
- `Core`: Unity 엔진에 의존하지 않는 핵심 규칙
- `Gameplay`: 플레이 진행과 게임플레이 로직
- `Presentation`: 화면, 연출, UI 연결
- `Infrastructure`: 저장/플랫폼/외부 기능
- `Network`: 향후 온라인 통신 구현

## 문서
- `docs/ARCHITECTURE.md`: 프로젝트 구조와 온라인 확장 원칙
- `docs/ROADMAP.md`: 초기 개발 단계
- `docs/SETUP_WINDOWS.md`: Windows/Unity 로컬 설정

## 현재 상태
Unity 프로젝트 골격과 코드 어셈블리 경계까지 `develop` 브랜치에 구성됨.
실제 Unity Editor 최초 오픈 및 Android 기기 빌드 대기.
