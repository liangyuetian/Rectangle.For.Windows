# 全 AI 实现 Rectangle Windows 版实操指南

> 本文档指导如何**完全依赖 AI** 实现 Rectangle Windows 版本，让 AI 工作更顺利、产出更可靠。

---

## 一、开始前的准备

### 1.1 环境与工具

| 项目 | 建议 |
|------|------|
| **AI 工具** | Cursor（推荐，可 @ 文件）、GitHub Copilot、Claude 等 |
| **IDE** | Visual Studio 2022 或 VS Code + C# 扩展 |
| **系统** | Windows 10/11，已安装 .NET 8 SDK |
| **工作目录** | 新建空文件夹，或 fork Rectangle 仓库单独建 `Rectangle.Windows` 子目录 |

### 1.2 必须准备好的文档（给 AI 当上下文）

把这些文件放在 AI 能访问的位置（如同一仓库的 `docs/`），并在对话中通过 `@` 引用：

| 文件 | 用途 |
|------|------|
| `docs/windows-implementation-plan.md` | 实现计划、技术栈、API 映射 |
| `docs/windows-ai-tech-analysis.md` | 技术选型、AI 适配策略 |
| `docs/02-window-actions.md` | 窗口操作完整列表 |
| `Rectangle/WindowCalculation/LeftRightHalfCalculation.swift` | 布局计算参考（可选） |

### 1.3 你的角色

- **任务拆分**：把大功能拆成小任务，一次只让 AI 做一件事
- **提供上下文**：每次对话附上相关文档或代码
- **验收**：编译、运行、简单测试
- **纠错**：发现错误时，把报错信息贴给 AI 让它修

---

## 二、推荐实现顺序（严格按序）

按下面顺序推进，每步完成并验证后再进入下一步。

```
阶段 0: 项目骨架
    ↓
阶段 1a: Win32 封装（CsWin32 + 窗口操作）
    ↓
阶段 1b: 布局计算 + WindowManager
    ↓
阶段 2: 全局快捷键
    ↓
阶段 3: 托盘 + 设置界面
    ↓
阶段 4: 拖拽吸附（可选延后）
    ↓
阶段 5+: 扩展功能
```

---

## 三、分步提示词模板

### 阶段 0：创建 WinUI 3 项目骨架

**提示词 1：**

```
请创建一个 WinUI 3 应用项目，要求：
- 使用 .NET 8
- 项目名：Rectangle.Windows
- 使用 MSIX 打包（packaged）
- 添加 NuGet 包：Microsoft.Windows.CsWin32、H.NotifyIcon.WinUI
- 创建以下目录结构：
  - Services/（WindowManager, HotkeyManager, ConfigService）
  - Core/（WindowAction 枚举、RectCalculator 基类）
  - Views/（SettingsPage）
  - Win32/（用于 CsWin32 生成的代码）
- 在项目根目录创建 NativeMethods.txt，内容先留空
- 主窗口启动时最小化到托盘，不显示窗口
```

**验收**：`dotnet build` 通过，运行后托盘出现图标。

---

### 阶段 1a：Win32 封装

**提示词 2：**

```
参考 @docs/windows-implementation-plan.md 第三节的 Win32 API，
使用 CsWin32 实现窗口操作封装。

1. 在 NativeMethods.txt 中添加以下 API 名（每行一个）：
   GetForegroundWindow
   GetWindowRect
   SetWindowPos
   MonitorFromWindow
   GetMonitorInfoW
   EnumDisplayMonitors
   SystemParametersInfoW

2. 创建 Services/Win32WindowService.cs，实现：
   - GetForegroundWindowHandle() -> IntPtr
   - GetWindowRect(IntPtr hwnd) -> (int x, int y, int width, int height)
   - SetWindowRect(IntPtr hwnd, int x, int y, int width, int height)
   - GetWorkAreaFromWindow(IntPtr hwnd) -> RECT（获取窗口所在显示器的工作区，排除任务栏）

使用 CsWin32 生成的 P/Invoke，不要手写 DllImport。
需要 RECT、MONITORINFO 等结构体时，也通过 CsWin32 生成。
```

**验收**：能获取前台窗口句柄，能读取其位置和大小。

**提示词 3（若 GetWorkArea 复杂）：**

```
在 Win32WindowService 中实现 GetWorkAreaFromWindow：
1. 用 MonitorFromWindow 获取窗口所在显示器
2. 用 GetMonitorInfo 获取 MONITORINFO，取 rcWork 作为工作区
3. 返回 RECT 结构，注意坐标是屏幕坐标
```

---

### 阶段 1b：布局计算 + WindowManager

**提示词 4：**

