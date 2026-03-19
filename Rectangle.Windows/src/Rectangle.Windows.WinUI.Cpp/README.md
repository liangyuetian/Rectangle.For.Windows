# Rectangle Windows C++ WinUI 3 版本

## 项目概述

这是一个使用 C++/WinUI 3 Direct 重写的 Rectangle 窗口管理工具。相比 C# 版本，C++ 版本可以显著减小最终打包的 EXE 文件体积。

## 技术栈

- **C++20** - 主要编程语言
- **WinUI 3 Direct** - Windows UI 框架
- **Windows SDK** - Windows API 调用
- **MSIX** - 打包格式（可选）
- **单文件 EXE** - 自包含部署

## 项目结构

```
Rectangle.Windows.WinUI.Cpp/
├── Core/                           # 核心功能
│   ├── Calculators/               # 窗口位置计算器
│   │   ├── HalfCalculators.h/cpp  # 半屏计算器
│   │   ├── CornerCalculators.h/cpp # 四角计算器
│   │   ├── ThirdCalculators.h/cpp  # 三等分计算器
│   │   ├── FourthCalculators.h/cpp # 四等分计算器
│   │   ├── SixthCalculators.h/cpp  # 六等分计算器
│   │   ├── EighthCalculators.h/cpp # 八等分计算器
│   │   ├── MoveCalculators.h/cpp   # 移动计算器
│   │   └── MiscCalculators.h/cpp   # 其他计算器
│   ├── Enums.h/cpp                # 枚举定义
│   ├── WindowRect.h/cpp           # 窗口矩形
│   ├── WindowAction.h/cpp         # 窗口动作
│   ├── IRectCalculator.h          # 计算器接口
│   ├── CalculatorFactory.h/cpp     # 计算器工厂
│   ├── WindowHistory.h/cpp        # 窗口历史
│   └── RepeatedExecutionsCalculator.h/cpp # 重复执行计算
├── Services/                       # 服务层
│   ├── ConfigService.h/cpp        # 配置服务
│   ├── Logger.h/cpp               # 日志服务
│   ├── Win32WindowService.h/cpp   # Win32 窗口服务
│   ├── HotkeyManager.h/cpp        # 热键管理
│   ├── WindowManager.h/cpp        # 窗口管理
│   ├── TrayIconService.h/cpp      # 托盘图标（重点）
│   ├── ScreenDetectionService.h/cpp # 屏幕检测
│   ├── LastActiveWindowService.h/cpp # 最后活动窗口
│   ├── SnapDetectionService.h/cpp # 拖拽吸附检测
│   ├── OperationHistoryManager.h/cpp # 操作历史
│   └── ThemeService.h/cpp        # 主题服务
├── ViewModels/                    # 视图模型
│   └── SettingsViewModel.h/cpp    # 设置视图模型
├── Views/                         # 视图
│   ├── MainWindow.h/cpp           # 主窗口
│   ├── ShortcutEditor.h/cpp       # 快捷键编辑器
│   └── SnapAreasViewModel.h/cpp   # 吸附区域视图模型
├── App.h/cpp                      # 应用入口
├── main.cpp                       # 主函数
├── pch.h/cpp                      # 预编译头
└── Resources/                     # 资源文件
```

## 托盘菜单注意事项

C++ 版本复刻了 C# 版本托盘菜单的实现，避免了以下常见坑点：

1. **图标加载时机** - 菜单图标在应用启动时预加载
2. **布局计算** - 首次打开菜单时强制触发布局测量
3. **MenuFlyoutPresenter** - 设置 MinWidth 防止布局挤压
4. **DispatcherQueue** - 所有 UI 操作在正确的线程上执行
5. **生命周期管理** - 应用退出时正确清理托盘图标

## 构建说明

### 前置要求

- Visual Studio 2022 或更高版本
- Windows 10 SDK (10.0.19041.0) 或更高版本
- C++20 支持

### 构建步骤

1. 打开 `Rectangle.Windows.WinUI.Cpp.vcxproj`
2. 选择目标平台 (x64, x86, ARM64)
3. 选择配置 (Debug 或 Release)
4. 构建项目

### 发布单文件 EXE

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

### 发布 MSIX

```bash
dotnet publish -c Release -r win-x64 -p:WindowsPackageType=MSIX
```

## 快捷键

| 操作 | 默认快捷键 |
|------|-----------|
| 左半屏 | Ctrl+Alt+← |
| 右半屏 | Ctrl+Alt+→ |
| 上半屏 | Ctrl+Alt+↑ |
| 下半屏 | Ctrl+Alt+↓ |
| 左上 | Ctrl+Alt+U |
| 右上 | Ctrl+Alt+I |
| 左下 | Ctrl+Alt+J |
| 右下 | Ctrl+Alt+K |
| 最大化 | Ctrl+Alt+Enter |
| 居中 | Ctrl+Alt+C |
| 撤销 | Ctrl+Alt+Z |
| 重做 | Ctrl+Alt+Shift+Z |

## 配置

配置文件位于: `%APPDATA%\Rectangle\config.json`

## 许可证

MIT License
