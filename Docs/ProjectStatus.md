# Past, Future — 진행 현황

- 갱신: 2026-11-26
- 현재 단계: 로드맵 17 핵심 게임플레이 프로토타입 / **M0 개발 환경 종료** (실제 Windows 실행은 이월, 2026-11-26 사용자 결정) → **M1 1인칭 기본 (진행 중)**
- 규칙: 실제 실행 근거(명령, 결과, 커밋)가 있는 항목만 완료로 적는다. 근거가 없으면 NotRun 또는 미확인이다.

## 1. 확인된 개발 환경 (2026-11-24~25 실측)

| 항목 | 값 |
|---|---|
| 장비 | MacBook Air (Mac16,12), Apple M4 10코어(P4+E6), RAM 16GB, macOS 26.3.1 |
| 저장 공간 | 내장 SSD 228GB, 남은 공간 약 31GB (2026-11-25 23:30, .NET SDK·VS Code·Windows 모듈 설치 후), 외장 SSD 없음 |
| Unity | Hub 4.0.1, Editor 6000.3.29f1 (revision 96d2fe17529c, arm64), 라이선스 활성 상태로 실행됨 |
| Unity 모듈 | Mac Standalone(Mono), Windows Build Support (Mono) (2026-11-25 설치, 978MB). Windows IL2CPP는 Mac용 모듈 목록에 없음 |
| 템플릿 | Universal 3D (`com.unity.template.3d-cross-platform` 17.0.14), URP 17.3.0 |
| 프로젝트 설정 | Force Text, Visible Meta Files, Input System only, Linear, API .NET Standard 2.1 |
| Git | 2.50.1 (Apple), Git LFS 3.8.1 (Homebrew) |
| 원격 | GitHub private `TirTir/PastFuture`, 브랜치 `main` |
| IDE | Visual Studio Code 1.147.0 (Homebrew cask), 확장 Unity `visualstudiotoolsforunity.vstuc` 1.3.2, C# Dev Kit 3.40.215, C# 2.160.4. Unity External Script Editor = VS Code (2026-11-25 사용자 선택) |
| .NET SDK | 10.0.404 (LTS, 사용자 선택)와 11.0.100 (Homebrew `dotnet-sdk@10`이 `dotnet-sdk`에 의존해 함께 설치). 기본 `dotnet`은 11.0.100. Unity 컴파일에는 쓰이지 않음 |
| Windows 검증 장비 | 없음 (2026-11-26 사용자 답). 실제 Windows 실행은 장비 확보 때까지 이월 |

## 2. M0 완료 기준 (Handover 4절)

| 기준 | 상태 | 근거 |
|---|---|---|
| Mac 사양, disk, Unity patch 확인 | 완료 | 1절 실측 |
| URP 프로젝트 실제 실행 | 완료 | `/Users/yujin/Projects/PastFuture`에서 Editor 실행, Editor.log 컴파일 오류 0건 |
| private Git + LFS, 첫 commit/clone | 완료 | 커밋 1237ee3 push, 새 clone에서 HEAD·파일 63개·LFS 파일 sha256 일치 |
| asmdef, Domain UnityEngine 분리 | 완료 | 커밋 af4dd26, ADR-563/568 |
| EditMode smoke | 완료 | 2026-11-25 batchmode EditMode 5/5 Passed, exit 0 (커밋 af4dd26). 최근 실행 14/14 Passed (3절) |
| 순수 C# smoke | 완료 | 커밋 a450feb, ADR-570. `Tools/Headless`에서 Domain/Application 소스를 Unity 없이 `netstandard2.1`로 빌드, `dotnet test` 2/2 Passed (SDK 10.0.404), 음성 확인(UnityEngine 사용 시 빌드 실패) |
| docs | 완료 | 커밋 5458754: Handover.md 원본 사본, 이 문서, DevelopmentRules.md, ADR/, CLAUDE.md, README.md |
| 최소 씬, Manual DI (Handover 4.1-3) | 완료 | 커밋 a73aa29, ADR-569. Bootstrap 씬(Build 0번), `GameBootstrapper`, `CompositionRoot`, `LifetimeScope`. EditMode 14/14 Passed, Editor 수동 Play/Stop 2회 로그 확인. UNITY_T003/T008은 2026-11-26 M1 PlayMode 테스트에서 Passed (4절) |
| Windows 경로 확인 | Mac 범위 완료, 실제 실행 이월 | 커밋 7646731, ADR-571. Mac에서 Windows Mono Development Build 성공 (프로젝트 사본, exit 0, errors 0, PE32+ x86-64). 2026-11-26 원래 프로젝트 Editor에서 `PastFuture.Editor.dll` 재컴파일 성공 (`BuildWindows.cs` 포함, CS 오류·경고 0건). 실제 Windows 실행(UNITY_T049)은 Windows 장비가 없어 NotRun, 장비 확보 때까지 이월 (2026-11-26 사용자 결정) |

