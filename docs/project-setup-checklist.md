# 프로젝트 초기 셋업 체크리스트

Unity URP 프로젝트를 실제 개발 환경에 맞춰 구성하기 위한 체크리스트입니다. Motherload 모작을 위한 핵심 루프 구현 전에 기본 기반을 다지는 것을 목표로 합니다.

## 1. 프로젝트 생성 및 기본 설정
- [ ] Unity Hub에서 **6000 LTS 이상 + URP 템플릿**으로 프로젝트 생성 여부 확인.
- [ ] 프로젝트 이름: `DesktopIdleDig` (혹은 최종 결정된 이름)으로 통일.
- [ ] Version Control 탭에서 **Visible Meta Files** 및 **Force Text** 저장 형식을 적용.
- [ ] `Project Settings > Editor`에서 Asset Serialization을 `Force Text`로 재확인.
- [ ] `Project Settings > Player`
  - [ ] `Company/Product Name` 설정.
  - [ ] `Color Space`를 `Linear`로 유지.
  - [ ] `Scripting Backend`는 PC 대상에 대해 `IL2CPP`, WebGL은 기본값 사용.
  - [ ] `Auto Graphics API` 비활성화 후 PC에서는 `DirectX11/Metal`, WebGL은 `OpenGLES3`만 남김.

## 2. URP 및 렌더링 준비
- [ ] URP Renderer에서 2D/3D 혼합 시 필요한 Renderer Feature(예: 2D Renderer Data) 점검.
- [ ] `Quality Settings`에서 URP용 Quality Level 정리(PC용 vs WebGL용 프로파일 분리).
- [ ] 라이팅, 그림자, 포스트 프로세싱 최소화 프로파일 생성(WebGL 대비).

## 3. 패키지 및 툴링
- [ ] `Localization` 패키지 설치 후 한국어/영어 Locale 생성.
- [ ] `Input System`(필요 시) 활성화 및 Player Input 설정.
- [ ] `Cinemachine`, `Addressables` 등 필요한 패키지 선별 후 설치.
- [ ] 디버깅용 `Unity Profiler`, `Frame Debugger` 활용 방법 문서화.

## 4. Git 및 협업 준비
- [ ] `.gitignore`에 Unity 기본 항목 + WebGL 빌드 산출물 제외 규칙 추가.
- [ ] Git LFS 여부 검토(대형 에셋 사용 시).
- [ ] 주요 폴더 구조 설계 및 README에 반영.
- [ ] 초기 커밋 태그(예: `setup`) 생성하여 기준점 확보.

## 5. WebGL 빌드 환경 점검
- [ ] `Player Settings > Publishing Settings`에서 `Compression Format`을 `Gzip` 또는 `Brotli`로 설정.
- [ ] `Data Caching` 비활성화 여부 확인(앱인토스 요구사항에 맞춤).
- [ ] 메모리 제한(2GB)을 넘지 않도록 `WebGL Memory Size` 초기값(512MB~1024MB) 설정 후 테스트.
- [ ] 샘플 씬으로 빌드 테스트, 앱인토스 업로드 흐름 드라이런.

## 6. 프로젝트 구조 초안
- [ ] `Assets/Scripts`, `Assets/Art`, `Assets/Audio`, `Assets/Prefabs`, `Assets/Scenes` 등 폴더 생성.
- [ ] `Assets/Settings` 폴더에 프로젝트 ScriptableObject 설정 모음.
- [ ] `Assets/Localization` 폴더에서 Table/Entries 생성.
- [ ] 핵심 데이터 구조(예: 타일/청크 ScriptableObject) 설계 문서화.

## 7. 세이브 시스템 준비
- [ ] PC: JSON 기반 세이브 파일 저장 경로 결정(예: `Application.persistentDataPath`).
- [ ] WebGL: LocalStorage 저장 방식 조사 및 용량 제한 확인.
- [ ] 세이브 슬롯 구조(슬롯 수, 자동저장 트리거) 정의.
- [ ] 테스트용 Mock 데이터로 저장/로드 사이클 검증 계획 수립.

## 8. 향후 액션 아이템
- [ ] 1주차 목표(이동, 채굴, 인벤토리)를 위한 핵심 클래스 다이어그램 초안 작성.
- [ ] Motherload 원작의 자원/업그레이드 테이블 리서치.
- [ ] 플레이 루프 프로토타입을 위한 타임라인(14일) 상세 작업 배분.

필요한 항목을 체크하면서 진행 상황을 수시로 업데이트해 주세요.
