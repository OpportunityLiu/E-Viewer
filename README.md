> Other languages:
>
> - [Simplified Chinese (简体中文)](/README.zh-hans.md)
> - [Korean (한국어)](/README.ko.md)

# E-Viewer

[![CI](https://github.com/OpportunityLiu/E-Viewer/actions/workflows/ci.yml/badge.svg)](https://github.com/OpportunityLiu/E-Viewer/actions/workflows/ci.yml)

A client for e-hentai.org and ExHentai on Windows 10 (version 1809 or later) and Windows 11.

[![](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Images/Screenshots/1.png)](https://github.com/OpportunityLiu/E-Viewer/wiki)
[More screenshots](https://github.com/OpportunityLiu/E-Viewer/wiki/Home)

## Releases

Download releases at [GitHub Releases](https://github.com/OpportunityLiu/E-Viewer/releases).

[![Github All Releases](https://img.shields.io/github/downloads/OpportunityLiu/E-Viewer/total.svg)](https://github.com/OpportunityLiu/E-Viewer/releases) [![Github Latest Release](https://img.shields.io/github/downloads/OpportunityLiu/E-Viewer/latest/total.svg)](https://github.com/OpportunityLiu/E-Viewer/releases/latest)

Install dependencies ([x86](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/x86.zip) | [x64](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/x64.zip) | [ARM](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/arm.zip) | [ARM64](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/arm64.zip)) and the signature (`.cer` file) first.

Download the `.msix` file to install. `.appxsym` files are symbol files for debugging.

Webpage log-on uses the Microsoft Edge WebView2 Runtime. Windows 11 includes it; on Windows 10, install the Evergreen WebView2 Runtime if it is missing. Cookie log-on does not require WebView2.

## FAQ

1. [How to install](https://github.com/OpportunityLiu/E-Viewer/wiki/How-to-Install)

2. [How to enable network proxy for UWP applications](https://github.com/OpportunityLiu/E-Viewer/wiki/Resolve-Connection-Issues)

3. [Tips for keyboard or xbox controller users](https://github.com/OpportunityLiu/E-Viewer/wiki/Tips)

4. If webpage log-on reports that the WebView2 Runtime is missing, install the Evergreen WebView2 Runtime, or use cookie log-on instead.

## Build

Here is a simple introduction for those who would like to build this project by themselves.

### Prerequisite

- Visual Studio 2022 with the "Universal Windows Platform development" workload
- Windows SDK 10.0.22621.0 (minimum supported OS: Windows 10 build 17763)

### Procedure

- Clone the repository to local
- Open `ExViewer.sln`
- Restore nuget packages
- Generate resource files with [Opportunity.ResourceGenerator](https://github.com/OpportunityLiu/ResourceGenerator)
  (CI runs `Opportunity.ResourceGenerator.Generator.exe ./ExViewer.sln`)
- Build and run

The CI workflow uses `windows-2022`, because `windows-latest` currently only ships Windows SDK 10.0.26100.0 while the project targets 10.0.22621.0.

## Translate

If you would like to help me translate this project into other languages,
just fork this project, translate resources in `**/Strings/` folders and open pull requests.

You can also provide translation of README, as the [Simplified Chinese](/README.zh-hans.md) version.
