# Rectangle.Windows.WinUI

基于 WinUI 3 的 Rectangle 窗口管理工具。

## 系统要求

- Windows 10 1809 (17763) 或更高版本
- 无需单独安装 .NET 运行时（自包含发布时）

## 打包发布

以下命令需在 **Windows** 上执行，且需安装 [.NET SDK](https://dotnet.microsoft.com/download)。

### 方式一：多文件发布（推荐，最稳定）

输出到 `publish` 目录，包含 exe 及依赖文件。用户无需安装 .NET。

```powershell
cd Rectangle.Windows/src/Rectangle.Windows.WinUI

# x64（推荐）
dotnet publish -c Release -r win-x64 --self-contained true -o publish

# x86
dotnet publish -c Release -r win-x86 --self-contained true -o publish

# ARM64
dotnet publish -c Release -r win-arm64 --self-contained true -o publish
```

### 方式二：单文件 exe（自包含）

打包成单个 exe，包含 .NET 运行时与 Windows App SDK，用户无需安装依赖。

```powershell
cd Rectangle.Windows/src/Rectangle.Windows.WinUI

dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -p:WindowsAppSDKSelfContained=true `
  -p:PublishTrimmed=true `
  -p:TrimMode=partial `
  -p:DebugType=none `
  -p:DebugSymbols=false `
  -o publish
```

> 说明：WinUI 3 单文件发布可能伴随 Windows App SDK 原生库解压，首次启动略慢。

#### 单文件体积优化

若单文件体积过大，可尝试以下方法：

| 方法 | 预计效果 | 说明 |
|------|----------|------|
| **精简单文件**（方式三） | 减少约 50% | 不包含 .NET 运行时，需用户安装 .NET |
| **InvariantGlobalization** | 减少约 1–2 MB | 应用不需要多语言时启用 |
| **移除未使用依赖** | 视依赖而定 | 如确认未使用 WebView2，可从 csproj 移除 |
| **TrimmerRemoveSymbols** | 减少约 0.5–1 MB | 移除 PDB 等调试符号 |
| **TrimMode=full** | 减少约 5–15 MB | 更激进裁剪，需充分测试 |

**体积优化版单文件命令示例：**

```powershell
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -p:WindowsAppSDKSelfContained=true `
  -p:PublishTrimmed=true `
  -p:TrimMode=partial `
  -p:TrimmerRemoveSymbols=true `
  -p:DebugType=none `
  -p:DebugSymbols=false `
  -o publish
```

> 若应用为纯英文且不需要多语言，可额外加 `-p:InvariantGlobalization=true` 再减约 1–2 MB。

> ⚠️ `TrimMode=full` 可能引发 WinUI 反射相关运行时错误，建议先保持 `partial`，仅在确认无问题后再尝试 `full`。

### 方式三：精简单文件（需用户安装 .NET）

不包含 .NET 运行时，体积更小，但需用户安装 [.NET 10 Runtime](https://dotnet.microsoft.com/download)。

```powershell
dotnet publish -c Release -r win-x64 --self-contained false `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:DebugType=none `
  -p:DebugSymbols=false `
  -o publish-lite
```

### 方式四：Inno Setup 安装包

1. 先执行发布（任选方式一或方式二）：

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -o publish
```

2. 安装 [Inno Setup](https://jrsoftware.org/isinfo.php)，用其打开并编译 `installer.iss`：

```powershell
# 或用命令行编译（需将 iscc 加入 PATH）
iscc installer.iss
```

3. 安装包输出在 `installer_output/Rectangle-Setup-1.0.0.exe`。

### 方式五：MSIX 安装包（应用商店格式）

在 Visual Studio 中：

1. 右键项目 → **打包和发布** → **创建应用包**
2. 选择 **旁加载** 或 **Microsoft Store**
3. 按向导完成签名与打包

或使用命令行（需配置签名证书）：

```powershell
msbuild /t:Pack /p:Configuration=Release /p:Platform=x64
```

## 打包参数说明

| 参数 | 说明 |
|------|------|
| `-c Release` | Release 配置 |
| `-r win-x64` | 目标平台（win-x86 / win-x64 / win-arm64） |
| `--self-contained true` | 包含 .NET 运行时 |
| `-p:PublishSingleFile=true` | 单文件 exe |
| `-p:IncludeNativeLibrariesForSelfExtract=true` | 原生库打包进 exe |
| `-p:EnableCompressionInSingleFile=true` | 单文件内压缩 |
| `-p:WindowsAppSDKSelfContained=true` | 包含 Windows App SDK 运行时 |
| `-p:PublishTrimmed=true` | 裁剪未使用代码 |
| `-p:TrimMode=partial` | 部分裁剪（WinUI 推荐） |
| `-o publish` | 输出目录 |

## 运行

```powershell
dotnet run --project Rectangle.Windows.WinUI.csproj -c Release
```

或直接运行发布后的 exe：

```powershell
.\publish\Rectangle.Windows.WinUI.exe
```

## 配置文件

`%APPDATA%\Rectangle\config.json`
