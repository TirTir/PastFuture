# CLAUDE.md — Past, Future

1인칭 3D 싱글플레이 타임루프 미스터리 게임. Unity 6.3 LTS (6000.3.29f1) + URP, 순수 C# Domain, Modular Monolith.

## 먼저 읽을 것

1. [Docs/DevelopmentRules.md](Docs/DevelopmentRules.md) — 작업 규칙 (Handover 5절 원문 + M0 프로젝트 규칙). **반드시 따른다.**
2. [Docs/ProjectStatus.md](Docs/ProjectStatus.md) — 현재 단계, 실제 실행 근거, 미결정
3. [Docs/Handover.md](Docs/Handover.md) — 인수인계 원본 (수정 금지). 필요한 절만 읽는다.
   - 0절 출처와 한계 (line 11), 1절 절대 변경하지 않을 게임 계약 (line 51)
   - 2절 아키텍처 16-1~16-18 (line 62), 3절 충돌·미결정 (line 586)
   - 4절 로드맵 M0~M9·Quality Gate (line 628), 5절 AI 개발 지침 (line 701)
   - 부록 B ADR·테스트 추적 (line 723), 부록 A GDD v0.2 원문 (line 1627, 스포일러)
4. [Docs/ADR/](Docs/ADR/README.md) — ADR-563 이후 결정

## 핵심 규칙 요약 (상세는 DevelopmentRules.md)

- 보고는 한국어, 코드·식별자·기술 용어는 영어 원문.
- 요청 범위만 수정. 관계없는 문제는 보고만 하고 승인 전 수정하지 않는다.
- 실제로 실행하지 않은 빌드·테스트는 완료로 표시하지 않는다. 미실행은 NotRun.
- 커밋·push는 사용자가 요청할 때만.
- 원문이 없는 ADR-142~357, 16-8~16-13 상세에 내용을 지어내지 않는다. 필요하면 새 제안(ADR-563 이후)으로 분리한다.
- 확정 게임 규칙(9개 능력, 한 회차 한 능력, Day 1 07:00 / Day 2 19:00, 고정된 과거와 실제 범인, 지식 계승·실제 재산 초기화 등)과 콘텐츠 ID는 바꾸지 않는다.

## 코드 규칙

- 계층: Domain / Application(둘 다 엔진 참조 금지) → Infrastructure, UnityAdapters → Presentation → Bootstrap. 허용 참조 표는 DevelopmentRules.md 3절.
- asmdef를 추가하거나 참조를 바꾸면 `Assets/Tests/EditMode/Architecture/AssemblyBoundaryTests.cs`의 `AllowedProjectReferences`를 함께 갱신한다 (승인 필요).
- `PastFuture.*` 안에서 Unity 타입은 전체 이름: `UnityEngine.Application`, `UnityEditor.Editor` (ADR-564).
- MonoBehaviour는 view/adapter 역할만 하고 WorldState를 직접 쓰지 않는다. `Time.timeScale`로 게임 시간 배율을 구현하지 않는다 (Handover 16-3, 16-17).
- `.meta`는 Unity가 만든다. 손으로 만들거나 GUID를 바꾸지 않는다.

## 명령

EditMode 테스트 (Unity Editor가 이 프로젝트를 열고 있으면 먼저 닫는다):

```bash
"/Applications/Unity/Hub/Editor/6000.3.29f1/Unity.app/Contents/MacOS/Unity" -batchmode -nographics -projectPath /Users/yujin/Projects/PastFuture -runTests -testPlatform EditMode -testResults TestResults/editmode.xml -logFile TestResults/editmode.log
```

PlayMode 테스트 (Editor를 먼저 닫는다, `-nographics` 쓰지 않음, ADR-565):

```bash
"/Applications/Unity/Hub/Editor/6000.3.29f1/Unity.app/Contents/MacOS/Unity" -batchmode -projectPath /Users/yujin/Projects/PastFuture -runTests -testPlatform PlayMode -testResults TestResults/playmode.xml -logFile TestResults/playmode.log
```

순수 .NET headless smoke (Editor를 닫지 않아도 됨, ADR-570):

```bash
dotnet test /Users/yujin/Projects/PastFuture/Tools/Headless/PastFuture.Tests.Headless/PastFuture.Tests.Headless.csproj
```

Editor가 열려 있을 때 새 파일을 반영하려면 Unity 창을 활성화하거나 `Cmd+R`. 컴파일 결과는 `~/Library/Logs/Unity/Editor.log`.
