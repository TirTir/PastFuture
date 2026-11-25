# Past, Future — 개발 규칙

- 대상: 이 저장소에서 작업하는 사람과 AI 도구(Claude Code, Codex 등)
- 기준 문서: [Handover.md](Handover.md) (인수인계 v0.3 원본, 수정 금지)
- 1절은 Handover 5절 원문 그대로이다. 2절 이후는 M0(2026-11-25)에서 사용자 승인으로 정한 프로젝트 규칙이며, 근거 ADR을 함께 적는다.

## 1. AI 개발 지침 (Handover 5절 원문)

1. 모든 설명/오류 분석/작업 보고는 한국어, 코드/클래스/변수/함수/파일 및 기술 용어 원문 영어. 모르면 추정하지 말고 미확인으로 보고. GDD/Architecture/ADR을 먼저 읽고 실제 코드 및 git 상태 확인.
2. 요청 범위만 수정. 발견한 관계없는 버그·리팩터링·최적화는 제안과 영향만 보고, 사용자 승인 전 수정하지 않는다. 기존 코드 및 사용자의 미커밋 변경을 임의로 삭제/덮어쓰기 금지. 기존 로직 대체 시 diff/사유와 추적 가능한 변경 이력 남기기. 불필요한 주석 코드 누적은 지양하되 기존 로직 보존/주석 요구가 명시되면 따른다.
3. 공개 API, 저장 Schema/ID, 콘텐츠·확정 규칙 변경은 영향·호환성·테스트 계획 설명 후 승인. Domain no UnityEngine, Single Writer, WorldChangeSet, Knowledge scope, Scene/Entity 분리, 실제 타임루프/엔딩/저장 계약 유지.
4. 수정 전 baseline 테스트 확인, 수정 후 관련 테스트 및 회귀 추가. 테스트 실패 숨기기/조건 완화/Skip 남용/무관 실패 자동 수정 금지. 미실행은 미실행, 실제 통과 로그 없으면 성공 주장 금지.
5. 명시 요청 없는 force reset/force push/대규모 삭제/자동 커밋·푸시 금지. 실제 수정 파일/이유/테스트 실행 명령·결과/남은 문제 보고. ADR/미결정/인수인계 갱신. 사용자 승인 범위를 넘어서는 변경 전에 정지.

## 2. 문서 운영

- `Docs/Handover.md`는 인수인계 원본(`PastFuture_Final_Handover_v0.3_2026-11-24.md`, sha256 `0f3c9437…af9a`)의 사본이다. 내용을 고치지 않는다. 진행 상황과 새 결정은 아래 문서에 기록한다.
  - 진행 상황, 실제 실행 근거, 미결정: [ProjectStatus.md](ProjectStatus.md)
  - 새 결정: [ADR/](ADR/README.md) (ADR-563부터. 001~562는 Handover 부록 B)
- ADR 상태: Proposed / Accepted / PendingValidation / Validated / Superseded / Rejected. 기존 번호 재사용 금지. 원문이 없는 ADR-142~357에 제목이나 내용을 지어내지 않는다.
- 스토리·콘텐츠 ID와 확정 규칙은 GDD(Handover 부록 A)를, 기술 정책은 Handover 본문을 우선한다. 충돌하면 임의로 바꾸지 말고 ProjectStatus.md의 미결정 목록에 적는다.

## 3. 어셈블리와 계층 (ADR-479, ADR-480, ADR-563, ADR-568)

| Assembly | 경로 | 허용 참조 | 엔진 참조 |
|---|---|---|---|
| `PastFuture.Domain` | `Assets/PastFuture/Scripts/Domain/` | 없음 | 금지 |
| `PastFuture.Application` | `Assets/PastFuture/Scripts/Application/` | Domain | 금지 |
| `PastFuture.Infrastructure` | `Assets/PastFuture/Scripts/Infrastructure/` | Application, Domain | 허용 |
| `PastFuture.UnityAdapters` | `Assets/PastFuture/Scripts/UnityAdapters/` | Application, Domain | 허용 |
| `PastFuture.Presentation` | `Assets/PastFuture/Scripts/Presentation/` | Application, Domain, UnityAdapters | 허용 |
| `PastFuture.Bootstrap` | `Assets/PastFuture/Scripts/Bootstrap/` | 위 런타임 계층 전부 | 허용 |
| `PastFuture.Editor` | `Assets/Editor/` | 런타임 계층 (Editor 전용) | 허용 |
| `PastFuture.Tests.EditMode` | `Assets/Tests/EditMode/` | 런타임 계층 + NUnit/TestRunner (Editor 전용) | 허용 |

- 금지: 아래 계층→위 계층 참조, Infrastructure↔UnityAdapters 상호 참조, UnityAdapters→Presentation 참조. 연결은 Bootstrap에서 한다.
- 모든 런타임 asmdef는 `autoReferenced: false`, 이름 참조(`useGUIDs` 미사용). Inspector에서 참조를 추가하면 GUID 형식으로 저장될 수 있으니 이름 형식으로 고친다.
- Unity 패키지 참조(Input System, AI Navigation, TextMeshPro, URP 등)는 실제로 쓰는 코드가 생길 때 추가한다.
- 새 `PastFuture.*` asmdef를 추가하거나 참조를 바꾸면 `Assets/Tests/EditMode/Architecture/AssemblyBoundaryTests.cs`의 `AllowedProjectReferences`도 같은 변경에서 갱신한다. 이 표를 넓히는 것은 아키텍처 변경이므로 사용자 승인이 필요하다.
- 모든 게임 코드는 asmdef 안에 둔다. `Assets/` 바로 아래의 asmdef 없는 스크립트(Assembly-CSharp)를 만들지 않는다.

