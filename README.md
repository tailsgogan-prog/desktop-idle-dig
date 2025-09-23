# Desktop Idle Dig

## 개요
- Motherload를 레퍼런스로 한 채굴형 싱글 플레이 게임.
- Unity 6000 LTS 이상의 URP 프로젝트를 기반으로 개발.
- 재미 검증 전에 핵심 매커니즘(이동, 채굴, 자원 흐름) 검증을 우선.

## 플랫폼 및 배포
- 기본 타깃: PC (Windows/macOS).
- 웹 배포: WebGL 빌드로 앱인토스(Appintos) 업로드를 목표로 함.
- WebGL 호환성을 고려하여 OpenGLES3 사용, 텍스처 압축(ETC/ASTC) 및 메모리 사용량을 초기부터 점검.

## 개발 환경 및 도구
- Unity 6000+ URP 템플릿.
- VS Code, Git 기반 1인 개발.
- Localization 패키지를 이용한 한국어/영어 UI 지원.

## 핵심 레퍼런스 및 차별화 아이디어
- Motherload의 연료 관리 → 채굴 → 판매 → 업그레이드 루프 유지.
- 독창 요소 초안
  - 특수 자원(온도, 방사능 등)으로 플레이 변주.
  - 스토리/미션 기반 진행 구조.
  - 세이브 슬롯 + 자동저장 지원.

## 리소스 계획
- 아트/사운드: Unity Asset Store 및 외부 리소스 활용, 라이선스 우선 확인.
- 필요한 외부 툴: 도트 그래픽 편집기(예: Aseprite) 검토.

## 일정 및 마일스톤(2주)
- **1주차**: URP 프로젝트 구성, 이동/굴착 프로토타입, 기본 인벤토리.
- **2주차**: 연료·기지 상호작용, 판매/업그레이드 루프, 세이브 시스템 초안, WebGL 드라이런 빌드.

## 추가 고려 사항
- 세이브 시스템: WebGL 환경에서는 LocalStorage 사용 제한 고려.
- 지형 데이터: 타일/청크 구조로 확장성 확보.
- 최적화: WebGL의 2GB 메모리 제한, 멀티스레딩 제약, 네트워크 요청 제한 사항 확인.

## 문서
- [`docs/project-setup-checklist.md`](docs/project-setup-checklist.md): 초기 셋업 체크리스트.
- [`docs/roadmap.md`](docs/roadmap.md): 2주 로드맵과 우선순위 작업.
