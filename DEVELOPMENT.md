# Rectangle for Windows - 开发文档

## 项目概述

Rectangle for Windows 是一个 Windows 窗口管理工具，灵感来源于 macOS 的 Rectangle 应用。它允许用户使用键盘快捷键快速调整窗口位置和大小。

## 技术栈

- **框架**: .NET 9 / Windows Forms
- **语言**: C# 12
- **Windows API**: CsWin32 (P/Invoke 代码生成)
- **UI**: 自定义 WinForms 控件 + Acrylic 效果

## 项目结构

```
Rectangle.Windows/
├── src/Rectangle.Windows/
│   ├── Core/                    # 核心功能
│   │   ├── Calculators/         # 窗口位置计算器
│   │   ├── WindowAction.cs      # 窗口操作枚举
│   │   ├── WindowRect.cs        # 窗口矩形结构
│   │   ├── WindowHistory.cs     # 窗口历史记录
│   │   └── ...
│   ├── Services/                # 服务层
│   │   ├── WindowManager.cs     # 窗口管理器
│   │   ├── ConfigService.cs     # 配置服务
│   │   ├── HotkeyManager.cs     # 热键管理器
│   │   ├── SnappingManager.cs   # 拖拽吸附管理器
│   │   ├── Logger.cs            # 日志服务
│   │   └── ...
│   ├── Views/                   # UI 层
│   │   ├── SettingsForm.cs      # 设置窗口
│   │   ├── LogViewerForm.cs     # 日志查看器
│   │   └── ...
│   └── Program.cs               # 程序入口
└── tests/Rectangle.Windows.Tests/  # 单元测试
```

## 核心概念

### 1. 窗口操作 (WindowAction)

所有支持的窗口操作都定义在 `WindowAction` 枚举中：

- **半屏**: LeftHalf, RightHalf, TopHalf, BottomHalf, CenterHalf
- **四角**: TopLeft, TopRight, BottomLeft, BottomRight
- **三分之一**: FirstThird, CenterThird, LastThird, FirstTwoThirds, LastTwoThirds
- **四等分**: FirstFourth, SecondFourth, ThirdFourth, LastFourth
- **六等分**: TopLeftSixth, TopCenterSixth, ...
- **九等分**: TopLeftNinth, TopCenterNinth, ...
- **八等分**: TopLeftEighth, TopCenterLeftEighth, ...
- **最大化**: Maximize, AlmostMaximize, MaximizeHeight
- **移动**: MoveLeft, MoveRight, MoveUp, MoveDown
- **显示器**: NextDisplay, PreviousDisplay
- **多窗口**: TileAll, CascadeAll, ReverseAll
- **Todo 模式**: LeftTodo, RightTodo

### 2. 计算器模式 (IRectCalculator)

每个窗口操作都有一个对应的计算器来实现具体的矩形计算：

```csharp
public interface IRectCalculator
{
    WindowRect Calculate(WorkArea workArea, WindowRect currentRect, WindowAction action);
}
```

计算器负责根据工作区大小和当前操作，计算出目标窗口位置。

### 3. 窗口历史 (WindowHistory)

`WindowHistory` 记录程序对窗口的操作，用于：
- **恢复功能**: 保存原始窗口位置，支持 Restore 操作
- **重复执行检测**: 检测连续执行同一操作，实现循环尺寸功能
- **Unsnap 恢复**: 拖拽已吸附窗口时恢复原始尺寸

### 4. 配置系统 (AppConfig)

配置存储在 `%APPDATA%/Rectangle/config.json`，包括：

- **基础配置**: 间隙大小、启动时运行、忽略应用列表
- **快捷键**: 每个操作的键盘快捷键
- **高级配置**: 接近最大化比例、最小窗口尺寸、光标移动等
- **拖拽吸附**: 吸附区域大小、预览窗口样式
- **Todo 模式**: Todo 应用、侧边栏宽度
- **日志**: 日志级别、日志文件路径

## 开发指南

### 添加新的窗口操作

1. **在 `WindowAction.cs` 中添加枚举值**

```csharp
public enum WindowAction
{
    // ... 现有操作
    MyNewAction
}
```

