# Rectangle for Windows

Windows 版 Rectangle 窗口管理工具，灵感来自 macOS 的 [Rectangle](https://github.com/rxhanson/Rectangle)。

## 功能特性

- 使用快捷键快速调整窗口位置和大小
- 系统托盘图标，右键菜单快速访问常用功能
- 支持多显示器
- 拖拽窗口到屏幕边缘自动吸附
- 自定义快捷键配置

## 系统要求

- Windows 10/11
- .NET 9.0 Runtime

## 编译

```bash
# 编译项目
dotnet build src/Rectangle.Windows/Rectangle.Windows.csproj -c Release

# 运行测试
dotnet test src/Rectangle.Windows.Tests/Rectangle.Windows.Tests.csproj
```

## 打包发布
cd ./Rectangle.Windows/src/Rectangle.Window
dotnet publish -c Release # 用户需要.net9，~24M
dotnet publish -c Release -r win-x64 --self-contained true # 用户不需要安装.net9 ~54M,启用压缩后

### 方式一：压缩单文件（推荐，约 54MB）

包含 .NET 运行时，启用压缩，用户无需安装任何依赖：

```bash
dotnet publish src/Rectangle.Windows/Rectangle.Windows.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -p:DebugSymbols=false -o ./publish
```

### 方式二：精简单文件（约 25MB）

不包含 .NET 运行时，需要用户安装 [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)：

```bash
dotnet publish src/Rectangle.Windows/Rectangle.Windows.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false -o ./publish-lite
```

### 方式三：未压缩单文件（约 138MB）

包含运行时但不压缩，启动速度稍快：

```bash
dotnet publish src/Rectangle.Windows/Rectangle.Windows.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish-full
```

### 打包大小对比

| 方式 | 文件大小 | 启动速度 | 用户要求 |
|------|----------|----------|----------|
| 压缩单文件 | ~54 MB | 稍慢（解压） | 无需安装 |
| 精简单文件 | ~25 MB | 快 | 需安装 .NET 9 |
| 未压缩单文件 | ~138 MB | 快 | 无需安装 |

### 打包参数说明

| 参数 | 说明 |
|------|------|
| `-c Release` | 使用 Release 配置 |
| `-r win-x64` | 目标平台为 Windows 64 位 |
| `--self-contained true` | 包含 .NET 运行时，无需用户安装 |
| `-p:PublishSingleFile=true` | 打包成单个 EXE 文件 |
| `-p:IncludeNativeLibrariesForSelfExtract=true` | 将原生库打包进单文件 |
| `-p:EnableCompressionInSingleFile=true` | 启用压缩，显著减小体积 |
| `-p:DebugType=none` | 不生成调试符号 |
| `-p:DebugSymbols=false` | 不生成 PDB 文件 |

> ⚠️ **注意**：由于项目使用 Windows Forms，不支持裁剪功能 (`PublishTrimmed`)。

## 运行

```bash
dotnet run --project src/Rectangle.Windows/Rectangle.Windows.csproj -c Release
```

或直接运行编译后的可执行文件：
```bash
src\Rectangle.Windows\bin\Release\net9.0-windows10.0.19041.0\Rectangle.Windows.exe
```

## 支持的快捷键

### 半屏操作
| 功能 | 快捷键 |
|------|--------|
| 左半屏 | `Ctrl + Alt + ←` |
| 右半屏 | `Ctrl + Alt + →` |
| 上半屏 | `Ctrl + Alt + ↑` |
| 下半屏 | `Ctrl + Alt + ↓` |

### 四角操作
| 功能 | 快捷键 |
|------|--------|
| 左上角 | `Ctrl + Alt + U` |
| 右上角 | `Ctrl + Alt + I` |
| 左下角 | `Ctrl + Alt + J` |
| 右下角 | `Ctrl + Alt + K` |

### 三分之一屏
| 功能 | 快捷键 |
|------|--------|
| 左首 1/3 | `Ctrl + Alt + D` |
| 中间 1/3 | `Ctrl + Alt + F` |
| 右首 1/3 | `Ctrl + Alt + G` |
| 左侧 2/3 | `Ctrl + Alt + E` |
| 中间 2/3 | `Ctrl + Alt + R` |
| 右侧 2/3 | `Ctrl + Alt + T` |

### 最大化与缩放
| 功能 | 快捷键 |
|------|--------|
| 最大化 | `Ctrl + Alt + Enter` |
| 居中 | `Ctrl + Alt + C` |
| 恢复 | `Ctrl + Alt + Backspace` |
| 最大化高度 | `Ctrl + Alt + Shift + ↑` |
| 放大 | `Ctrl + Alt + =` |
| 缩小 | `Ctrl + Alt + -` |

### 显示器切换
| 功能 | 快捷键 |
|------|--------|
| 下一个显示器 | `Ctrl + Alt + Win + →` |
| 上一个显示器 | `Ctrl + Alt + Win + ←` |

## 托盘菜单

右键点击系统托盘图标可访问：
- 半屏操作（左/右/上/下）
- 四角操作
- 三分之一屏操作
- 最大化/恢复/居中
- 偏好设置
- 退出

## 配置

配置文件位置：`%APPDATA%\Rectangle\config.json`

可配置项：
- `GapSize`: 窗口间隙大小 (0-20)
- `LaunchOnLogin`: 开机启动
- `IgnoredApps`: 忽略的应用程序列表
- `Shortcuts`: 自定义快捷键

## 技术栈

- .NET 9
- WinForms (系统托盘)
- CsWin32 (Windows API 调用)
- H.NotifyIcon (托盘图标)

## 许可证

MIT License
