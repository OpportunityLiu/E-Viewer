> Other languages:
>
> - [Simplified Chinese (简体中文)](/README.zh-hans.md)
> - [Korean (한국어)](/README.ko.md)

# E-Viewer

[![CI](https://github.com/OpportunityLiu/E-Viewer/actions/workflows/ci.yml/badge.svg)](https://github.com/OpportunityLiu/E-Viewer/actions/workflows/ci.yml)

A Client for e-hentai.org on Windows 10/11.

[![](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Images/Screenshots/1.png)](https://github.com/OpportunityLiu/E-Viewer/wiki)
[More screenshots](https://github.com/OpportunityLiu/E-Viewer/wiki/Home)

## Releases

Download releases at [GitHub Releases](https://github.com/OpportunityLiu/E-Viewer/releases).

[![Github All Releases](https://img.shields.io/github/downloads/OpportunityLiu/E-Viewer/total.svg)](https://github.com/OpportunityLiu/E-Viewer/releases) [![Github Latest Release](https://img.shields.io/github/downloads/OpportunityLiu/E-Viewer/latest/total.svg)](https://github.com/OpportunityLiu/E-Viewer/releases/latest)

Install dependencies
([x86](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/x86.zip) |
[x64](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/x64.zip) |
[ARM](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/arm.zip) |
[ARM64](https://raw.github.com/wiki/OpportunityLiu/E-Viewer/Dependencies/arm64.zip))
and signature (`.cer` file) first.

Please download `.appx` file to install.
`.appxsym` files are symbol files for debugging.

## FAQ

1. [How to install](https://github.com/OpportunityLiu/E-Viewer/wiki/How-to-Install)

2. [How to enable network proxy for UWP applications](https://github.com/OpportunityLiu/E-Viewer/wiki/Resolve-Connection-Issues)

3. [Tips for keyboard or xbox controller users](https://github.com/OpportunityLiu/E-Viewer/wiki/Tips)

## Build

Here is a simple introduction for those who would like to build this project by themselves.

### Prerequisite

- Visual Studio 2022 17.4
- Windows SDK 10.0.22000

### Procedure

- Clone the repository to local
- Open `ExViewer.sln`
- Restore nuget packages
- _Run `Convert-Resource` in **Package Manager** (See [OpportunityLiu/ResourceGenerator](https://github.com/OpportunityLiu/ResourceGenerator))_
- Build and run

## Translate

If you would like to help me translate this project into other languages,
just fork this project, translate resources in `**/Strings/` folders and open pull requests.

You can also provide translation of README, as the [Simplified Chinese](/README.zh-hans.md) version.
