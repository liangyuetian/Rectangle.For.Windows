# Rectangle Windows 版本实现计划

## 一、项目概述

### 1.1 目标

将 Rectangle（macOS 窗口管理应用）的核心功能移植到 Windows 平台，提供一致的键盘快捷键和拖拽吸附体验。

### 1.2 功能范围

| 优先级 | 功能模块 | 说明 |
|--------|----------|------|
| P0 | 核心窗口操作 | 半屏、四分之一、最大化、居中、恢复 |
| P0 | 全局快捷键 | 注册、冲突检测、忽略应用 |
| P0 | 窗口移动 API | 获取前台窗口、设置位置和大小 |
| P1 | 拖拽吸附 | 边缘/角落吸附、足迹预览 |
| P1 | 三分之一/四分之一布局 | 完整布局支持 |
| P1 | 多显示器 | 跨显示器移动、屏幕检测 |
| P2 | 六分之一/九分之一/八分之一 | 高级布局 |
| P2 | 放大/缩小 | Larger/Smaller |
| P2 | 配置导入导出 | JSON 配置 |
| P3 | URL 方案 | 自动化支持 |
| P3 | Todo 模式 | 可选 |

### 1.3 参考项目

- **PowerToys FancyZones**：微软开源，C++，可参考架构与 Win32 调用
- **Rectangle (macOS)**：功能与交互设计来源

---

## 二、技术选型

### 2.1 选定方案：C# + WinUI 3

| 维度 | 选择 | 理由 |
|------|------|------|
| 语言 | C# | 开发效率高、生态成熟、易于维护 |
| UI 框架 | WinUI 3 | Fluent Design、现代化控件、Windows 11 原生风格 |
| 运行时 | .NET 8 | LTS 版本 |
| 系统要求 | Windows 10 1809+ | WinUI 3 最低要求 |

### 2.2 技术栈清单

| 组件 | 技术 | 说明 |
|------|------|------|
| 应用框架 | .NET 8 | LTS |
| UI | WinUI 3 | 主窗口、设置界面 |
| 系统托盘 | H.NotifyIcon 或 Shell_NotifyIcon | WinUI 3 无内置托盘，需第三方或 Win32 |
| 窗口操作 | CsWin32 / P/Invoke | user32.dll, shell32.dll |
| 全局快捷键 | RegisterHotKey | 需通过 WindowNative.GetWindowHandle 获取句柄 |
| 配置存储 | System.Text.Json + 文件 | %APPDATA%\Rectangle\config.json |
| 安装包 | MSIX | WinUI 3 推荐打包方式 |

### 2.3 WinUI 3 专项说明

