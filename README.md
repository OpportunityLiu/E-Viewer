# ExViewer
A Client for e-hentai.org on Windows 10.    
用于 WIndows 10 的 E-站 UWP 应用。

| Barnch | Build status |
| ------ | ------------ |
| master | [![Build status](https://ci.appveyor.com/api/projects/status/fcfmss6sltiub0sb/branch/master?svg=true)](https://ci.appveyor.com/project/OpportunityLiu/exviewer/branch/master) |
| dev    | [![Build status](https://ci.appveyor.com/api/projects/status/fcfmss6sltiub0sb/branch/dev?svg=true)](https://ci.appveyor.com/project/OpportunityLiu/exviewer/branch/dev) |

![](https://raw.github.com/wiki/OpportunityLiu/ExViewer/Images/Screenshots/0.png)

## Releases / 发布
| [Latest Release / 最新版本](https://github.com/OpportunityLiu/ExViewer/releases/latest) | [![Github Latest Release](https://img.shields.io/github/downloads/OpportunityLiu/ExViewer/latest/total.svg)](https://github.com/OpportunityLiu/ExViewer/releases/latest) |
| :--- | ---: |
| [**All Releases / 所有版本**](https://github.com/OpportunityLiu/ExViewer/releases) | [![Github All Releases](https://img.shields.io/github/downloads/OpportunityLiu/ExViewer/total.svg)](https://github.com/OpportunityLiu/ExViewer/releases) |

Install dependencies ([x86](https://raw.github.com/wiki/OpportunityLiu/ExViewer/Dependencies/x86.zip)|[x64](https://raw.github.com/wiki/OpportunityLiu/ExViewer/Dependencies/x64.zip)|[ARM](https://raw.github.com/wiki/OpportunityLiu/ExViewer/Dependencies/ARM.zip)) and signature (`.cer` file) first.     
首先安装依赖包 ([x86](https://raw.github.com/wiki/OpportunityLiu/ExViewer/Dependencies/x86.zip)|[x64](https://raw.github.com/wiki/OpportunityLiu/ExViewer/Dependencies/x64.zip)|[ARM](https://raw.github.com/wiki/OpportunityLiu/ExViewer/Dependencies/ARM.zip)) 和证书（`.cer` 文件）。

Please download `.appxbundle` or `.appx` file to install.
`.appxsym` files are symbol files for debugging.    
下载 `.appxbundle` 或 `.appx` 安装包来安装。
`.appxsym` 文件是用于调试的符号信息。

## FAQ / 常见问题
1. [How to install](https://github.com/OpportunityLiu/ExViewer/wiki/How-to-Install)  
   [如何安装](https://github.com/OpportunityLiu/ExViewer/wiki/安装说明)

2. [How to enable network proxy for UWP applications](https://github.com/OpportunityLiu/ExViewer/wiki/Resolve-Connection-Issues)  
   [如何解决 UWP 无法使用代理的问题](https://github.com/OpportunityLiu/ExViewer/wiki/解决连接问题)

3. [Tips for keyboard or xbox controller users](https://github.com/OpportunityLiu/ExViewer/wiki/Tips)  
   [如何使用键盘或 Xbox 控制器操作](https://github.com/OpportunityLiu/ExViewer/wiki/提示)

## Build
Here is a simple introduction for those who would like to build this project by themselves.

### Prerequisite
- Visual Studio 2017 15.6
- Windows SDK 10.0.15063
- Windows SDK 10.0.16299

### Procedure
- Clone the repository to local
- Open `ExViewer.sln`
- Restore nuget packages
- Run `Convert-Resource` in **Package Manager** (See [OpportunityLiu/ResourceGenerator](https://github.com/OpportunityLiu/ResourceGenerator))
- Build and run

## Translate
If you would like to help me translate this project into other languages,
just fork this project, translate resources in `*/Strings/` folders and open pull requests.
