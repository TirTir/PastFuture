# Past, Future — 진행 현황

- 갱신: 2026-11-25
- 현재 단계: 로드맵 17 핵심 게임플레이 프로토타입 / **M0 개발 환경 (진행 중)**
- 규칙: 실제 실행 근거(명령, 결과, 커밋)가 있는 항목만 완료로 적는다. 근거가 없으면 NotRun 또는 미확인이다.

## 1. 확인된 개발 환경 (2026-11-24~25 실측)

| 항목 | 값 |
|---|---|
| 장비 | MacBook Air (Mac16,12), Apple M4 10코어(P4+E6), RAM 16GB, macOS 26.3.1 |
| 저장 공간 | 내장 SSD 228GB, 남은 공간 약 38GB (2026-11-25), 외장 SSD 없음 |
| Unity | Hub 4.0.1, Editor 6000.3.29f1 (revision 96d2fe17529c, arm64), 라이선스 활성 상태로 실행됨 |
| Unity 모듈 | Mac Standalone(Mono)만 설치. Windows Build Support 미설치 |
| 템플릿 | Universal 3D (`com.unity.template.3d-cross-platform` 17.0.14), URP 17.3.0 |
| 프로젝트 설정 | Force Text, Visible Meta Files, Input System only, Linear, API .NET Standard 2.1 |
| Git | 2.50.1 (Apple), Git LFS 3.8.1 (Homebrew) |
| 원격 | GitHub private `TirTir/PastFuture`, 브랜치 `main` |
| IDE | Visual Studio Code 1.147.0 (Homebrew cask), 확장 Unity `visualstudiotoolsforunity.vstuc` 1.3.2, C# Dev Kit 3.40.215, C# 2.160.4. Unity External Script Editor = VS Code (2026-11-25 사용자 선택) |
| .NET SDK | 10.0.404 (LTS, 사용자 선택)와 11.0.100 (Homebrew `dotnet-sdk@10`이 `dotnet-sdk`에 의존해 함께 설치). 기본 `dotnet`은 11.0.100. Unity 컴파일에는 쓰이지 않음 |
| Windows 검증 장비 | 미확인 |

## 2. M0 완료 기준 (Handover 4절)

| 기준 | 상태 | 근거 |
|---|---|---|
| Mac 사양, disk, Unity patch 확인 | 완료 | 1절 실측 |
| URP 프로젝트 실제 실행 | 완료 | `/Users/yujin/Projects/PastFuture`에서 Editor 실행, Editor.log 컴파일 오류 0건 |
| private Git + LFS, 첫 commit/clone | 완료 | 커밋 1237ee3 push, 새 clone에서 HEAD·파일 63개·LFS 파일 sha256 일치 |
| asmdef, Domain UnityEngine 분리 | 완료 | 커밋 af4dd26, ADR-563/568 |
| EditMode smoke | 완료 | 2026-11-25 batchmode EditMode 5/5 Passed, exit 0 (커밋 af4dd26). 최근 실행 14/14 Passed (3절) |
| 순수 C# smoke | 해석 확인 필요 | 엔진 참조가 없는 어셈블리를 EditMode에서 검증하는 것으로 대신함. `dotnet test` headless는 NotRun (.NET SDK는 설치됨, 테스트 프로젝트 없음) |
| docs | 완료 | 커밋 5458754: Handover.md 원본 사본, 이 문서, DevelopmentRules.md, ADR/, CLAUDE.md, README.md |
| 최소 씬, Manual DI (Handover 4.1-3) | 완료 | 커밋 a73aa29, ADR-569. Bootstrap 씬(Build 0번), `GameBootstrapper`, `CompositionRoot`, `LifetimeScope`. EditMode 14/14 Passed, Editor 수동 Play/Stop 2회 로그 확인. UNITY_T003/T008은 NotRun (M1 PlayMode) |
| Windows 경로 확인 | 미착수 | Windows Build Support 미설치, 검증 장비 미확인 |

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
| UNITY_T003 | Bootstrap → 서비스 한 번만 생성 | NotRun | - | - |
| UNITY_T008 | Editor PlayMode 반복 → static 혼입 없음 | NotRun | - | - |

- 실행 명령은 [DevelopmentRules.md](DevelopmentRules.md) 5절. 결과 파일 `TestResults/editmode.xml`은 Git에서 제외된다.
- 위반을 일부러 넣은 실행(Presentation→Infrastructure, Application 엔진 참조 허용)에서 `FollowLayerRules`, `HasNoEngineReferencesFlag`가 Failed. 되돌린 뒤 5/5 Passed.
- `HaveNoCycles`의 실패 사례는 실행하지 못했다 (Unity가 순환 asmdef를 컴파일 단계에서 거부).
- 최근 실행: 2026-11-25 batchmode EditMode 14/14 Passed, exit 0, 컴파일 경고·오류 0건 (Build 씬 목록 변경 뒤 재실행).
- UNITY_T003/T008: 2026-11-25 Editor 수동 Play/Stop 2회에서 `Composition root created`/`Application scope disposed` 로그가 짝지어 나오고 예외가 없었다. 수동 확인이라 Passed로 적지 않는다. PlayMode 자동 테스트는 M1 (ADR-565).
- 위 표에 없는 Handover의 모든 Test ID는 NotRun이다.

## 4. 커밋 이력 (M0)

| 커밋 | 내용 |
|---|---|
| 1237ee3 | Unity 6.3 LTS URP 초기 프로젝트, `.gitignore`, `.gitattributes` |
| c1692be | 템플릿 예제와 미사용 패키지 정리 (ADR-567) |
| af4dd26 | 계층별 asmdef 골격과 경계 테스트 (ADR-563, ADR-568) |
| 5458754 | Handover 원본 사본, 개발 규칙, 현황, ADR-563~568 |
| a73aa29 | Bootstrap 씬, LifetimeScope, CompositionRoot, Build 씬 목록 스크립트 (ADR-569) |

## 5. 이번 단계에서 정한 사항

ADR-563~569 ([ADR/README.md](ADR/README.md)).

## 6. 미결정 / 다음 작업

- M0 남은 작업: Windows Build Support 설치와 Windows 경로 확인, "순수 C# smoke" 해석 확인
- headless 테스트 프로젝트를 만들 경우 `global.json`으로 SDK 10을 고정할지 미정
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