```
参考 @docs/02-window-actions.md 和 macOS Rectangle 的 LeftRightHalfCalculation，
实现 Core/RectCalculator 布局计算。

1. 定义接口 IRectCalculator：
   Rect Calculate(Rect workArea, Rect currentWindow, WindowAction action);

2. 实现 LeftHalfCalculator：返回工作区左半部分的矩形
3. 实现 RightHalfCalculator：返回工作区右半部分
4. 实现 MaximizeCalculator：返回整个工作区
5. 实现 CenterCalculator：窗口居中，保持当前大小
6. 实现 TopLeftCalculator、TopRightCalculator、BottomLeftCalculator、BottomRightCalculator：四角四分之一

使用 System.Windows.Rect 或自定义 Rect 结构，坐标与 Win32 一致（左上角为原点）。
```

**提示词 5：**

```
实现 Services/WindowManager.cs：

1. 依赖 Win32WindowService 和 IRectCalculator
2. 实现 Execute(WindowAction action)：
   - 获取前台窗口句柄
   - 若为 IntPtr.Zero，播放系统提示音并返回
   - 根据 action 选择对应的 Calculator
   - 获取窗口所在显示器工作区
   - 计算目标矩形
   - 调用 SetWindowRect 移动窗口
3. 实现 WindowHistory：字典存储 windowId -> 上次 Rect，用于 Restore
4. Restore 时从历史恢复

WindowAction 枚举至少包含：LeftHalf, RightHalf, Maximize, Center, Restore, TopLeft, TopRight, BottomLeft, BottomRight
```

**验收**：通过临时按钮或调试代码调用 `WindowManager.Execute(WindowAction.LeftHalf)`，当前窗口能正确左半屏。

---

### 阶段 2：全局快捷键

**提示词 6：**

```
参考 https://whid.eu/2022/05/13/chapter-5-add-global-hot-key-in-winui-3/
实现 WinUI 3 全局快捷键。

1. 创建 Services/HotkeyManager.cs
2. 需要 MainWindow 的句柄：用 WinRT.Interop.WindowNative.GetWindowHandle(window)
3. 使用 RegisterHotKey 注册 Ctrl+Alt+Left 为左半屏
4. 通过窗口子类化或 HwndMessageHook 接收 WM_HOTKEY (0x312)
5. 收到后调用 WindowManager.Execute(WindowAction.LeftHalf)
6. 在 App 启动时初始化 HotkeyManager，传入 MainWindow

注意：MainWindow 可以隐藏，但必须存在才能接收消息。
```

**提示词 7（若消息接收失败）：**

```
我的 WinUI 3 应用无法收到 WM_HOTKEY 消息。当前代码是 [粘贴代码]。

请检查：
1. 窗口句柄是否正确获取
2. 是否需要在窗口显示后才能注册热键
3. 是否需要使用 SetWinEventHook 或 WndProc 子类化
4. 参考 https://whid.eu/2022/05/13/chapter-5-add-global-hot-key-in-winui-3/ 的完整实现
```

**验收**：应用在后台时，按 Ctrl+Alt+Left 能使当前窗口左半屏。

---

### 阶段 3：托盘 + 设置界面

**提示词 8：**

```
使用 H.NotifyIcon.WinUI 实现系统托盘：

1. 在 MainWindow 或 App 中集成 TaskbarIcon
2. 托盘图标点击：显示菜单，包含「设置」「退出」
3. 点击「设置」打开 SettingsWindow
4. 点击「退出」关闭应用
5. 应用启动时主窗口隐藏，只显示托盘图标

参考 H.NotifyIcon 的 WinUI 3 示例。
```

**提示词 9：**

```
创建设置界面 SettingsPage.xaml：

1. 使用 NavigationView 或简单布局
2. 快捷键标签页：列表显示 LeftHalf -> Ctrl+Alt+Left，可点击修改（先只读即可）
3. 通用标签页：开机启动（ToggleSwitch）、窗口间隙（Slider 0-20）
4. 配置保存到 %APPDATA%\Rectangle\config.json
5. 使用 System.Text.Json 序列化，结构参考 @docs/windows-implementation-plan.md 附录 B
```

---

### 阶段 4：拖拽吸附（可选）

**提示词 10：**

```
实现拖拽吸附到屏幕边缘：

1. 使用 SetWindowsHookEx(WH_MOUSE_LL) 监听全局鼠标
2. 检测鼠标按下时是否在某个窗口的标题栏区域（可用 GetWindowRect + 估算标题栏高度约 30px）
3. 若在拖拽中（按下后移动），根据光标位置判断是否靠近屏幕边缘（如 20px 内）
4. 靠近左边缘时显示半透明预览矩形（可新建一个透明置顶窗口）
5. 鼠标释放时，若在吸附区域，调用 WindowManager.Execute(对应 Action)

边缘映射：左->LeftHalf, 右->RightHalf, 上->Maximize, 四角->四分之一
```

