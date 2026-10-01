# Kai-Ché Architecture

## 목표
싱글플레이로 시작하되, 이후 온라인 기능을 추가할 때 핵심 게임 규칙을 다시 쓰지 않는 구조를 유지한다.

## 계층
### 1. Core
- 전투 규칙
- 능력치 계산
- 아이템/스킬 규칙
- 캐릭터 상태
- 게임 진행 규칙

Core는 Unity UI, 저장 방식, 서버 구현에 직접 의존하지 않는다.

### 2. Gameplay
- 플레이어 입력 처리
- 전투 흐름
- 퀘스트/스테이지 진행
- 월드 상호작용

### 3. Presentation
- 화면
- HUD
- 애니메이션 연결
- 사운드/UI 피드백

### 4. Infrastructure
- 로컬 저장
- 설정
- 로그
- 플랫폼 기능

### 5. Network
초기에는 비활성 또는 로컬 구현을 사용한다.
온라인 전환 시 인증, 세션, 상태 동기화, 서버 API를 이 계층에 추가한다.

## 핵심 규칙
- 게임 규칙 안에서 직접 HTTP 호출을 하지 않는다.
- MonoBehaviour에 핵심 규칙을 몰아넣지 않는다.
- 저장소와 서버는 인터페이스 뒤에 둔다.
- 클라이언트가 최종 판정을 소유해야 하는 온라인 규칙은 만들지 않는다.
- 온라인 전환 가능성이 있는 상태 변화는 명령/이벤트 단위로 분리한다.

## 초기 Unity 폴더 계획
```text
Assets/
  _Project/
    Core/
    Gameplay/
    Presentation/
    Infrastructure/
    Network/
    Data/
    Scenes/
    Tests/
```

Unity 프로젝트를 실제 생성한 뒤 위 구조와 각 `.asmdef` 경계를 추가한다.
