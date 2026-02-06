# 접근성 키오스크 음성 출력 시스템 (TTS Kiosk)

Visual Studio 2017, WPF .NET Framework 4.5 기반의 barrier-free 키오스크 애플리케이션

## 프로젝트 개요

시각장애인, 저시력자, 노인 등 모든 사용자가 쉽게 사용할 수 있는 음성 안내 키오스크 시스템입니다. WCAG 2.1 AA 접근성 기준을 준수하며, 한국어 TTS를 지원합니다.

## 주요 기능

### 🔊 음성 출력 (TTS)
- System.Speech.Synthesis 기반 한국어 음성 합성
- 음성 속도 조절 (-10 ~ +10)
- 음성 볼륨 조절 (0 ~ 100)
- 설치된 음성 목록 조회 및 선택
- 비동기 음성 출력
- 음성 중지 기능

### ♿ 접근성 기능
- **고대비 테마**: 흰색/검정 배경 전환 (WCAG 2.1 AA 준수)
- **글씨 크기 조절**: 기본 16pt, 확대 시 최대 32pt
- **키보드 접근성**: 
  - Ctrl+V: 음성 출력
  - Escape: 중지
  - Ctrl+Delete: 텍스트 지우기
  - Tab 순서 논리적 배치
- **화면 낭독기 지원**: AutomationProperties 완벽 구현
- **터치 타겟**: 최소 44x44 픽셀
- **포커스 표시**: 명확한 시각적 피드백

### 🎯 편의 기능
- 빠른 문구 버튼: "안녕하세요", "도움이 필요하신가요?", "감사합니다"
- 환영 메시지 자동 재생
- 여러 줄 텍스트 입력 지원

## 시스템 요구사항

- **OS**: Windows 10 이상
- **IDE**: Visual Studio 2017 이상
- **Framework**: .NET Framework 4.5
- **TTS 음성**: Windows 기본 한국어 음성 (Heami 등)

## 빌드 및 실행

### 1. 솔루션 열기
```bash
VoiceKiosk.sln 파일을 Visual Studio 2017에서 열기
```

### 2. 빌드
```
빌드 > 솔루션 빌드 (Ctrl+Shift+B)
```

### 3. 실행
```
디버그 > 디버깅 시작 (F5)
```

## 한국어 TTS 음성 설치 방법

### Windows 10/11에 한국어 음성 추가하기

1. **설정** 열기 (Win + I)
2. **시간 및 언어** > **음성** 선택
3. **음성 추가** 클릭
4. **한국어** 검색 및 설치
5. 설치 후 애플리케이션 재시작

### 사용 가능한 한국어 음성
- Microsoft Heami (여성 음성)
- 추가 음성은 Windows Store에서 다운로드 가능

## 프로젝트 구조

```
VoiceKiosk.sln
└── SpeechKiosk/
    ├── StartupPoint.xaml          # 애플리케이션 시작점
    ├── KioskInterface.xaml        # 메인 UI
    ├── Audio/
    │   └── VocalEngine.cs         # TTS 엔진 (독창적 구현)
    ├── Brain/
    │   └── PresentationBrain.cs   # 상태 관리 컨트롤러
    ├── Access/
    │   └── UniversalAccessManager.cs  # 접근성 매니저
    ├── Design/
    │   ├── StandardVisuals.xaml   # 기본 테마
    │   └── HighContrastVisuals.xaml  # 고대비 테마
    └── Info/
        └── AssemblyInfo.cs        # 어셈블리 정보
```

## 코드 아키텍처

### VocalEngine (음성 엔진)
- `VocalizeText()`: 텍스트를 음성으로 변환
- `HaltVocalization()`: 음성 출력 중지
- `ModifyPitchLevel()`: 속도 조절
- `ModifyVolumeLevel()`: 볼륨 조절
- `GetAvailableVoices()`: 설치된 음성 목록 반환

### PresentationBrain (프레젠테이션 컨트롤러)
- 상태 관리 및 UI 로직 처리
- ActionCommand 패턴으로 버튼 액션 바인딩
- INotifyPropertyChanged 구현으로 자동 UI 업데이트

### UniversalAccessManager (접근성 관리자)
- AutomationProperties 자동 설정
- 고대비 테마 전환
- 키보드 단축키 등록
- WCAG 색상 대비 검증

## 사용 방법

### 기본 사용
1. 텍스트 입력란에 읽고 싶은 텍스트 입력
2. "🔊 음성출력" 버튼 클릭 (또는 Ctrl+V)
3. 음성으로 텍스트가 재생됨

### 설정 조정
- **속도 슬라이더**: 음성 재생 속도 조절
- **볼륨 슬라이더**: 음성 크기 조절
- **음성 선택**: 드롭다운에서 원하는 음성 선택

### 접근성 설정
- **고대비 버튼**: 고대비 테마 전환
- **A+ / A- 버튼**: 글씨 크기 조절
- **키보드**: Tab으로 이동, Enter/Space로 활성화

### 빠른 문구
- 하단의 빠른 문구 버튼 클릭 시 즉시 음성 출력

## 접근성 기능 상세

### WCAG 2.1 AA 준수 사항
- ✅ 색상 대비율 4.5:1 이상
- ✅ 터치 타겟 최소 44x44px
- ✅ 키보드로 모든 기능 접근 가능
- ✅ 포커스 표시 명확
- ✅ 화면 낭독기 호환

### AutomationProperties 구현
- `AutomationProperties.Name`: 모든 컨트롤에 이름 설정
- `AutomationProperties.HelpText`: 상세 도움말 제공
- `AutomationProperties.LiveSetting`: 동적 콘텐츠 알림

## 기술 스택

- **언어**: C# 5.0
- **UI 프레임워크**: WPF (Windows Presentation Foundation)
- **TTS 엔진**: System.Speech.Synthesis (SAPI 5)
- **디자인 패턴**: Custom Controller Pattern
- **접근성**: UI Automation API

## 라이선스

MIT License

Copyright (c) 2026 Barrier-Free Solutions

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## 기여

접근성 개선 제안 및 버그 리포트는 환영합니다!

## 문의

접근성 관련 문의 또는 기술 지원이 필요하신 경우 이슈를 등록해 주세요.