| 模块 | 注意事项 | 推荐方案 |
|------|----------|----------|
| **系统托盘** | WinUI 3 无内置 NotifyIcon | 使用 [H.NotifyIcon](https://github.com/HavenDV/H.NotifyIcon)（MIT）或 Shell_NotifyIcon + 消息窗口 |
| **全局快捷键** | 需窗口句柄接收 WM_HOTKEY | `WinRT.Interop.WindowNative.GetWindowHandle(this)` + 消息钩子 |
| **P/Invoke** | 推荐使用 CsWin32 源生成器 | 减少手写声明错误 |
| **打包** | 必须 MSIX | 不支持传统 exe 安装包（可用 unpackaged 模式开发） |
| **DPI** | 默认 PerMonitorV2 | 在 app.manifest 中配置 |

### 2.4 推荐 NuGet 包

| 包名 | 用途 |
|------|------|
| Microsoft.WindowsAppSDK | WinUI 3 运行时 |
| Microsoft.Windows.SDK.BuildTools | WinUI 3 构建 |
| Microsoft.Windows.CsWin32 | P/Invoke 源生成（Win32 API） |
| H.NotifyIcon.WinUI | 系统托盘（或 H.NotifyIcon 通用版） |

---

## 三、macOS 与 Windows API 映射

### 3.1 窗口操作

| macOS (Rectangle) | Windows 等效 API | 说明 |
|-------------------|------------------|------|
| AccessibilityElement (AXUIElement) | GetForegroundWindow / FindWindow | 获取窗口句柄 |
| AXFrame / setFrame | GetWindowRect / SetWindowPos | 获取/设置窗口位置和大小 |
| NSScreen | MonitorFromWindow / EnumDisplayMonitors | 多显示器 |
| CGWindowListCopyWindowInfo | EnumWindows + GetWindowText | 枚举窗口（可选） |

### 3.2 关键 Win32 API

```csharp
// 获取前台窗口
[DllImport("user32.dll")]
static extern IntPtr GetForegroundWindow();

// 获取窗口矩形（屏幕坐标）
[DllImport("user32.dll")]
static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

// 设置窗口位置和大小
[DllImport("user32.dll")]
static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, 
    int X, int Y, int cx, int cy, uint uFlags);

// 获取工作区（排除任务栏）
[DllImport("user32.dll")]
static extern bool SystemParametersInfo(uint uiAction, uint uiParam, 
    ref RECT pvParam, uint fWinIni);

// 获取窗口所在显示器
[DllImport("user32.dll")]
static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

// 获取显示器信息
[DllImport("user32.dll")]
static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
```

### 3.3 全局快捷键（WinUI 3）

| macOS | Windows (WinUI 3) |
|-------|-------------------|
| MASShortcut | RegisterHotKey / UnregisterHotKey |
| NSEvent.addGlobalMonitorForEvents | WM_HOTKEY 消息处理 |

**WinUI 3 实现要点：**

- 获取窗口句柄：`WinRT.Interop.WindowNative.GetWindowHandle(this)`（`this` 为 `Window`）
- WinUI 3 不直接暴露消息循环，需通过**窗口子类化**或 **HwndMessageHook** 拦截 `WM_HOTKEY` (0x312)
- 参考：[Chapter 5: Add global hot key in WinUI 3](https://whid.eu/2022/05/13/chapter-5-add-global-hot-key-in-winui-3/)

### 3.4 拖拽检测

| macOS | Windows |
|-------|---------|
| NSEvent.addGlobalMonitorForEvents (mouse) | SetWindowsHookEx (WH_MOUSE_LL) 或 GetAsyncKeyState 轮询 |
| 窗口标题栏拖拽 | 需监听鼠标按下 + 移动，判断是否在标题栏 |

---

## 四、Windows 平台特殊考虑

### 4.1 Windows 11 Snap Layouts

- Windows 11 自带 Snap Layouts（Win + Z），可能与快捷键冲突
- **建议**：文档中说明用户可关闭系统 Snap，或使用不同快捷键（如 Ctrl+Alt）
- 注册表关闭 Snap：`HKEY_CURRENT_USER\...\Explorer\Advanced` → `EnableSnapBar` = 0

### 4.2 权限与提升

- 普通应用无法移动**以管理员身份运行**的窗口
- **方案**：应用也以管理员运行，或在文档中说明限制
- PowerToys FancyZones 采用「以管理员运行」解决此问题

### 4.3 DPI 感知

- 必须声明 DPI 感知，否则坐标在高 DPI 下会错位
- 在 app.manifest 中设置：
  ```xml
  <dpiAware>true/pm</dpiAware>
  <dpiAwareness>PerMonitorV2</dpiAwareness>
  ```

### 4.4 多显示器与虚拟桌面

- `MonitorFromWindow` 可获取窗口所在显示器
- 虚拟桌面（Virtual Desks）API 较新，可选支持

### 4.5 配置存储位置

| 方案 | 路径 | 说明 |
|------|------|------|
| 文件 | %APPDATA%\Rectangle\config.json | 与 macOS 风格一致 |
| 注册表 | HKCU\Software\Rectangle | 传统 Windows 方式 |
| 推荐 | 文件 + 可选注册表 | 便于导入导出 |

---

## 五、分阶段实现计划

### 阶段 0：项目初始化（1–2 天）

| 任务 | 产出 | 验收标准 |
|------|------|----------|
| 创建解决方案结构 | .sln, 项目文件 | 可编译运行 |
| 配置 WPF 应用 | MainWindow, App.xaml | 启动显示托盘图标 |
| 配置 DPI 感知 | app.manifest | 高 DPI 下显示正常 |
| 搭建 CI（可选） | GitHub Actions | 自动构建 |

**目录结构建议（WinUI 3）：**

```
Rectangle.Windows/
├── Rectangle.Windows.sln
├── src/
│   ├── Rectangle.Windows/           # 主应用 (WinUI 3)
│   │   ├── App.xaml
│   │   ├── MainWindow.xaml
│   │   ├── TrayIcon/                 # H.NotifyIcon 或自定义
│   │   ├── Services/
│   │   │   ├── WindowManager.cs
│   │   │   ├── HotkeyManager.cs      # RegisterHotKey + WM_HOTKEY
│   │   │   └── ConfigService.cs
│   │   ├── Core/
│   │   │   ├── WindowAction.cs
│   │   │   ├── RectCalculator/
│   │   │   └── Win32/                # CsWin32 或 P/Invoke
│   │   └── Views/
│   │       └── SettingsPage.xaml     # 或 SettingsWindow
│   └── Rectangle.Windows.Tests/
├── NativeMethods.txt                 # CsWin32 源生成用
└── docs/
```

---

### 阶段 1：核心窗口管理（3–5 天）

#### 1.1 Win32 封装

| 任务 | 说明 |
|------|------|
| 创建 Win32Interop.cs | P/Invoke 声明：GetForegroundWindow, GetWindowRect, SetWindowPos, GetMonitorInfo 等 |
| 创建 WindowInfo 模型 | 句柄、位置、大小、所在显示器 |
| 实现窗口过滤 | 排除无边框、工具窗口、不可见窗口 |
| 实现 MoveWindow / SetWindowPos 封装 | 支持 SWP_NOACTIVATE 等标志 |

#### 1.2 布局计算

| 任务 | 对应 macOS 类 | 说明 |
|------|---------------|------|
| ScreenHelper | UsableScreens | 获取工作区（排除任务栏） |
| RectCalculator 基类 | WindowCalculation | 输入：窗口、屏幕、动作 → 输出：目标 RECT |
| LeftHalfCalculator | LeftRightHalfCalculation | 左半屏 |
| RightHalfCalculator | LeftRightHalfCalculation | 右半屏 |
| TopHalfCalculator | TopHalfCalculation | 上半屏 |
| BottomHalfCalculator | BottomHalfCalculation | 下半屏 |
| MaximizeCalculator | MaximizeCalculation | 最大化 |
| CenterCalculator | CenterCalculation | 居中 |
| TopLeftCalculator | UpperLeftCalculation | 左上四分之一 |
| TopRightCalculator | UpperRightCalculation | 右上四分之一 |
| BottomLeftCalculator | LowerLeftCalculation | 左下四分之一 |
| BottomRightCalculator | LowerRightCalculation | 右下四分之一 |
| RestoreLogic | WindowHistory | 恢复上次位置 |

#### 1.3 WindowManager 核心

| 任务 | 说明 |
|------|------|
| Execute(WindowAction) | 获取前台窗口 → 选择计算器 → 计算目标 RECT → 调用 SetWindowPos |
| WindowHistory | 字典存储 windowId → 上次 RECT |
| 错误处理 | 无前台窗口时提示音或 Toast |

**验收标准**：通过代码或临时快捷键能正确执行左半屏、右半屏、最大化、恢复。

---

### 阶段 2：全局快捷键（2–3 天）

#### 2.1 HotkeyManager（WinUI 3）

| 任务 | 说明 |
|------|------|
| 获取 MainWindow 句柄 | `WindowNative.GetWindowHandle(App.MainWindow)` |
| 注册消息钩子 | 子类化窗口以接收 WM_HOTKEY (0x312) |
| RegisterHotKey 封装 | 支持 Ctrl/Alt/Shift/Win 组合 |
| 快捷键 → WindowAction 映射 | 配置驱动 |
| 冲突检测 | 检测是否已被占用，提示用户 |
| 应用最小化时 | 需保持窗口存在以接收消息，可隐藏主窗口 |

#### 2.2 默认快捷键

与 macOS 推荐方案对齐（可用 Ctrl+Alt 替代 Cmd+Option）：

| 操作 | 默认快捷键 |
|------|------------|
| 左半屏 | Ctrl + Alt + ← |
| 右半屏 | Ctrl + Alt + → |
| 上半屏 | Ctrl + Alt + ↑ |
| 下半屏 | Ctrl + Alt + ↓ |
| 最大化 | Ctrl + Alt + Enter |
| 恢复 | Ctrl + Alt + Delete |
| ... | 见 docs/03-keyboard-shortcuts.md |

#### 2.3 忽略应用

| 任务 | 说明 |
|------|------|
| 获取前台进程 | GetWindowThreadProcessId + GetForegroundWindow |
| 忽略列表 | 进程名或路径，可配置 |
| 动态注册 | 当前应用在忽略列表时，不响应快捷键 |

**验收标准**：注册快捷键后，任意应用在前台时能触发对应窗口操作。

---

### 阶段 3：设置界面与配置（2–3 天）

#### 3.1 设置窗口

| 任务 | 说明 |
|------|------|
| 快捷键标签页 | 列表展示所有操作，支持点击录制 |
| 通用标签页 | 开机启动、托盘图标、间隙大小、重复执行模式 |
| 忽略应用标签页 | 添加/移除忽略的应用 |
| 保存到配置文件 | JSON 格式，便于与 macOS 配置互导 |

#### 3.2 配置持久化

| 任务 | 说明 |
|------|------|
| ConfigService | 读写 %APPDATA%\Rectangle\config.json |
| 配置结构 | 与 macOS Config 兼容（shortcuts + defaults） |
| 导入/导出 | 支持从 macOS 导出的 JSON 导入 |

**验收标准**：修改快捷键和设置后重启应用，配置正确保留。

---

### 阶段 4：拖拽吸附（4–6 天）

#### 4.1 拖拽检测

| 任务 | 说明 |
|------|------|
| 低级别鼠标钩子 | SetWindowsHookEx(WH_MOUSE_LL) |
| 判断拖拽开始 | 鼠标按下 + 在窗口标题栏区域 |
| 判断拖拽结束 | 鼠标释放 |
| 光标位置 → 吸附区域 | 根据屏幕边缘距离判断 |

#### 4.2 吸附区域模型

| 任务 | 说明 |
|------|------|
| SnapAreaModel | 与 macOS SnapAreaModel 对应 |
| 横屏/竖屏 | 根据显示器方向选择不同映射 |
| 边缘边距 | snapEdgeMarginTop 等可配置 |

#### 4.3 足迹预览

| 任务 | 说明 |
|------|------|
| 透明覆盖窗口 | 半透明矩形显示目标区域 |
| 位置与大小 | 与计算出的目标 RECT 一致 |
| 动画（可选） | 淡入淡出 |

#### 4.4 修饰键限制

| 任务 | 说明 |
|------|------|
| snapModifiers | 仅当按下指定修饰键时启用吸附 |
| 与 PowerToys 区分 | 默认 Shift+Drag 或可配置 |

**验收标准**：拖拽窗口到左边缘松手，窗口吸附到左半屏，并显示预览。

---

### 阶段 5：三分之一与高级布局（2–3 天）

| 任务 | 对应 macOS | 说明 |
|------|------------|------|
| FirstThirdCalculator | FirstThirdCalculation | 第一三分之一 |
| CenterThirdCalculator | CenterThirdCalculation | 中间三分之一 |
| LastThirdCalculator | LastThirdCalculation | 最后三分之一 |
| FirstTwoThirdsCalculator | FirstTwoThirdsCalculation | 前三分之二 |
| LastTwoThirdsCalculator | LastTwoThirdsCalculation | 后三分之二 |
| CenterHalfCalculator | CenterHalfCalculation | 中间半屏 |
| 四分之一细分 | FirstFourthCalculation 等 | first-fourth, second-fourth 等 |
| 重复执行模式 | subsequentExecutionMode | 1/2 → 2/3 → 1/3 循环 |

**验收标准**：三分之一、四分之一等布局通过快捷键正确执行。

---

### 阶段 6：多显示器（1–2 天）

| 任务 | 说明 |
|------|------|
| 屏幕枚举 | EnumDisplayMonitors |
| 工作区计算 | 每个显示器的工作区（排除任务栏） |
| NextDisplay / PreviousDisplay | 按物理顺序或 X 坐标排序 |
| 光标所在屏幕 | 可选：useCursorScreenDetection |
| 跨显示器移动光标 | moveCursorAcrossDisplays |

**验收标准**：多显示器环境下，Next/Previous Display 正确移动窗口。

---

### 阶段 7：扩展功能（3–5 天）

#### 7.1 尺寸调整

| 任务 | 说明 |
|------|------|
| LargerCalculator | 宽高各增加 sizeOffset |
| SmallerCalculator | 宽高各减少，下限 minimumWindowWidth/Height |
| 窗帘式调整 | 贴边时只调整对边 |

#### 7.2 六分之一、九分之一、八分之一

| 任务 | 说明 |
|------|------|
| 六分之一 | 6 个计算器 |
| 九分之一 | 9 个计算器 |
| 八分之一 | 8 个计算器 |
| 可通过 Extra Shortcuts 配置 | 不全部放入默认菜单 |

#### 7.3 其他

| 任务 | 说明 |
|------|------|
| AlmostMaximize | 90% 屏幕 |
| MaximizeHeight | 仅最大化高度 |
| 双击标题栏 | 可选：最大化/恢复 |
| 平铺/层叠 | tileAll, cascadeAll（可选） |

---

### 阶段 8：URL 方案与自动化（1–2 天）

| 任务 | 说明 |
|------|------|
| 注册 URL 协议 | rectangle:// 关联到应用 |
| 命令行参数 | 解析 execute-action?name=left-half |
| 不激活窗口 | 后台执行 |

**注册表示例：**

```
HKEY_CURRENT_USER\Software\Classes\rectangle
  Default = "URL:Rectangle Protocol"
  URL Protocol = ""
  shell\open\command = "C:\...\Rectangle.exe" "%1"
```

---

### 阶段 9：打包与发布（2–3 天）

| 任务 | 说明 |
|------|------|
| 安装程序 | MSIX 或 Inno Setup |
| 自动更新 | Squirrel / 自建 API |
| 签名 | 代码签名证书（可选） |
| 商店 | Microsoft Store 或 GitHub Releases |

---

## 六、风险与应对

| 风险 | 影响 | 应对 |
|------|------|------|
| 管理员窗口无法移动 | 部分应用无法管理 | 提供「以管理员运行」选项，文档说明 |
| 与 Windows 11 Snap 冲突 | 用户体验混乱 | 文档说明关闭系统 Snap，或使用不同快捷键 |
| 某些应用窗口无法移动 | 特殊窗口类型 | 过滤 + 兜底逻辑，记录日志 |
| 全局钩子被杀毒软件拦截 | 安装/运行失败 | 提供签名，加入白名单说明 |
| DPI 多显示器坐标错误 | 布局错位 | 严格使用 PerMonitorV2，测试多场景 |

---

## 七、时间估算

| 阶段 | 预估工时 | 累计 |
|------|----------|------|
| 阶段 0：项目初始化 | 1–2 天 | 2 天 |
| 阶段 1：核心窗口管理 | 3–5 天 | 7 天 |
| 阶段 2：全局快捷键 | 2–3 天 | 10 天 |
| 阶段 3：设置与配置 | 2–3 天 | 13 天 |
| 阶段 4：拖拽吸附 | 4–6 天 | 19 天 |
| 阶段 5：三分之一与高级布局 | 2–3 天 | 22 天 |
| 阶段 6：多显示器 | 1–2 天 | 24 天 |
| 阶段 7：扩展功能 | 3–5 天 | 29 天 |
| 阶段 8：URL 方案 | 1–2 天 | 31 天 |
| 阶段 9：打包发布 | 2–3 天 | 34 天 |

**总计**：约 6–7 周（按单人全职开发估算）。MVP（阶段 0–4）约 3–4 周。

---

## 八、附录

### A. 推荐学习资源

**Win32 / 窗口管理：**

- [Win32 API - SetWindowPos](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos)
- [PowerToys FancyZones 源码](https://github.com/microsoft/PowerToys/tree/main/src/modules/fancyzones)

**WinUI 3 专项：**

- [WinUI 3 全局快捷键](https://whid.eu/2022/05/13/chapter-5-add-global-hot-key-in-winui-3/)
- [WinUI 3 系统托盘 - H.NotifyIcon](https://github.com/HavenDV/H.NotifyIcon)
- [CsWin32 - Microsoft 官方 P/Invoke 源生成器](https://github.com/microsoft/CsWin32)

### B. 配置 JSON 兼容格式

```json
{
  "bundleId": "com.knollsoft.Rectangle.Windows",
  "version": "1.0.0",
  "shortcuts": {
    "leftHalf": { "keyCode": 37, "modifierFlags": 786432 },
    "rightHalf": { "keyCode": 39, "modifierFlags": 786432 }
  },
  "defaults": {
    "launchOnLogin": { "bool": true },
    "gapSize": { "float": 10 }
  }
}
```

### C. 键码参考（Windows Virtual Key Codes）

| 键 | VK 值 |
|----|-------|
| ← | 0x25 (37) |
| → | 0x27 (39) |
| ↑ | 0x26 (38) |
| ↓ | 0x28 (40) |
| Enter | 0x0D (13) |
| Delete | 0x2E (46) |