## 4. 이름 충돌 (ADR-564)

`PastFuture.*` 네임스페이스 안에서는 bare `Application`이 `PastFuture.Application` 네임스페이스로, bare `Editor`가 `PastFuture.Editor` 네임스페이스로 해석된다. Unity 타입은 항상 전체 이름으로 쓴다.

```csharp
var path = UnityEngine.Application.persistentDataPath;   // O
public sealed class FooInspector : UnityEditor.Editor { } // O
var path = Application.persistentDataPath;                // X (컴파일 오류)
```

## 5. 테스트

- 상태 표기: Passed / Failed / Skipped / Blocked / NotRun. 실행 로그가 없으면 NotRun이다.
- Handover에 대응하는 Test ID가 있는 테스트는 이름을 그 ID로 시작한다 (예: `UNITY_T001_...`). 원문에 의미가 없는 Test ID에 새 의미를 배정하지 않는다.
- 대응하는 Test ID가 없는 테스트는 접두사 없이 `대상_조건_기대결과` 형식으로 짓고, ProjectStatus.md 테스트 기록에 "Handover ID 없음"으로 적는다. 임시 ID를 만들지 않는다 (ADR-569).
- EditMode 실행 (Unity Editor가 같은 프로젝트를 열고 있으면 실패하므로 먼저 닫는다):

```bash
"/Applications/Unity/Hub/Editor/6000.3.29f1/Unity.app/Contents/MacOS/Unity" -batchmode -nographics -projectPath /Users/yujin/Projects/PastFuture -runTests -testPlatform EditMode -testResults TestResults/editmode.xml -logFile TestResults/editmode.log
```

- 결과는 `TestResults/`에 남고 Git에서 제외된다. 보고할 때는 XML의 total/passed/failed와 exit code를 함께 적는다.
- PlayMode 테스트 어셈블리는 M1에서 만든다 (ADR-565).
- 순수 .NET headless 실행 (Unity Editor가 열려 있어도 된다. SDK는 `Tools/Headless/global.json`이 10.0.x로 고정, ADR-570):

```bash
dotnet test /Users/yujin/Projects/PastFuture/Tools/Headless/PastFuture.Tests.Headless/PastFuture.Tests.Headless.csproj
```

- headless 프로젝트는 Domain/Application의 Unity 소스를 그대로 컴파일한다. 소스를 복사하지 않는다. 엔진 무관 asmdef를 새로 만들면 대응하는 `Tools/Headless/*.Headless.csproj`도 같은 변경에서 추가한다.

## 6. Git

- 커밋과 push는 사용자가 요청할 때만 한다. 브랜치는 `main`; 짧은 `feat/`, `fix/`, `test/`, `content/`, `refactor/`, `experiment/`, `release/` 브랜치는 후보 (Handover 16-18).
- 한 커밋에 하나의 논리 변경. 메시지는 `type(scope): summary` 형식, 본문에 근거 ADR과 실제 실행한 검증을 적는다.
- `.meta` 파일은 항상 원본 파일과 함께 커밋한다. Unity가 만든 `.meta`를 손으로 만들거나 GUID를 바꾸지 않는다.
- 대형 바이너리는 `.gitattributes`의 LFS 규칙을 따른다. 이미지(`.png`, `.jpg` 포함)도 LFS 대상이다 (ADR-566).
- 제외 대상은 `.gitignore`에 정의되어 있다: `Library/`, `Temp/`, `Logs/`, `UserSettings/`, 빌드 출력, `TestResults/`, 실제 저장 데이터, 비밀 정보.

## 7. Manual DI와 Scope (ADR-569, Handover 16-1·16-17)

- 진입점: `Assets/PastFuture/Scenes/Bootstrap.unity`(Build 0번)의 `GameBootstrapper`가 `CompositionRoot`를 한 번 만들고, 파괴될 때 해제한다.
- 생성자 주입만 쓴다. Service Locator, DI 컨테이너, static singleton, 전역 가변 static을 쓰지 않는다. 예외는 `GameBootstrapper`의 감지용 static 하나이며 `SubsystemRegistration`에서 초기화한다.
- 수명이 있는 서비스는 `CompositionRoot`만 생성하고 연결한다. 생성 순서는 16-17의 Bootstrap Scene → App registry/save/profile → menu → Session → Loop → Scene → bindings → simulation을 따른다.
- MonoBehaviour는 의존성을 명시적 `Initialize(...)`로 받는다. `FindObjectOfType`, 싱글톤 `Instance` 같은 조회로 얻지 않는다. WorldState를 직접 쓰지 않는다 (ADR-484).
- 해제가 필요한 객체는 소유 `LifetimeScope`에 `Register`한다. 비동기 작업은 그 scope의 `Cancellation`을 받는다. scope를 해제하면 token 취소 → 자식 scope(최신 것부터) → 등록 항목(역순) 순서로 정리된다.
- `LifetimeScope`는 thread-safe가 아니다. main thread에서만 생성, 등록, 해제한다.
- Build 씬 목록은 `Assets/Editor/Build/BuildSceneList.cs`의 `ScenePaths`를 고친 뒤 batchmode로 적용한다 (Editor를 닫고 실행. Editor가 열려 있으면 메뉴 PastFuture/Build/Apply Build Scene List). `EditorBuildSettings.asset`을 손으로 고치지 않는다.

```bash
"/Applications/Unity/Hub/Editor/6000.3.29f1/Unity.app/Contents/MacOS/Unity" -batchmode -nographics -projectPath /Users/yujin/Projects/PastFuture -executeMethod PastFuture.Editor.Build.BuildSceneList.Apply -quit -logFile TestResults/buildscenes.log
```
