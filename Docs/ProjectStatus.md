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
| IDE / .NET SDK | 미설치, 선택 미정 |
| Windows 검증 장비 | 미확인 |

## 2. M0 완료 기준 (Handover 4절)

| 기준 | 상태 | 근거 |
|---|---|---|
| Mac 사양, disk, Unity patch 확인 | 완료 | 1절 실측 |
| URP 프로젝트 실제 실행 | 완료 | `/Users/yujin/Projects/PastFuture`에서 Editor 실행, Editor.log 컴파일 오류 0건 |
| private Git + LFS, 첫 commit/clone | 완료 | 커밋 1237ee3 push, 새 clone에서 HEAD·파일 63개·LFS 파일 sha256 일치 |
| asmdef, Domain UnityEngine 분리 | 완료 | 커밋 af4dd26, ADR-563/568 |
| EditMode smoke | 완료 | 2026-11-25 batchmode EditMode 5/5 Passed, exit 0 |
| 순수 C# smoke | 해석 확인 필요 | 엔진 참조가 없는 어셈블리를 EditMode에서 검증하는 것으로 대신함. `dotnet test` headless는 NotRun (.NET SDK 미설치) |
| docs | 완료 | `docs:` 커밋 (2026-11-25): Handover.md 원본 사본, 이 문서, DevelopmentRules.md, ADR/, CLAUDE.md, README.md |
| 최소 씬, Manual DI (Handover 4.1-3) | 미착수 | Bootstrap 설계안 필요 |
| Windows 경로 확인 | 미착수 | Windows Build Support 미설치, 검증 장비 미확인 |

## 3. 테스트 실행 기록

| Test ID | 테스트 | 최근 결과 | 날짜 | 커밋 |
|---|---|---|---|---|
| UNITY_T001 | `UNITY_T001_EngineFreeAssembly_DoesNotReferenceUnity("PastFuture.Domain")` | Passed | 2026-11-25 | af4dd26 |
| UNITY_T001 | `UNITY_T001_EngineFreeAssembly_DoesNotReferenceUnity("PastFuture.Application")` | Passed | 2026-11-25 | af4dd26 |
| UNITY_T001 | `UNITY_T001_EngineFreeAsmdef_HasNoEngineReferencesFlag` | Passed | 2026-11-25 | af4dd26 |
| UNITY_T002 | `UNITY_T002_ProjectAsmdefs_FollowLayerRules` | Passed | 2026-11-25 | af4dd26 |
| UNITY_T002 | `UNITY_T002_ProjectAsmdefs_HaveNoCycles` | Passed | 2026-11-25 | af4dd26 |

- 실행 명령은 [DevelopmentRules.md](DevelopmentRules.md) 5절. 결과 파일 `TestResults/editmode.xml`은 Git에서 제외된다.
- 위반을 일부러 넣은 실행(Presentation→Infrastructure, Application 엔진 참조 허용)에서 `FollowLayerRules`, `HasNoEngineReferencesFlag`가 Failed. 되돌린 뒤 5/5 Passed.
- `HaveNoCycles`의 실패 사례는 실행하지 못했다 (Unity가 순환 asmdef를 컴파일 단계에서 거부).
- 위 표에 없는 Handover의 모든 Test ID는 NotRun이다.

## 4. 커밋 이력 (M0)

| 커밋 | 내용 |
|---|---|
| 1237ee3 | Unity 6.3 LTS URP 초기 프로젝트, `.gitignore`, `.gitattributes` |
| c1692be | 템플릿 예제와 미사용 패키지 정리 (ADR-567) |
| af4dd26 | 계층별 asmdef 골격과 경계 테스트 (ADR-563, ADR-568) |

## 5. 이번 단계에서 정한 사항

ADR-563~568 ([ADR/README.md](ADR/README.md)).

## 6. 미결정 / 다음 작업

- M0 남은 작업: Bootstrap 최소 씬과 Manual DI, Windows Build Support 설치와 Windows 경로 확인, IDE 선택, "순수 C# smoke" 해석 확인
- `productName`(`pastfuture`), `companyName`(`DefaultCompany`) 최종 이름 미정
- Mobile 품질 레벨 정리는 URP 품질 프로필 결정 때 (ADR-567)
- UnityYAMLMerge 연결 여부 (ADR-566)
- 저장 데이터 실제 경로 (16-16 미정, `.gitignore`의 `LocalSaves/`는 임시 이름)
- GitHub 인증이 `credential.helper=store`(평문 저장) 방식이다. 키체인 전환 여부는 사용자 결정
- Unity Cloud에 프로젝트 ID가 등록되어 있음 (`cloudEnabled: 0`, 서비스 비활성)
- 전체 미결정 목록은 Handover 3절과 부록 A 17절