## 3. 테스트 실행 기록

| Test ID | 테스트 | 최근 결과 | 날짜 | 커밋 |
|---|---|---|---|---|
| UNITY_T001 | `UNITY_T001_EngineFreeAssembly_DoesNotReferenceUnity("PastFuture.Domain")` | Passed | 2026-11-25 | af4dd26 |
| UNITY_T001 | `UNITY_T001_EngineFreeAssembly_DoesNotReferenceUnity("PastFuture.Application")` | Passed | 2026-11-25 | af4dd26 |
| UNITY_T001 | `UNITY_T001_EngineFreeAsmdef_HasNoEngineReferencesFlag` | Passed | 2026-11-25 | af4dd26 |
| UNITY_T002 | `UNITY_T002_ProjectAsmdefs_FollowLayerRules` | Passed | 2026-11-25 | af4dd26 |
| UNITY_T002 | `UNITY_T002_ProjectAsmdefs_HaveNoCycles` | Passed | 2026-11-25 | af4dd26 |
| Handover ID 없음 | `LifetimeScope_DisposesRegisteredItemsInReverseOrder` | Passed | 2026-11-25 | a73aa29 |
| Handover ID 없음 | `LifetimeScope_DisposesChildrenBeforeOwnItems` | Passed | 2026-11-25 | a73aa29 |
| Handover ID 없음 | `LifetimeScope_CancelsOwnAndChildTokensOnDispose` | Passed | 2026-11-25 | a73aa29 |
| Handover ID 없음 | `LifetimeScope_DisposingChild_DoesNotCancelParent_AndDetachesChild` | Passed | 2026-11-25 | a73aa29 |
| Handover ID 없음 | `LifetimeScope_DisposeTwice_DisposesItemsOnce` | Passed | 2026-11-25 | a73aa29 |
| Handover ID 없음 | `LifetimeScope_RegisterOrCreateChildAfterDispose_Throws` | Passed | 2026-11-25 | a73aa29 |
| Handover ID 없음 | `LifetimeScope_RegisterNull_Throws` | Passed | 2026-11-25 | a73aa29 |
| Handover ID 없음 | `LifetimeScope_ThrowingItem_StillDisposesOthers_AndAggregatesErrors` | Passed | 2026-11-25 | a73aa29 |
| Handover ID 없음 | `CompositionRoot_CreatesApplicationScope_AndDisposesIt` | Passed | 2026-11-25 | a73aa29 |
| Handover ID 없음 | `PlayerMotorMathTests` 8개 (걷기·달리기 속도, 대각선 제한, 감속, 수직 입력 무시, 30/60 FPS 동일, 접지, 낙하) | Passed | 2026-11-26 | 9282b9e |
| Handover ID 없음 | `FirstPersonLookStateTests` 4개 (pitch 방향·제한, invertY, yaw 감기, 초기 pitch 읽기) | Passed | 2026-11-26 | 9282b9e |
| Handover ID 없음 (PlayMode) | `M1TestRoomMovementTests` 10개 (ADR-572 ValidationEvidence) | Passed | 2026-11-26 | 9282b9e |
| Handover ID 없음 (PlayMode) | `FirstPersonRigInputTests` 7개 (가상 키보드·마우스 → rig → motor) | Passed | 2026-11-26 | 15c6b8c |
| Handover ID 없음 (headless) | `EngineFreeAssemblies_LoadWithoutUnity_AndReferenceNoUnityAssembly` | Passed | 2026-11-25 | a450feb |
| Handover ID 없음 (headless) | `LifetimeScope_RunsHeadless_DisposesChildThenItemsAndCancels` | Passed | 2026-11-25 | a450feb |
| UNITY_T003 | `UNITY_T003_BootstrapSceneLoadedTwice_KeepsSingleCompositionRoot` (PlayMode) | Passed | 2026-11-26 | c19ebf3 |
| UNITY_T049 | Windows Development Build → 실제 실행/Profiler | NotRun (빌드만 성공, Windows 장비 없음, 이월) | - | - |
| UNITY_T008 | `UNITY_T008_BootstrapRestartedInSameDomain_CreatesFreshRootAndDisposesPrevious` (PlayMode, 같은 도메인 재시작 범위) | Passed | 2026-11-26 | c19ebf3 |

