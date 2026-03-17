# Rectangle.Windows.WinUI

基于 WinUI 3 的 Rectangle 窗口管理工具。

## 系统要求

- Windows 10 1809 (17763) 或更高
- 打包需 [.NET SDK](https://dotnet.microsoft.com/download)，在 **Windows** 上执行

## 打包发布

```powershell
cd Rectangle.Windows/src/Rectangle.Windows.WinUI
```

| 方式 | 命令 | 输出 |
|------|------|------|
| **多文件** | `dotnet publish -c Release -r win-x64 --self-contained true -o publish` | `publish/` |
| **单文件 Lite** | `dotnet publish -p:PublishProfile=win-x64-lite -c Release` | `publish-lite/`，体积最小，需用户安装 .NET |
| **单文件 Full** | `dotnet publish -p:PublishProfile=win-x64-full -c Release` | `publish-full/`，可独立运行 |

### Inno Setup 安装包

1. 先执行上述任一发布
2. 用 [Inno Setup](https://jrsoftware.org/isinfo.php) 打开 `installer.iss` 编译
3. 输出：`installer_output/Rectangle-Setup-1.0.0.exe`

### MSIX（应用商店）

Visual Studio → 右键项目 → **打包和发布** → **创建应用包**

## 运行

```powershell
dotnet run --project Rectangle.Windows.WinUI.csproj -c Release
# 或直接运行
.\publish\Rectangle.Windows.WinUI.exe
```

## 配置

`%APPDATA%\Rectangle\config.json`
