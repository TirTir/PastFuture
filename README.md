# Past, Future

1인칭 3D 싱글플레이 타임루프 미스터리 게임 (개발 중, 비공개).
시나리오 01: 바다는 죽은 자를 기억한다 (The Sea Remembers).

> 이 저장소의 문서에는 스토리 스포일러가 포함되어 있다 (개발자 전용).

## 개발 환경

- Unity 6000.3.29f1 (Unity 6.3 LTS), Universal Render Pipeline 17.3.0
- Git + Git LFS (clone 전에 `git lfs install` 필요)
- 개발: macOS / 목표 출시 플랫폼: Windows (Steam)

```bash
git lfs install
```

```bash
git clone https://github.com/TirTir/PastFuture.git
```

Unity Hub에서 Add → `PastFuture` 폴더를 열고, Editor 버전 6000.3.29f1을 사용한다.

## 문서

- [Docs/ProjectStatus.md](Docs/ProjectStatus.md) — 현재 진행 상황
- [Docs/DevelopmentRules.md](Docs/DevelopmentRules.md) — 개발 규칙, 테스트 실행 방법
- [Docs/Handover.md](Docs/Handover.md) — 설계 인수인계 원본 (GDD 포함)
- [Docs/ADR/](Docs/ADR/README.md) — 아키텍처 결정 기록

## 폴더 구조

```text
Assets/PastFuture/Scripts/{Domain,Application,Infrastructure,UnityAdapters,Presentation,Bootstrap}
Assets/PastFuture/Scenes/Test/  생성기로 만든 테스트 씬 (M1_TestRoom)
Assets/PastFuture/Settings/     입력 액션 등 설정 에셋
Assets/Editor/            Editor 전용 코드
Assets/Tests/EditMode/    EditMode 테스트
Assets/Tests/PlayMode/    PlayMode 테스트
Docs/                     설계·규칙·결정 문서
```