- 실행 명령은 [DevelopmentRules.md](DevelopmentRules.md) 5절. 결과 파일 `TestResults/editmode.xml`은 Git에서 제외된다.
- 위반을 일부러 넣은 실행(Presentation→Infrastructure, Application 엔진 참조 허용)에서 `FollowLayerRules`, `HasNoEngineReferencesFlag`가 Failed. 되돌린 뒤 5/5 Passed.
- `HaveNoCycles`의 실패 사례는 실행하지 못했다 (Unity가 순환 asmdef를 컴파일 단계에서 거부).
- headless: 2026-11-25 `dotnet test` ([DevelopmentRules.md](DevelopmentRules.md) 5절) 2/2 Passed, exit 0, 빌드 경고 0건.
- UNITY_T003/T008: 2026-11-26 PlayMode 자동 테스트 2/2 Passed (ADR-565). UNITY_T008은 Domain Reload를 끈 Play 반복과 같은 조건(같은 도메인에서 재시작)만 다룬다. Editor Play/Stop 버튼 반복은 자동화하지 않았다. 음성 확인(중복 검사·해제 제거)에서 둘 다 Failed, 되돌린 뒤 Passed.
- 최근 실행: 2026-11-26 scratchpad 사본에서 batchmode EditMode 26/26 Passed. PlayMode 23개 중 21 Passed, 2 Skipped. Skipped 2개는 `testables`로 포함된 Input System 패키지 자체 테스트로, 패키지가 Ignore로 표시한 것이다. exit 0, 컴파일 경고·오류 0건. headless 2/2 Passed. 원래 프로젝트의 Editor가 열려 있어 사본에서 실행했다.
- 원래 프로젝트 Editor: 2026-11-26 새 코드 컴파일(오류·경고 0)과 `M1_TestRoom` Play(콘솔 오류 없음)를 확인했다 (ADR-572).
- 위 표에 없는 Handover의 모든 Test ID는 NotRun이다.

## 4. M1 진행 (Handover 4절 M1)

| 단계 | 상태 | 근거 |
|---|---|---|
| 1. PlayMode 테스트 어셈블리, UNITY_T003/T008 | 완료 (c19ebf3) | ADR-565. PlayMode 2/2, EditMode 14/14 Passed (2026-11-26, 사본) |
| 2. 방·복도 테스트 씬, 입력, `CharacterController` 이동, 카메라 | 자동 테스트 완료 (9282b9e, 입력 활성 상태 수정 ab66634, 입력 연결 테스트 15c6b8c), 조작감 NotRun | ADR-572. EditMode 26/26, PlayMode 12/12 Passed (2026-11-26, 사본). Editor에서 직접 플레이해 보는 확인은 사용자 몫 |
| 3. Entity ID ↔ GameObject 연결 등록부 (UNITY_T024) | NotStarted | - |
| 4. 물건 줍기·놓기 Command | NotStarted | 착수 전 Domain 저장소 범위 확인 필요 (WorldState는 M2) |
| 5. 물리 결과(`PhysicsObservation`) 제출 | NotStarted | - |
| 6. 시간 배율 초기 측정 | NotStarted | - |

## 5. 커밋 이력

