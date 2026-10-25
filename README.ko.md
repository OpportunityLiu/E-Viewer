> 다른 언어:
>
> - [English](/README.md)
> - [Simplified Chinese (简体中文)](/README.zh-hans.md)

# E-Viewer

[![CI](https://github.com/OpportunityLiu/E-Viewer/actions/workflows/ci.yml/badge.svg)](https://github.com/OpportunityLiu/E-Viewer/actions/workflows/ci.yml)

Windows 10(버전 1809 이상) 및 Windows 11용 e-hentai.org / ExHentai UWP 클라이언트입니다.

[![](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Images/Screenshots/1.png)](https://github.com/OpportunityLiu/E-Viewer/wiki)
[더 많은 스크린샷](https://github.com/OpportunityLiu/E-Viewer/wiki)

## 배포

[GitHub Releases](https://github.com/OpportunityLiu/E-Viewer/releases)에서 배포 파일을 내려받을 수 있습니다.

[![Github All Releases](https://img.shields.io/github/downloads/OpportunityLiu/E-Viewer/total.svg)](https://github.com/OpportunityLiu/E-Viewer/releases) [![Github Latest Release](https://img.shields.io/github/downloads/OpportunityLiu/E-Viewer/latest/total.svg)](https://github.com/OpportunityLiu/E-Viewer/releases/latest)

먼저 종속성([x86](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/x86.zip) | [x64](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/x64.zip) | [ARM](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/arm.zip) | [ARM64](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/arm64.zip))과 서명(`.cer` 파일)을 설치하고 신뢰하십시오.

`.msix` 파일을 내려받아 설치하십시오. `.appxsym`은 디버깅용 심볼 파일입니다.

웹 페이지 로그인은 Microsoft Edge WebView2 런타임을 사용합니다. Windows 11에는 기본 포함되어 있습니다. Windows 10에서 없는 경우 Evergreen WebView2 런타임을 설치하십시오. 쿠키 로그인은 WebView2가 필요하지 않습니다.

## 자주 묻는 질문

1. [설치 방법](https://github.com/OpportunityLiu/E-Viewer/wiki/How-to-Install)

2. [UWP 앱에서 네트워크 프록시를 사용하는 방법](https://github.com/OpportunityLiu/E-Viewer/wiki/Resolve-Connection-Issues)

3. [키보드 또는 Xbox 컨트롤러 사용자를 위한 팁](https://github.com/OpportunityLiu/E-Viewer/wiki/Tips)

4. 웹 페이지 로그인에서 WebView2 런타임이 없다고 표시되면 Evergreen WebView2 런타임을 설치하거나 쿠키 로그인을 사용하십시오.

## 빌드

직접 빌드하려는 개발자를 위한 간단한 안내입니다.

### 사전 요구 사항

- "유니버설 Windows 플랫폼 개발" 워크로드가 설치된 Visual Studio 2022
- Windows SDK 10.0.22621.0 (지원되는 최소 OS: Windows 10 빌드 17763)

### 절차

- 저장소를 로컬에 복제
- `ExViewer.sln` 열기
- NuGet 패키지 복원
- [Opportunity.ResourceGenerator](https://github.com/OpportunityLiu/ResourceGenerator)로 리소스 파일 생성
  (CI에서는 `Opportunity.ResourceGenerator.Generator.exe ./ExViewer.sln` 실행)
- 빌드 및 실행

CI는 `windows-latest`가 현재 Windows SDK 10.0.26100.0만 제공하고 프로젝트는 10.0.22621.0을 대상으로 하므로 `windows-2022`를 사용합니다.

## 번역

이 프로젝트의 번역을 돕고 싶다면 저장소를 fork한 뒤 `**/Strings/` 폴더의 리소스를 번역하여 pull request를 보내주십시오.

[简体中文](/README.zh-hans.md) 버전처럼 README 번역을 제공할 수도 있습니다.