2. **创建计算器**

```csharp
public class MyNewActionCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentRect, WindowAction action)
    {
        // 计算目标矩形
        return new WindowRect(x, y, width, height);
    }
}
```

3. **在 `CalculatorFactory` 中注册**

```csharp
public static IRectCalculator? GetCalculator(WindowAction action)
{
    return action switch
    {
        // ... 现有映射
        WindowAction.MyNewAction => new MyNewActionCalculator(),
        _ => null
    };
}
```

4. **在 `WindowManager.Execute()` 中处理（如果需要特殊逻辑）**

5. **添加默认快捷键（可选）**

在 `ConfigService.GetDefaultShortcuts()` 中添加：

```csharp
["MyNewAction"] = new() { KeyCode = 0xXX, ModifierFlags = ctrlAlt }
```

### 添加新的配置项

1. **在 `AppConfig` 中添加属性**

```csharp
public class AppConfig
{
    public bool MyNewSetting { get; set; } = false;
    public int MyNewNumber { get; set; } = 100;
}
```

2. **在需要的地方使用**

```csharp
var config = _configService.Load();
if (config.MyNewSetting)
{
    // 执行逻辑
}
```

### 使用日志系统

```csharp
// 初始化（通常在程序启动时）
Logger.Initialize(
    minLevel: LogLevel.Info,
    logToFile: true,
    logFilePath: "",
    maxFileSizeMB: 10
);

// 记录日志
Logger.Debug("Category", "Debug message");
Logger.Info("Category", "Info message");
Logger.Warning("Category", "Warning message");
Logger.Error("Category", "Error message", exception);
```

## 测试

### 运行单元测试

```bash
cd Rectangle.Windows/tests/Rectangle.Windows.Tests
dotnet test
```

### 测试覆盖范围

- **CalculatorTests**: 所有窗口位置计算器的测试
- **WindowHistoryTests**: 窗口历史记录功能测试
- **WindowRectTests**: 窗口矩形结构测试
- **LoggerTests**: 日志系统测试

## 调试技巧

### 1. 启用详细日志

在 `Program.cs` 中初始化日志时设置级别为 `LogLevel.Debug`：

```csharp
Logger.Initialize(LogLevel.Debug, true, "", 10);
```

### 2. 使用日志查看器

在托盘菜单中添加打开日志查看器的选项：

```csharp
var logViewer = new LogViewerForm();
logViewer.Show();
```

### 3. 调试窗口操作

在 `WindowManager.Execute()` 中添加断点，查看：
- 目标窗口句柄
- 执行的操作类型
- 计算出的目标矩形

## 性能优化

### 1. 窗口信息缓存

使用 `WindowInfoCache` 减少频繁的 Win32 API 调用：

```csharp
var cache = new WindowInfoCache();
var rect = cache.GetWindowRect(hwnd, _win32);
```

### 2. 历史记录清理

`WindowHistory` 会自动清理过期记录，可通过配置调整：

```json
{
  "MaxWindowHistoryCount": 100,
  "WindowHistoryExpirationMinutes": 60
}
```

### 3. 批量操作

多窗口操作（如 TileAll）使用批量处理，减少重复计算。

## 常见问题

### Q: 为什么某些窗口无法调整？

A: 检查 `WindowTypeService.IsResizable()`，某些窗口（如模态对话框、工具窗口）被排除在外。

### Q: 多显示器支持如何实现？

A: 使用 `Win32WindowService.GetWorkAreaFromWindow()` 或 `ScreenDetectionService.GetWorkAreaFromCursor()` 获取正确的显示器工作区。

### Q: 如何调试拖拽吸附功能？

A: 启用 Debug 级别日志，查看 `[SnappingManager]` 相关的日志输出。

## 贡献指南

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 创建 Pull Request

## 许可证

本项目采用 MIT 许可证 - 详见 LICENSE 文件

## 相关资源

- [Rectangle macOS](https://github.com/rxhanson/Rectangle) - 原版 macOS 应用
- [Windows API 文档](https://docs.microsoft.com/en-us/windows/win32/api/)
- [CsWin32](https://github.com/microsoft/CsWin32) - P/Invoke 代码生成器