| 커밋 | 내용 |
|---|---|
| 1237ee3 | Unity 6.3 LTS URP 초기 프로젝트, `.gitignore`, `.gitattributes` |
| c1692be | 템플릿 예제와 미사용 패키지 정리 (ADR-567) |
| af4dd26 | 계층별 asmdef 골격과 경계 테스트 (ADR-563, ADR-568) |
| 5458754 | Handover 원본 사본, 개발 규칙, 현황, ADR-563~568 |
| a73aa29 | Bootstrap 씬, LifetimeScope, CompositionRoot, Build 씬 목록 스크립트 (ADR-569) |
| 7284c85 | ADR-569, Manual DI 규칙, M0 현황 |
| ccd2f5a | VS Code 작업 공간 설정, IDE/.NET SDK 기록 |
| a450feb | 순수 .NET headless smoke (ADR-570) |
| 1d4e294 | ADR-570, headless 실행 규칙, M0 현황 |
| 7646731 | Windows Mono Development Build 메서드 (ADR-571) |
| 90882ec | ADR-571, Windows 빌드 규칙, M0 현황 |
| c19ebf3 | M1: PlayMode 테스트 어셈블리, UNITY_T003/T008 (ADR-565) |
| 9282b9e | M1: 1인칭 이동·카메라·입력, 테스트 방 생성기와 씬 (ADR-572) |
| fc3ce9c | M0 종료, ADR-572, M1 1·2단계 기록 |
| ab66634 | rig 입력 활성 상태를 직접 관리 (커서 잠금 의존 제거) |
| 15c6b8c | 입력 연결 PlayMode 테스트 (`InputTestFixture`, manifest `testables`) |

## 6. 이번 단계에서 정한 사항

ADR-563~571 ([ADR/README.md](ADR/README.md)).

## 7. 미결정 / 다음 작업

- 이월: 실제 Windows 장비에서 빌드 실행 확인 (UNITY_T049). Windows 장비 없음 (2026-11-26). 장비를 확보하면 M1 이후 어느 시점에든 실행한다
- 다음 작업: M1 3단계 (4절 표). M1_TestRoom 직접 플레이로 조작감 확인 (사용자)
- 2026-11-26 전날 실행한 Unity Editor 프로세스가 "Hold on..." 상태로 멈춘 채 남아 있었다. 사용자가 새로 연 Editor와 같은 프로젝트를 동시에 열고 있었고, 사용자 승인으로 종료했다. Editor를 다시 켤 때는 이전 프로세스가 끝났는지 확인한다
- Play 시작 직후 Game 화면 밖에 있던 마우스의 움직임이 시점 회전으로 들어갈 수 있음 (ADR-572). 조작감 확인 때 처리 결정
- 템플릿 `Assets/InputSystem_Actions.inputactions`: 게임 입력은 `PastFutureControls`로 옮겼다. 템플릿 파일은 아직 project-wide actions로 등록되어 있다. 삭제하거나 등록을 해제할지 미정 (ADR-567, ADR-572)
- 점프·앉기·게임패드·리바인딩 여부 미정 (원문에 근거 없음, ADR-572)
- 2026-11-26 Editor.log에 `Unexpected transport error from import worker 1 (possible crash)` 1건. 이후 컴파일과 import는 정상. 원인 미확인, 조치하지 않음
- Windows 빌드 시 Unity가 tracked 설정 파일을 다시 저장할 수 있음 (ADR-571 Consequences). 특히 `UnityConnectSettings.asset`의 `m_Enabled` 0→1은 사본 첫 import에서 관찰됨. 원래 프로젝트에서 같은 일이 생기면 커밋 전 검토
- Windows Mono/IL2CPP 비교와 D3D11/D3D12 선택은 Windows 장비 확보 후 (Handover 16-17)
- `dotnet build PastFuture.Tests.EditMode.csproj`(2026-11-25)에서 CS0649 경고 1건: `AssemblyBoundaryTests.AsmdefData.noEngineReferences` (JsonUtility가 채우는 필드). Unity 컴파일에서는 경고 0건. 수정하지 않음
- Bootstrap이 아닌 씬에서 Editor Play할 때 Bootstrap 자동 로드 여부 미정 (ADR-569)
- `Assets/Scenes/SampleScene.unity`는 Build 목록에서만 빠졌고 파일은 남아 있다. 삭제 여부 미정 (ADR-567)
- `productName`(`pastfuture`), `companyName`(`DefaultCompany`) 최종 이름 미정
- Mobile 품질 레벨 정리는 URP 품질 프로필 결정 때 (ADR-567)
- UnityYAMLMerge 연결 여부 (ADR-566)
- 저장 데이터 실제 경로 (16-16 미정, `.gitignore`의 `LocalSaves/`는 임시 이름)
- GitHub 인증이 `credential.helper=store`(평문 저장) 방식이다. 키체인 전환 여부는 사용자 결정
- Unity Cloud에 프로젝트 ID가 등록되어 있음 (`cloudEnabled: 0`, 서비스 비활성)
- 전체 미결정 목록은 Handover 3절과 부록 A 17절