---

## 四、每次对话的最佳实践

### 4.1 单次对话只做一件事

| ✅ 好 | ❌ 差 |
|------|------|
| 「实现 LeftHalfCalculator」 | 「实现所有布局计算和 WindowManager」 |
| 「添加 CsWin32 的 GetForegroundWindow」 | 「实现整个 Win32 封装」 |
| 「修复 SetWindowPos 返回 false 的问题」 | 「修一下代码」 |

### 4.2 提供足够的上下文

```
✅ 好的提示词：
「在 WindowManager.Execute 中，当 GetForegroundWindow 返回 IntPtr.Zero 时，
请添加 System.Media.SystemSounds.Beep.Play() 并 return。
当前代码见 @Services/WindowManager.cs」

❌ 差的提示词：
「加个错误处理」
```

### 4.3 出错时把完整信息给 AI

```
✅ 好的反馈：
「编译报错：CS0246 找不到类型或命名空间名 "RECT"。
当前 NativeMethods.txt 内容是 [粘贴]，
Win32WindowService.cs 中使用了 PInvoke.RECT。」
```

### 4.4 分步确认

每完成一个小模块，先编译运行，再继续。不要一次让 AI 写很多文件，否则出错难以定位。

---

## 五、常见问题与应对

### 5.1 CsWin32 找不到类型

- **原因**：NativeMethods.txt 中未声明，或声明名与 Win32 不一致
- **应对**：在 NativeMethods.txt 中补充 API 名，如 `GetMonitorInfoW`（注意 W 后缀）
- **参考**：https://github.com/microsoft/CsWin32/blob/main/docs/readme.md

### 5.2 热键无响应

- **检查**：窗口句柄是否为 0、是否在窗口 Show 之后注册
- **应对**：让 AI 参考 whid.eu 教程，确保有 WndProc 或消息钩子处理 WM_HOTKEY

### 5.3 窗口移动后位置不对

- **检查**：坐标是屏幕坐标还是客户区坐标、DPI 是否考虑
- **应对**：确认 GetWindowRect 和 SetWindowPos 使用同一坐标系，检查 app.manifest 的 DPI 配置

### 5.4 AI 生成的代码与现有代码风格不一致

- **应对**：在提示词中加「保持与项目中现有代码风格一致」「使用 file-scoped namespace」

### 5.5 AI 一次生成太多，难以审查

- **应对**：明确说「请只实现 LeftHalfCalculator，不要实现其他 Calculator」

---

## 六、验收检查清单

每完成一个阶段，逐项确认：

### 阶段 0
- [ ] 项目可编译
- [ ] 运行后托盘有图标
- [ ] 点击托盘可显示菜单

### 阶段 1
- [ ] 能获取前台窗口
- [ ] 左半屏、右半屏、最大化、四角、居中、恢复均正确
- [ ] 多显示器下窗口在正确显示器上移动

### 阶段 2
- [ ] Ctrl+Alt+Left 等快捷键在任意应用前台时生效
- [ ] 应用在后台时快捷键仍有效

### 阶段 3
- [ ] 设置可保存并加载
- [ ] 开机启动选项生效（若实现）

### 阶段 4
- [ ] 拖到左边缘松手可左半屏
- [ ] 有预览效果（可选）

---

## 七、提示词速查

| 场景 | 可复制使用的开头 |
|------|------------------|
| 新建文件 | 「请创建 Core/LeftHalfCalculator.cs，实现...」 |
| 修改现有 | 「请修改 @Services/WindowManager.cs，在 Execute 方法中...」 |
| 修复错误 | 「编译报错 [粘贴错误]，相关代码在 @xxx，请修复」 |
| 补充实现 | 「参考 @docs/windows-implementation-plan.md 第 X 节，实现...」 |
| 风格统一 | 「使用 C# 12 语法，file-scoped namespace，保持与项目一致」 |

---

## 八、总结

| 原则 | 说明 |
|------|------|
| **小步快跑** | 一次一个类/一个方法，验证后再继续 |
| **上下文优先** | 用 @ 引用文档和代码，减少 AI 猜测 |
| **明确输入输出** | 告诉 AI 输入是什么、输出是什么 |
| **出错即反馈** | 把完整报错和代码贴给 AI，让它修 |
| **人工把关** | P/Invoke、句柄、坐标等关键逻辑要自己看一眼 |

按本指南的顺序和提示词推进，AI 可以完成绝大部分实现工作，你主要负责拆分任务、提供上下文和验收。
