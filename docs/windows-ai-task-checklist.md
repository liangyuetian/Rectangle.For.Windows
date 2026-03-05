# Rectangle Windows 版 - AI 自主实现任务清单（详细版）

> **面向 AI 的原子级任务清单**。每项任务包含：精确动作、完整代码示例、可执行验证命令、预期输出、失败处理、禁止事项。

---

## 文档使用规则（AI 必须遵守）

1. **严格按 TASK 编号顺序执行**，不得跳过或调序
2. **每项任务完成后**：在 `[ ]` 中改为 `[x]`，执行验证，验证通过才进入下一项
3. **验证失败时**：执行「若失败」步骤，若仍失败则执行「自修正」并重试
4. **禁止**：合并多个任务、一次性生成多个文件（除非任务明确要求）、猜测不确定的 API
5. **必须**：使用任务中给出的精确路径、命令、类名；若需偏离，需在任务中说明理由

### 明确禁止事项（DO NOT）

- 不要使用 `FindWindow` 获取前台窗口，必须用 `GetForegroundWindow`
- 不要关闭 MainWindow（Close），热键需其存在才能接收消息，只能 Hide
- 不要手写 `[DllImport]`，必须用 CsWin32 生成的 API
- 不要在未验证的情况下进入下一任务
- 不要修改任务中指定的文件路径（如 `src/Rectangle.Windows/` 不可改为其他）

---

## 工作目录约定

- **根目录**：`Rectangle.Windows/`（与本文档所在仓库同级或独立）
- **所有路径**：相对于 `Rectangle.Windows/`，除非标注为绝对路径
- **执行命令**：默认在 `Rectangle.Windows/` 下执行

---

## 前置条件检查

执行 TASK-000 前，确认：

- [ ] 当前目录为 `Rectangle.Windows/` 或可创建该目录
- [ ] `dotnet --version` 输出 8.x
- [ ] 操作系统为 Windows 10/11，非 WSL

---

## TASK-000：读取上下文文档

- [ ] **TASK-000** 读取并理解上下文文档

### 动作（分步执行）

1. 在对话中通过 `@` 引用并读取：`docs/windows-implementation-plan.md`
2. 读取：`docs/windows-ai-tech-analysis.md`
3. 读取：`docs/02-window-actions.md`
4. 实现快捷键与托盘菜单时，参考本文档 **附录 A：快捷键功能对照表**（基于 macOS 版托盘菜单）
5. 实现设置窗口时，参考本文档 **附录 B：偏好设置 UI 结构**（基于 macOS 版偏好设置）
6. 添加图片资源时，参考本文档 **附录 D：图片资源**，从 macOS 版对应文件夹获取
7. 需参考 macOS 源码或资源时，见本文档 **附录 E：macOS 参考文件索引**

### 必须掌握的关键信息

- 技术栈：C# + WinUI 3 + .NET 8 + CsWin32 + H.NotifyIcon.WinUI
- 窗口操作：GetForegroundWindow → GetWindowRect → 计算目标 RECT → SetWindowPos
- 工作区：GetMonitorInfo 的 rcWork，排除任务栏
- 左半屏：workArea 左半部分，宽度 = (Right - Left) / 2
- WinUI 3 热键：必须通过 Win32 子类化接收 WM_HOTKEY (0x312)，参考 https://whid.eu/2022/05/13/chapter-5-add-global-hot-key-in-winui-3/

### 验证

- 能回答：左半屏时目标矩形宽度 = workArea 宽度的 1/2
- 能回答：WM_HOTKEY 的数值为 0x312

### 若失败

- 若无法读取文档：请求用户提供文档或确认路径

---

## 第一部分：项目架构与骨架

### TASK-001：创建解决方案文件

- [ ] **TASK-001** 创建解决方案文件

#### 动作

1. 在项目根目录执行（若 `Rectangle.Windows` 不存在则先创建）：
   ```bash
   cd Rectangle.Windows
   dotnet new sln -n Rectangle.Windows
   ```
2. 若目录已存在且为空，可直接执行：
   ```bash
   dotnet new sln -n Rectangle.Windows
   ```

#### 验证

```bash
dotnet sln list
```

- **预期输出**：无项目列表（仅 Solution 文件头），或 `Project(s)` 为空
- **若输出包含项目**：检查是否在正确的 sln 目录

---

### TASK-002：创建 WinUI 3 项目

- [ ] **TASK-002** 创建 WinUI 3 项目（前置：TASK-001）

#### 动作

1. 在 `Rectangle.Windows/` 下执行：
   ```bash
   dotnet new winui -n Rectangle.Windows -o src/Rectangle.Windows -f net8-windows
   ```
2. 若模板 `winui` 不存在，尝试：
   ```bash
   dotnet new list
   ```
   查找包含 `winui` 或 `WinUI` 的模板名，可能为 `winui3` 或 `winui-desktop`
3. 备选（若 `dotnet new winui` 失败）：
   ```bash
   dotnet new install Microsoft.WindowsAppSDK.Templates
   dotnet new winui -n Rectangle.Windows -o src/Rectangle.Windows -f net8-windows
   ```

#### 验证

```bash
dotnet build src/Rectangle.Windows/Rectangle.Windows.csproj
```

- **预期输出**：`Build succeeded`，无错误
- **若失败**：常见为缺少 workload，执行 `dotnet workload install windowsdesktop`

---

### TASK-003：将项目加入解决方案

- [ ] **TASK-003** 将项目加入解决方案

#### 动作

```bash
dotnet sln add src/Rectangle.Windows/Rectangle.Windows.csproj
```

#### 验证

```bash
dotnet sln list
```

- **预期输出**：包含 `Rectangle.Windows` 项目路径

---

### TASK-004：添加 CsWin32 NuGet 包

- [ ] **TASK-004** 添加 CsWin32

#### 动作

```bash
dotnet add src/Rectangle.Windows/Rectangle.Windows.csproj package Microsoft.Windows.CsWin32
```

#### 验证

1. 打开 `src/Rectangle.Windows/Rectangle.Windows.csproj`
2. 确认存在：`<PackageReference Include="Microsoft.Windows.CsWin32" Version="..." />`
3. 执行 `dotnet restore src/Rectangle.Windows/Rectangle.Windows.csproj` 无错误

---

### TASK-005：添加 H.NotifyIcon.WinUI NuGet 包

- [ ] **TASK-005** 添加 H.NotifyIcon.WinUI

#### 动作

```bash
dotnet add src/Rectangle.Windows/Rectangle.Windows.csproj package H.NotifyIcon.WinUI
```

#### 验证

- `dotnet restore` 成功
- csproj 中有 `H.NotifyIcon.WinUI` 引用

---

### TASK-006：创建 NativeMethods.txt

- [ ] **TASK-006** 创建 CsWin32 配置

#### 动作

1. 创建文件：`src/Rectangle.Windows/NativeMethods.txt`
2. 内容（每行一个，无空行，无多余空格）：
   ```
   GetForegroundWindow
   GetWindowRect
   SetWindowPos
   MonitorFromWindow
   GetMonitorInfoW
   EnumDisplayMonitors
   ```

#### 重要

- 文件必须位于**项目目录**（与 .csproj 同级），即 `src/Rectangle.Windows/`
- 使用 `GetMonitorInfoW`（W 后缀），不要用 `GetMonitorInfo`（会生成 A 版本）

#### 验证

```bash
dotnet build src/Rectangle.Windows/Rectangle.Windows.csproj
```

- **预期**：编译成功
- 检查 `obj/` 目录下是否有 CsWin32 生成的 .cs 文件（如 `Windows.Win32.g.cs` 或类似）
- CsWin32 生成到 `Windows.Win32` 命名空间，使用 `using Windows.Win32;` 后可直接调用 `PInvoke.GetForegroundWindow()`

---

### TASK-007：创建目录结构

- [ ] **TASK-007** 创建空目录与占位文件

#### 动作

1. 在 `src/Rectangle.Windows/` 下创建目录：
   - `Services/`
   - `Core/`
   - `Views/`
   - `Core/Calculators/`（用于后续布局计算器）

2. 创建占位文件 `Services/Placeholder.cs`：
   ```csharp
   namespace Rectangle.Windows.Services;

   // Placeholder for Services directory
   ```

3. 创建 `Core/Placeholder.cs`：
   ```csharp
   namespace Rectangle.Windows.Core;

   // Placeholder for Core directory
   ```

4. 创建 `Views/Placeholder.cs`：
   ```csharp
   namespace Rectangle.Windows.Views;

   // Placeholder for Views directory
   ```

#### 验证

```bash
dotnet build src/Rectangle.Windows/Rectangle.Windows.csproj
```

- **预期**：Build succeeded
- 确认目录存在：`ls src/Rectangle.Windows/Services` 等

---

### TASK-007a：从 macOS 版复制图片资源

- [ ] **TASK-007a** 复制图片资源（参考附录 D、附录 E）

#### 前置条件

- 仓库包含 macOS 版 Rectangle 源码（`Rectangle/` 目录存在），图标见 `Rectangle/Assets.xcassets/`
- TASK-002 已完成，`src/Rectangle.Windows/` 存在

#### 动作

1. 创建 `src/Rectangle.Windows/Assets/WindowPositions/` 目录
2. 从 `Rectangle/Assets.xcassets/WindowPositions/*.imageset/*.png` 复制所有 .png 到 `Assets/WindowPositions/`（见附录 D.4 脚本）
3. 从 `Rectangle/Assets.xcassets/StatusTemplate.imageset/RectangleStatusTemplate.png` 复制到 `Assets/`，使用 ImageMagick 或在线工具转为 `AppIcon.ico`，或使用 WinUI 模板默认图标
4. 在 .csproj 中确保 `Assets/` 下文件为 Content 且 CopyToOutputDirectory 正确（WinUI 模板通常已配置）

#### 验证

- `Assets/WindowPositions/leftHalfTemplate.png` 等文件存在
- 托盘图标可正常显示

---

## 第二部分：核心数据模型

### TASK-008：创建 WindowAction 枚举

- [ ] **TASK-008** 创建 WindowAction.cs（参考 macOS：`Rectangle/WindowAction.swift`）

#### 动作

1. 创建文件：`src/Rectangle.Windows/Core/WindowAction.cs`
2. 完整内容：
   ```csharp
   namespace Rectangle.Windows.Core;

   public enum WindowAction
   {
       // 半屏
       LeftHalf,
       RightHalf,
       CenterHalf,
       TopHalf,
       BottomHalf,
       // 四角
       TopLeft,
       TopRight,
       BottomLeft,
       BottomRight,
       // 三分之一
       FirstThird,
       CenterThird,
       LastThird,
       FirstTwoThirds,
       CenterTwoThirds,
       LastTwoThirds,
       // 四等分（子菜单）
       FirstFourth,
       SecondFourth,
       ThirdFourth,
       LastFourth,
       FirstThreeFourths,
       CenterThreeFourths,
       LastThreeFourths,
       // 六等分（子菜单）
       TopLeftSixth,
       TopCenterSixth,
       TopRightSixth,
       BottomLeftSixth,
       BottomCenterSixth,
       BottomRightSixth,
       // 移动到边缘（子菜单）
       MoveLeft,
       MoveRight,
       MoveUp,
       MoveDown,
       // 最大化与缩放
       Maximize,
       AlmostMaximize,
       MaximizeHeight,
       Larger,
       Smaller,
       Center,
       Restore,
       // 显示器
       NextDisplay,
       PreviousDisplay
   }
   ```

#### 验证

- `dotnet build` 成功
- 无 CS0246 等类型未找到错误

---

### TASK-009：创建 WorkArea 与 WindowRect 结构

- [ ] **TASK-009** 创建矩形辅助结构

#### 动作

1. 创建文件：`src/Rectangle.Windows/Core/WindowRect.cs`
2. 内容：
   ```csharp
   namespace Rectangle.Windows.Core;

   public readonly record struct WorkArea(int Left, int Top, int Right, int Bottom)
   {
       public int Width => Right - Left;
       public int Height => Bottom - Top;
   }

   public readonly record struct WindowRect(int X, int Y, int Width, int Height)
   {
       public int Left => X;
       public int Top => Y;
       public int Right => X + Width;
       public int Bottom => Y + Height;
   }
   ```

#### 验证

- 能实例化：`var wa = new WorkArea(0, 0, 1920, 1080);` 且 `wa.Width == 1920`

---

## 第三部分：Win32 窗口服务

### TASK-010：创建 Win32WindowService

- [ ] **TASK-010** 创建 Win32WindowService（前置：TASK-006）

创建 `Services/Win32WindowService.cs`，实现 `GetForegroundWindowHandle()`：`return (nint)PInvoke.GetForegroundWindow().Value`。需 `using Windows.Win32`。验证：有前台窗口时返回非零句柄。

---

### TASK-011：实现 GetWindowRect

在 Win32WindowService 添加：`PInvoke.GetWindowRect((HWND)hwnd, out var rect); return (rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);`。需 `using Windows.Win32.Graphics.Gdi`。

---

### TASK-012：实现 SetWindowRect

在 Win32WindowService 添加：`PInvoke.SetWindowPos((HWND)hwnd, HWND.Null, x, y, width, height, SET_WINDOW_POS_FLAGS.SWP_NOZORDER)`，返回 `result.Value`。

---

### TASK-013：确认 GetMonitorInfoW 可用

确认 NativeMethods.txt 含 GetMonitorInfoW、MonitorFromWindow（TASK-006 已添加）。可调用 `PInvoke.GetMonitorInfoW`、`PInvoke.MonitorFromWindow`。

---

### TASK-014：实现 GetWorkAreaFromWindow

`MonitorFromWindow(MONITOR_DEFAULTTONEAREST)` → `GetMonitorInfoW` 取 rcWork。**必须**设置 `mi.cbSize = (uint)Marshal.SizeOf<MONITORINFO>()`。返回 WorkArea(rcWork.left, top, right, bottom)。

---

## 第四部分：布局计算器

### TASK-015：创建 IRectCalculator 接口

- [ ] **TASK-015** 创建 IRectCalculator

#### 动作

1. 创建 `src/Rectangle.Windows/Core/IRectCalculator.cs`：
   ```csharp
   namespace Rectangle.Windows.Core;

   public interface IRectCalculator
   {
       WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action);
   }
   ```

2. 若 `WindowRect` 与 `WorkArea` 在 `Core` 命名空间，无需额外 using

#### 验证

- 编译通过

---

### TASK-016～021：创建基础计算器

- [ ] **TASK-016** LeftHalfCalculator（参考 `Rectangle/WindowCalculation/LeftRightHalfCalculation.swift`）：`width=workArea.Width/2; return new WindowRect(workArea.Left, workArea.Top, width, workArea.Height)`
- [ ] **TASK-017** RightHalfCalculator：同上，`x=workArea.Left+width`
- [ ] **TASK-018** TopHalfCalculator、BottomHalfCalculator：`h=workArea.Height/2`，Top 用 Top，Bottom 用 Top+h
- [ ] **TASK-019** TopLeft、TopRight、BottomLeft、BottomRight：`w=Width/2,h=Height/2`，四角坐标组合
- [ ] **TASK-020** MaximizeCalculator：`return new WindowRect(workArea.Left, workArea.Top, workArea.Width, workArea.Height)`
- [ ] **TASK-021** CenterCalculator：`x=Left+(Width-current.Width)/2, y=Top+(Height-current.Height)/2`，保持 current 尺寸

每个文件：`namespace Rectangle.Windows.Core.Calculators;`，`public class XxxCalculator : IRectCalculator`。验证：LeftHalf 对 1920 宽返回 960；四角覆盖 workArea 无重叠。

---

### TASK-022：创建 CalculatorFactory

- [ ] **TASK-022** 创建 CalculatorFactory

#### 动作

1. 创建 `src/Rectangle.Windows/Core/CalculatorFactory.cs`：
   ```csharp
   namespace Rectangle.Windows.Core;

   public class CalculatorFactory
   {
       private readonly Dictionary<WindowAction, IRectCalculator> _calculators = new()
       {
           [WindowAction.LeftHalf] = new Calculators.LeftHalfCalculator(),
           [WindowAction.RightHalf] = new Calculators.RightHalfCalculator(),
           [WindowAction.TopHalf] = new Calculators.TopHalfCalculator(),
           [WindowAction.BottomHalf] = new Calculators.BottomHalfCalculator(),
           [WindowAction.TopLeft] = new Calculators.TopLeftCalculator(),
           [WindowAction.TopRight] = new Calculators.TopRightCalculator(),
           [WindowAction.BottomLeft] = new Calculators.BottomLeftCalculator(),
           [WindowAction.BottomRight] = new Calculators.BottomRightCalculator(),
           [WindowAction.Maximize] = new Calculators.MaximizeCalculator(),
           [WindowAction.Center] = new Calculators.CenterCalculator(),
       };

       public IRectCalculator? GetCalculator(WindowAction action)
       {
           return _calculators.TryGetValue(action, out var c) ? c : null;
       }
   }
   ```

2. TASK-019 已创建 TopLeft、TopRight、BottomLeft、BottomRight 四个 Calculator，此处直接引用

#### 验证

- `GetCalculator(WindowAction.LeftHalf)` 返回非 null，且类型为 LeftHalfCalculator

---

## 第五部分：WindowManager

### TASK-023：创建 WindowHistory

- [ ] **TASK-023** 创建 WindowHistory

#### 动作

1. 创建 `src/Rectangle.Windows/Core/WindowHistory.cs`：
   ```csharp
   namespace Rectangle.Windows.Core;

   public class WindowHistory
   {
       private readonly Dictionary<nint, (int X, int Y, int W, int H)> _history = new();

       public void Save(nint hwnd, int x, int y, int w, int h) => _history[hwnd] = (x, y, w, h);

       public bool TryGet(nint hwnd, out (int X, int Y, int W, int H) rect) => _history.TryGetValue(hwnd, out rect);

       public void Remove(nint hwnd) => _history.Remove(hwnd);
   }
   ```

#### 验证

- Save 后 TryGet 能读回相同值

---

### TASK-024：创建 WindowManager 并实现 Execute（非 Restore）

- [ ] **TASK-024** 创建 WindowManager.Execute

#### 动作

1. 创建 `src/Rectangle.Windows/Services/WindowManager.cs`：
   ```csharp
   using Rectangle.Windows.Core;

   namespace Rectangle.Windows.Services;

   public class WindowManager
   {
       private readonly Win32WindowService _win32;
       private readonly CalculatorFactory _factory;
       private readonly WindowHistory _history;

       public WindowManager(Win32WindowService win32, CalculatorFactory factory, WindowHistory history)
       {
           _win32 = win32;
           _factory = factory;
           _history = history;
       }

       public void Execute(WindowAction action)
       {
           if (action == WindowAction.Restore)
           {
               ExecuteRestore();
               return;
           }

           var hwnd = _win32.GetForegroundWindowHandle();
           if (hwnd == 0)
           {
               System.Media.SystemSounds.Beep.Play();
               return;
           }

           var (x, y, w, h) = _win32.GetWindowRect(hwnd);
           var workArea = _win32.GetWorkAreaFromWindow(hwnd);
           var current = new WindowRect(x, y, w, h);

           var calculator = _factory.GetCalculator(action);
           if (calculator == null) return;

           _history.Save(hwnd, x, y, w, h);

           var target = calculator.Calculate(workArea, current, action);
           _win32.SetWindowRect(hwnd, target.X, target.Y, target.Width, target.Height);
       }

       private void ExecuteRestore()
       {
           var hwnd = _win32.GetForegroundWindowHandle();
           if (hwnd == 0) { System.Media.SystemSounds.Beep.Play(); return; }
           if (!_history.TryGet(hwnd, out var rect))
           { System.Media.SystemSounds.Beep.Play(); return; }
           _win32.SetWindowRect(hwnd, rect.X, rect.Y, rect.W, rect.H);
           _history.Remove(hwnd);
       }
   }
   ```

#### 验证

1. 在 MainWindow 中临时创建服务并调用：
   ```csharp
   var win32 = new Win32WindowService();
   var factory = new CalculatorFactory();
   var history = new WindowHistory();
   var wm = new WindowManager(win32, factory, history);
   wm.Execute(WindowAction.LeftHalf);
   ```
2. 先打开记事本并聚焦，再运行应用，记事本应左半屏

---

### TASK-024a：忽略应用列表检查（前置：TASK-036 完成后再实现）

- [ ] **TASK-024a** WindowManager 执行前检查忽略列表（参考 macOS 忽略逻辑，见附录 E）

#### 前置条件

- TASK-036 ConfigService 已完成（需读取 IgnoredApps）

#### 动作

1. 在 Win32WindowService 中添加 `string GetProcessNameFromWindow(nint hwnd)`：使用 `GetWindowThreadProcessId` 获取 PID，`OpenProcess` + `QueryFullProcessImageName` 或 `GetModuleFileNameEx` 获取进程路径，提取文件名（如 `Cursor.exe`）
2. NativeMethods.txt 需添加：`GetWindowThreadProcessId`、`OpenProcess`、`QueryFullProcessImageNameW`（或 `K32GetModuleFileNameExW`）
3. WindowManager 构造函数增加 `ConfigService` 或 `Func<List<string>> getIgnoredApps` 参数
4. 在 `Execute` 开头：若 `getIgnoredApps().Contains(processName, StringComparer.OrdinalIgnoreCase)` 则直接 return，不执行操作

#### 验证

- 将 Cursor 加入忽略列表后，按快捷键对 Cursor 窗口无效果，对记事本有效

---

### TASK-025：TASK-024 已包含 Restore，本任务为验证

- [ ] **TASK-025** 验证 Restore 逻辑

#### 动作

1. 确认 TASK-024 的 ExecuteRestore 已实现
2. 测试：左半屏 → 按 Restore（需通过热键或临时按钮）→ 窗口恢复

#### 验证

- 左半屏后调用 `Execute(WindowAction.Restore)`，窗口恢复原位置

---

### TASK-026：在 App 中注册服务

- [ ] **TASK-026** 注册服务

#### 动作

1. 在 `App.xaml.cs` 的 `OnLaunched` 中创建单例：
   ```csharp
   public static WindowManager? WindowManager { get; private set; }

   protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
   {
       var win32 = new Win32WindowService();
       var factory = new CalculatorFactory();
       var history = new WindowHistory();
       WindowManager = new WindowManager(win32, factory, history);
       // ... 原有 MainWindow 创建逻辑
   }
   ```

2. 在 MainWindow 中可通过 `App.WindowManager` 访问

#### 验证

- MainWindow 可调用 `App.WindowManager!.Execute(WindowAction.LeftHalf)` 且有效

---

## 第六部分：托盘与主窗口

### TASK-027：在 MainWindow 中添加 TaskbarIcon

- [ ] **TASK-027** 添加 TaskbarIcon

#### 动作

1. 在 `MainWindow.xaml` 的根元素内添加（参考 H.NotifyIcon.WinUI 文档）：
   ```xml
   <Window ...>
       <h:TaskbarIcon
           xmlns:h="using:H.NotifyIcon.WinUI"
           ToolTipText="Rectangle"
           IconSource="ms-appx:///Assets/AppIcon.ico" />
   </Window>
   ```

2. **图片资源从 macOS 版获取**（见附录 D、附录 E）：将 `Rectangle/Assets.xcassets/StatusTemplate.imageset/RectangleStatusTemplate.png` 转为 .ico 放入 `src/Rectangle.Windows/Assets/AppIcon.ico`；或使用 `Rectangle/AppIcon.icon` 导出 .ico
3. WinUI 3 模板默认在 `Assets/` 下有图标。若不存在，从 macOS 版复制并转换后放入 `Assets/`
4. H.NotifyIcon.WinUI 的 xmlns 为 `xmlns:h="using:H.NotifyIcon.WinUI"`

#### 验证

- 运行后系统托盘显示图标

---

### TASK-028：托盘菜单

- [ ] **TASK-028** 托盘菜单（结构参考 macOS：`Rectangle/RectangleStatusItem.swift`，见附录 A）

#### 动作

1. 为 TaskbarIcon 设置 ContextMenu，菜单结构如下（与 macOS 托盘菜单对应）：

   **主菜单项（直接执行）：**
   - 左半屏、右半屏、中间半屏、上半屏、下半屏
   - 左上、右上、左下、右下（四角）
   - 左首 1/3、中间 1/3、右首 1/3、左侧 2/3、中间 2/3、右侧 2/3
   - 最大化、接近最大化、最大化高度、放大、缩小、中间、恢复
   - 下一个显示器、上一个显示器

   **子菜单「移动到边缘」：**
   - 向左移动、向右移动、向上移动、向下移动

   **子菜单「四等分」：**
   - 左首 1/4、左二 1/4、右二 1/4、右首 1/4
   - 左侧 3/4、中间 3/4、右侧 3/4

   **子菜单「六等分」：**
   - 左上 1/6、中上 1/6、右上 1/6、左下 1/6、中下 1/6、右下 1/6

   **应用控制：**
   - 「忽略 [当前应用名]」：动态显示，点击后加入忽略列表
   - 分隔线
   - 偏好设置...（Click → OpenSettings()）
   - 查看日志...（Click → 打开日志窗口，可选）
   - 检查更新...（Click → 检查更新，可选）
   - 分隔线
   - 退出 Rectangle（Click → Application.Current.Exit()，显示 ⌘Q 或 Alt+F4）

2. 每个窗口操作菜单项 Click 时调用 `App.WindowManager!.Execute(对应的 WindowAction)`
3. 「忽略 [当前应用名]」：动态获取前台窗口进程名，若已在 IgnoredApps 中则显示「取消忽略 XXX」，否则显示「忽略 XXX」；Click 时切换该应用在 IgnoredApps 中的状态，并调用 ConfigService.Save()
4. 「设置」对应「偏好设置...」，在 MainWindow 中创建空方法 `void OpenSettings() { }`，TASK-039 将实现具体逻辑

#### 验证

- 右键托盘图标显示完整菜单，结构与 macOS 版一致
- 点击各窗口操作项，窗口正确移动/调整
- 点击退出可关闭应用

---

### TASK-029：主窗口启动隐藏

- [ ] **TASK-029** 主窗口隐藏

#### 动作

1. 在 `OnLaunched` 中，创建并激活 MainWindow 后调用其 `Hide()` 方法。WinUI 3 模板中 MainWindow 变量名通常为 `m_window`；若模板使用其他名称（如 `_window`、`mainWindow`），则用该名称调用 Hide()
2. 窗口必须先 Activate 一次再 Hide，否则热键将无法注册。正确顺序：创建 MainWindow → Activate() → 创建 HotkeyManager（注册热键）→ Hide()
2. **禁止**：关闭窗口（Close），否则无法接收热键消息

#### 验证

- 启动后只看到托盘图标，无主窗口显示

---

## 第七部分：全局快捷键

### TASK-030：在 App 中保存 MainWindow 引用

- [ ] **TASK-030** 保存 MainWindow

#### 动作

1. 在 `App.xaml.cs` 中：
   ```csharp
   public static Window? MainWindow { get; private set; }

   // 在 OnLaunched 中创建 m_window 后：
   MainWindow = m_window;
   ```

#### 验证

- 从外部可访问 `App.MainWindow`，非 null

---

### TASK-031：获取 MainWindow 的 HWND

- [ ] **TASK-031** 获取 HWND

#### 动作

1. `WinRT.Interop` 随 Microsoft.WindowsAppSDK 提供，WinUI 3 项目已引用，无需额外 NuGet
2. 使用：
   ```csharp
   var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
   ```
3. 需 `using WinRT.Interop;` 或 `using WinRT;`

#### 验证

- hwnd 非 0 或 IntPtr.Zero

---

### TASK-032：在 NativeMethods.txt 中添加热键与子类化 API

- [ ] **TASK-032** 添加热键与子类化 API

#### 动作

1. 在 `src/Rectangle.Windows/NativeMethods.txt` 末尾添加（每行一个）：
   ```
   RegisterHotKey
   UnregisterHotKey
   SetWindowSubclass
   DefSubclassProc
   RemoveWindowSubclass
   ```
2. SetWindowSubclass、DefSubclassProc、RemoveWindowSubclass 在 ComCtl32.dll 中，CsWin32 会生成到 `ComCtl32.PInvoke` 命名空间
3. 重新 build

#### 验证

- 可调用 `PInvoke.RegisterHotKey`
- 可调用 `ComCtl32.PInvoke.SetWindowSubclass`（CsWin32 将 ComCtl32 API 生成到 ComCtl32 命名空间）

---

### TASK-033：创建 HotkeyManager 并注册热键

- [ ] **TASK-033** 创建 HotkeyManager

#### 前置条件

- TASK-032 已完成，NativeMethods.txt 已包含 RegisterHotKey、UnregisterHotKey、SetWindowSubclass、DefSubclassProc、RemoveWindowSubclass

#### 动作

1. 创建 `src/Rectangle.Windows/Services/HotkeyManager.cs`
2. 构造函数接收 `(nint hwnd, WindowManager wm)`，保存为字段
3. 修饰键（CsWin32 中）：`HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT`
4. 虚拟键：`VK_LEFT` = 0x25, `VK_RIGHT` = 0x27, `VK_UP` = 0x26, `VK_DOWN` = 0x28, `VK_RETURN` = 0x0D, `VK_DELETE` = 0x2E
5. 调用 `PInvoke.RegisterHotKey((HWND)hwnd, id, modifiers, (uint)vk)`，id 从 1 开始递增
6. **WM_HOTKEY 接收**（必须实现）：
   - WinUI 3 不暴露 WndProc，需用 Win32 子类化
   - 在 NativeMethods.txt 添加：`SetWindowSubclass`、`DefSubclassProc`、`RemoveWindowSubclass`（来自 ComCtl32）
   - 必须使用 SetWindowSubclass 子类化 MainWindow 的 HWND，参考：https://whid.eu/2022/05/13/chapter-5-add-global-hot-key-in-winui-3/
   - 子类化回调中：`if (msg == 0x312) { /* WM_HOTKEY */ 根据 (int)wParam 调用 _wm.Execute(...); return 0; } return DefSubclassProc(...)`

#### 验证

- 在 WM_HOTKEY 处理内加 `System.Diagnostics.Debug.WriteLine("Hotkey");`，按 Ctrl+Alt+Left 时输出应出现

---

### TASK-034：实现 WM_HOTKEY 到 WindowAction 的映射

- [ ] **TASK-034** 实现热键映射

#### 动作

1. 在 HotkeyManager 的 WM_HOTKEY 处理中：
   - id 1 → `WindowAction.LeftHalf`
   - id 2 → `WindowAction.RightHalf`
   - id 3 → `WindowAction.TopHalf`
   - id 4 → `WindowAction.BottomHalf`
   - id 5 → `WindowAction.Maximize`
   - id 6 → `WindowAction.Restore`

2. 调用 `_windowManager.Execute(action)`

#### 验证

- 应用在后台，按 Ctrl+Alt+Left，当前前台窗口应左半屏

---

### TASK-035：注册全部默认快捷键并在 App 中初始化 HotkeyManager

- [ ] **TASK-035** 注册全部默认快捷键并初始化（完整对照见附录 A；macOS 默认值参考 `Rectangle/ShortcutManager.swift`）

#### 依赖说明

- **建议顺序**：TASK-036（ConfigService）应在 TASK-035 之前完成，以便 HotkeyManager 可从配置加载快捷键；若未完成，则使用硬编码默认值
- HotkeyManager 构造函数接收 `(nint hwnd, WindowManager wm, ConfigService? config)`：若 `config?.Shortcuts` 非空则按配置注册（跳过 `Enabled=false`），否则使用下述默认值

#### 动作

1. 在 HotkeyManager 构造函数中注册以下热键（id 从 1 递增，修饰键均为 MOD_CONTROL|MOD_ALT，显示器切换加 MOD_WIN）；若从 Config 加载则按 Shortcuts 字典映射：

   **半屏：**
   - Ctrl+Alt+Left (0x25) → LeftHalf
   - Ctrl+Alt+Right (0x27) → RightHalf
   - Ctrl+Alt+Up (0x26) → TopHalf
   - Ctrl+Alt+Down (0x28) → BottomHalf

   **四角：**
   - Ctrl+Alt+U (0x55) → TopLeft
   - Ctrl+Alt+I (0x49) → TopRight
   - Ctrl+Alt+J (0x4A) → BottomLeft
   - Ctrl+Alt+K (0x4B) → BottomRight

   **三分之一：**
   - Ctrl+Alt+D (0x44) → FirstThird
   - Ctrl+Alt+F (0x46) → CenterThird
   - Ctrl+Alt+G (0x47) → LastThird
   - Ctrl+Alt+E (0x45) → FirstTwoThirds
   - Ctrl+Alt+R (0x52) → CenterTwoThirds
   - Ctrl+Alt+T (0x54) → LastTwoThirds

   **最大化与缩放：**
   - Ctrl+Alt+Enter (0x0D) → Maximize
   - Ctrl+Alt+Shift+Up (0x26, MOD_SHIFT) → MaximizeHeight
   - Ctrl+Alt+= (0xBB) → Larger
   - Ctrl+Alt+- (0xBD) → Smaller
   - Ctrl+Alt+C (0x43) → Center
   - Ctrl+Alt+Backspace (0x08) → Restore（避免与系统 Ctrl+Alt+Del 冲突；若需与 macOS 一致可用 Delete 0x2E，但可能无法注册）

   **显示器（加 MOD_WIN）：**
   - Ctrl+Alt+Win+Left (0x25) → PreviousDisplay
   - Ctrl+Alt+Win+Right (0x27) → NextDisplay

2. CenterHalf、AlmostMaximize、MoveLeft/Right/Up/Down、四等分、六等分 等无默认快捷键，可通过托盘菜单或设置界面配置
3. 在 `App.OnLaunched` 中，创建 MainWindow 并执行 `m_window.Activate()` 后，立即执行（ConfigService 若已创建则传入）：
   ```csharp
   var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(m_window);
   var config = App.ConfigService?.Load();  // 若 TASK-036 已完成
   _ = new HotkeyManager(hwnd, WindowManager!, config);
   ```
4. 将 HotkeyManager 实例保存为字段（避免被 GC 回收），或确保其内部持有对 WindowManager 的引用

#### 验证

- 每个快捷键均能触发对应窗口操作

---

## 第八部分：设置界面

### TASK-036：创建 ConfigService

- [ ] **TASK-036** 创建 ConfigService（完整结构见附录 B.5）

#### 动作

1. 创建 `src/Rectangle.Windows/Services/ConfigService.cs`
2. 配置路径：`Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rectangle", "config.json")`
3. 在 Save 前执行 `Directory.CreateDirectory(Path.GetDirectoryName(path)!)` 确保目录存在
4. 实现 `Load()`、`Save()`，使用 `System.Text.Json.JsonSerializer.Serialize`/`Deserialize`
5. 配置类定义（与附录 B.5 一致）：
   ```csharp
   public class AppConfig
   {
       public int GapSize { get; set; } = 0;
       public bool LaunchOnLogin { get; set; } = false;
       public List<string> IgnoredApps { get; set; } = new();  // 忽略的进程名，如 "Cursor.exe"
       public Dictionary<string, ShortcutConfig> Shortcuts { get; set; } = new();
       public SnapAreaConfig SnapAreas { get; set; } = new();
   }
   public class ShortcutConfig
   {
       public bool Enabled { get; set; } = true;
       public int KeyCode { get; set; }
       public uint ModifierFlags { get; set; }
   }
   public class SnapAreaConfig
   {
       public bool DragToSnap { get; set; } = true;
       public bool RestoreSizeOnSnapEnd { get; set; } = true;
       public bool HapticFeedback { get; set; } = false;
       public bool SnapAnimation { get; set; } = false;
       public Dictionary<string, string> AreaActions { get; set; } = new();
   }
   ```

6. 在 App 中创建 ConfigService 单例（如 `App.ConfigService`），供 HotkeyManager、WindowManager、SettingsWindow 使用

#### 验证

- 调用 Save 后文件存在，Load 可读回

---

### TASK-037：创建 SettingsWindow（三选项卡结构）

- [ ] **TASK-037** 创建 SettingsWindow（布局参考附录 B；macOS 参考：`Rectangle/PrefsWindow/` 下各 ViewController）

#### 动作

1. 创建 `src/Rectangle.Windows/Views/SettingsWindow.xaml` 及 `SettingsWindow.xaml.cs`
2. 窗口标题：Rectangle 偏好设置
3. 使用 TabView 或 Pivot 实现三个选项卡：
   - **键盘快捷键**：图标 + 选项卡内容（TASK-037a 实现）
   - **吸附区域**：图标 + 选项卡内容（TASK-037b 实现）
   - **设置**：图标（齿轮）+ 选项卡内容
4. **设置**选项卡内容（最小实现）：StackPanel 包含 Slider（Minimum=0, Maximum=20, 绑定 GapSize）、ToggleSwitch（绑定 LaunchOnLogin）
5. 使用 WinUI 3 的 TabView/Pivot、Slider、ToggleSwitch 控件

#### 验证

- 从托盘菜单打开设置窗口，三个选项卡可见，点击可切换

---

### TASK-037a：键盘快捷键选项卡

两列布局，每项：图标（ms-appx:///Assets/WindowPositions/xxxTemplate.png，见附录 D）、名称、ToggleSwitch(Enabled)、快捷键输入框、清除按钮。分组：半屏、四角、三分之一、最大化与缩放、显示器。录入用 KeyDown 或键盘钩子捕获 KeyCode+ModifierFlags。**参考 macOS**：`Rectangle/PrefsWindow/KeyboardShortcutsViewController.swift`。

**「恢复默认」按钮**：在键盘快捷键选项卡底部或顶部添加「恢复默认」按钮。Click 时：1）用附录 A 的默认快捷键映射覆盖 `config.Shortcuts`（含 KeyCode、ModifierFlags、Enabled=true）；2）调用 `ConfigService.Save()`；3）调用 `HotkeyManager.ReloadFromConfig(config.Shortcuts)` 或等价逻辑重新注册；4）刷新界面显示。需在 ConfigService 或独立类中定义 `GetDefaultShortcuts()` 返回内置默认 Dictionary。

### TASK-037b：吸附区域选项卡

顶部开关：DragToSnap、RestoreSizeOnSnapEnd。下方 8 区网格，每区下拉选 WindowAction。底部中央可做 ⅓/⅔ 动态。绑定 SnapAreaConfig。**参考 macOS**：`Rectangle/PrefsWindow/SnapAreaViewController.swift`。

---

### TASK-038：设置与 ConfigService 绑定

- [ ] **TASK-038** 绑定配置

#### 动作

1. 加载时 ConfigService.Load 填充所有选项卡控件（设置、快捷键、吸附区域）
2. 关闭时 ConfigService.Save
3. 快捷键修改后需调用 HotkeyManager 重新注册（Unregister 旧键，Register 新键）

#### 验证

- 修改间隙、快捷键、吸附选项后关闭，重启应用后值保留

---

### TASK-038a：在计算器中应用 GapSize

- [ ] **TASK-038a** 布局计算时应用间隙（参考 macOS：`Rectangle/WindowCalculation/GapCalculation.swift`）

#### 动作

1. 扩展 `IRectCalculator.Calculate` 签名，增加 `int gap` 参数：`WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap)`
2. 或创建 `LayoutContext(WorkArea, WindowRect, int Gap)` 传入
3. 所有 Calculator 实现中应用 gap：相邻窗口之间留出 gap 像素。例如 LeftHalf：`width = (workArea.Width - gap) / 2`；RightHalf：`x = workArea.Left + (workArea.Width - gap) / 2 + gap`；四角、三分之一等类似
4. WindowManager.Execute 从 ConfigService 获取 GapSize，调用 `calculator.Calculate(..., config.GapSize)`
5. CalculatorFactory 或 WindowManager 需注入 ConfigService 以读取 GapSize

#### 验证

- 设置 GapSize=10 后，左半屏与右半屏之间可见 10px 间隙

---

### TASK-038b：实现开机启动 (LaunchOnLogin)

- [ ] **TASK-038b** 根据 LaunchOnLogin 配置注册/取消注册开机启动

#### 动作

1. 使用注册表 `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`，键名如 `Rectangle`
2. 值：应用可执行文件完整路径（`Environment.ProcessPath` 或 `Assembly.GetExecutingAssembly().Location`）
3. LaunchOnLogin=true 时写入，false 时删除该项
4. 在 ConfigService.Save 后、或设置窗口 LaunchOnLogin 切换时立即执行
5. NativeMethods.txt 需添加 `RegSetValueExW`、`RegDeleteValueW`、`RegOpenKeyExW` 等，或使用 .NET `Microsoft.Win32.Registry`

#### 验证

- 勾选开机启动后重启系统，Rectangle 自动启动

---

### TASK-039：托盘「设置」打开 SettingsWindow

- [ ] **TASK-039** 托盘打开设置

#### 动作

1. 在 MainWindow 的 `OpenSettings()` 方法中实现：`new SettingsWindow().Activate()`（或 `Show()`，确保窗口显示）

#### 验证

- 点击设置可打开窗口

---

## 第九部分：三分之一布局

### TASK-040：创建 FirstThird、CenterThird、LastThird 计算器

- [ ] **TASK-040** 创建三分之一计算器（参考 `Rectangle/WindowCalculation/FirstThirdCalculation.swift` 等）

#### 动作

1. 创建 `src/Rectangle.Windows/Core/Calculators/FirstThirdCalculator.cs`：宽度 = workArea.Width/3，X = workArea.Left
2. 创建 `CenterThirdCalculator.cs`：宽度 = workArea.Width/3，X = workArea.Left + workArea.Width/3
3. 创建 `LastThirdCalculator.cs`：宽度 = workArea.Width/3，X = workArea.Left + 2*workArea.Width/3
4. 在 CalculatorFactory 中注册这三个计算器

#### 验证

- 对 workArea (0,0,1920,1080)，FirstThirdCalculator 返回 Width=640, X=0

---

### TASK-041：创建 FirstTwoThirds、CenterTwoThirds、LastTwoThirds 计算器

- [ ] **TASK-041** 创建三分之二计算器（参考 `Rectangle/WindowCalculation/FirstTwoThirdsCalculation.swift` 等）

#### 动作

1. 创建 `FirstTwoThirdsCalculator.cs`：宽度 = 2*workArea.Width/3，X = workArea.Left
2. 创建 `CenterTwoThirdsCalculator.cs`：宽度 = 2*workArea.Width/3，X = workArea.Left + workArea.Width/6
3. 创建 `LastTwoThirdsCalculator.cs`：宽度 = 2*workArea.Width/3，X = workArea.Left + workArea.Width/3
4. 在 CalculatorFactory 中注册

#### 验证

- FirstTwoThirds 对 1920 宽度返回 Width=1280
- CenterTwoThirds 对 1920 宽度返回 Width=1280, X=320

---

### TASK-041a：CenterHalf、AlmostMaximize、MaximizeHeight、Larger、Smaller

CenterHalf：Width=w/2, X=Left+w/4。AlmostMaximize：Maximize 四周留 10px。MaximizeHeight：Width=current.Width, Height=workArea.Height。Larger/Smaller：按 10% 缩放，限制在 workArea 内。注册到 CalculatorFactory。**参考**：`CenterHalfCalculation`、`AlmostMaximizeCalculation`、`MaximizeHeightCalculation`、`ChangeSizeCalculation`。

### TASK-041b：MoveLeft/Right/Up/Down

同 LeftHalf/RightHalf/TopHalf/BottomHalf，可复用或委托。注册到 CalculatorFactory。

### TASK-041c：四等分 7 个计算器

1/4：Width=w/4，X 依次 Left, Left+w/4, Left+w/2, Left+3w/4。3/4：Width=3w/4，X 依次 Left, Left+w/8, Left+w/4。注册到 CalculatorFactory。**参考**：`Rectangle/WindowCalculation/FirstFourthCalculation.swift` 等。

### TASK-041d：六等分 6 个计算器

3 列×2 行，格子 w/3×h/2。坐标：左上(Left,Top)、中上(Left+w/3,Top)、右上(Left+2w/3,Top)、左下(Left,Top+h/2)、中下(Left+w/3,Top+h/2)、右下(Left+2w/3,Top+h/2)。注册到 CalculatorFactory。**参考**：`Rectangle/WindowCalculation/TopLeftSixthCalculation.swift` 等。

### TASK-042：三分之一快捷键

TASK-035 已包含，确认 HotkeyManager 与 CalculatorFactory 已注册 FirstThird、CenterThird、LastThird、FirstTwoThirds、CenterTwoThirds、LastTwoThirds。

---

## 第十部分：多显示器

### TASK-043：实现 EnumDisplayMonitors 与 GetMonitorWorkAreas

- [ ] **TASK-043** 枚举显示器工作区

#### 动作

1. NativeMethods.txt 在 TASK-006 已包含 EnumDisplayMonitors，确认存在
2. 在 Win32WindowService 中实现 `List<WorkArea> GetMonitorWorkAreas()`：使用 EnumDisplayMonitors 回调，对每个显示器调用 GetMonitorInfoW 取 rcWork，返回 List<WorkArea>
3. 需定义委托类型用于 EnumDisplayMonitors 的 callback 参数

#### 验证

- 单屏返回 1 个 WorkArea，双屏返回 2 个

---

### TASK-044：实现 NextDisplay、PreviousDisplay

- [ ] **TASK-044** 实现 NextDisplay、PreviousDisplay

#### 动作

1. 在 Win32WindowService 中实现 `GetNextMonitorWorkArea(nint hwnd)` 和 `GetPreviousMonitorWorkArea(nint hwnd)`：获取当前窗口所在显示器在列表中的索引，返回下一/上一显示器的 WorkArea
2. 在 WindowManager 中为 NextDisplay、PreviousDisplay 添加处理：获取下一/上一工作区，将窗口移动到该工作区居中
3. 在 CalculatorFactory 中注册 NextDisplay、PreviousDisplay（需新建计算器或直接在 WindowManager 中特殊处理）
4. 在 HotkeyManager 中注册 Ctrl+Alt+Cmd+Left→PreviousDisplay，Ctrl+Alt+Cmd+Right→NextDisplay（VK_LEFT=0x25, VK_RIGHT=0x27，修饰键加 MOD_WIN 或 MOD_CONTROL）

#### 验证

- 双屏时按快捷键，窗口移动到另一显示器

---

## 第十一部分：拖拽吸附

### TASK-045：添加鼠标钩子 API

- [ ] **TASK-045** 在 NativeMethods.txt 中添加 SetWindowsHookEx、UnhookWindowsHookEx、CallNextHookEx

#### 前置条件

- TASK-035 已完成

#### 动作

1. 在 `src/Rectangle.Windows/NativeMethods.txt` 末尾添加（每行一个）：
   ```
   SetWindowsHookExW
   UnhookWindowsHookEx
   CallNextHookEx
   ```
2. CsWin32 会自动生成 `WH_MOUSE_LL` 常量（值为 14），无需手写
3. 重新 build

#### 验证

- 可调用 `PInvoke.SetWindowsHookEx`

---

### TASK-046：创建 SnapDetectionService

- [ ] **TASK-046** 创建 SnapDetectionService

#### 动作

1. 创建 `src/Rectangle.Windows/Services/SnapDetectionService.cs`
2. 使用 `SetWindowsHookEx(WH_MOUSE_LL, callback, ...)` 安装低级鼠标钩子
3. 在回调中检测：鼠标按下（WM_LBUTTONDOWN）、移动（WM_MOUSEMOVE）、释放（WM_LBUTTONUP）
4. 按下时记录是否在某个窗口的标题栏区域（可用 GetWindowRect + 估算标题栏高度 30px）
5. 移动时若处于拖拽状态，根据光标与屏幕边缘距离（如 20px 内）判断吸附区域：左边缘→LeftHalf，右边缘→RightHalf，上边缘→Maximize，四角→对应四分之一
6. 释放时若在吸附区域，将对应 WindowAction 存入字段供外部读取
7. 吸附区域与 WindowAction 的映射从 `ConfigService.Load().SnapAreas.AreaActions` 读取；若未配置则使用默认（左边缘→LeftHalf，右边缘→RightHalf，上边缘→Maximize，四角→对应四分之一）

#### 验证

- 拖拽窗口时能检测到「靠近左边缘」状态

---

### TASK-047：创建预览窗口

- [ ] **TASK-047** 创建预览窗口

#### 动作

1. 创建 `src/Rectangle.Windows/Views/SnapPreviewWindow.xaml` 及 code-behind
2. 窗口属性：无边框、透明背景、置顶（TopMost）、不显示在任务栏
3. 内容：半透明矩形（如 Opacity 0.3），大小与目标 RECT 一致
4. 提供 `Show(Rect rect)` 和 `Hide()` 方法

#### 验证

- 调用 Show 时在指定位置显示半透明矩形

---

### TASK-048：松手时执行 WindowManager.Execute

- [ ] **TASK-048** 松手时执行窗口操作

#### 动作

1. 在 SnapDetectionService 的释放处理中，若检测到吸附区域，调用 `WindowManager.Execute(对应 Action)`
2. 在显示吸附预览时（TASK-047），松手前显示预览，松手后隐藏预览并执行 Execute
3. SnapDetectionService 需注入 WindowManager 和 SnapPreviewWindow

#### 验证

- 拖拽窗口到左边缘松手，窗口左半屏，预览正确显示后消失

---

## 第十二部分：自测与完善

### TASK-049：创建测试项目

- [ ] **TASK-049** 创建 xunit 测试项目

#### 动作

1. 在 `Rectangle.Windows/` 下执行：
   ```bash
   dotnet new xunit -n Rectangle.Windows.Tests -o src/Rectangle.Windows.Tests
   dotnet sln add src/Rectangle.Windows.Tests/Rectangle.Windows.Tests.csproj
   dotnet add src/Rectangle.Windows.Tests/Rectangle.Windows.Tests.csproj reference src/Rectangle.Windows/Rectangle.Windows.csproj
   ```

#### 验证

- `dotnet test src/Rectangle.Windows.Tests/Rectangle.Windows.Tests.csproj` 可运行（即使无测试也通过）

---

### TASK-050：为 Calculator 编写测试

- [ ] **TASK-050** 为 LeftHalfCalculator 编写单元测试

#### 动作

1. 在 `src/Rectangle.Windows.Tests/` 下创建 `LeftHalfCalculatorTests.cs`
2. 测试用例：`new LeftHalfCalculator().Calculate(new WorkArea(0,0,1920,1080), default, WindowAction.LeftHalf)` 返回的 `Width == 960` 且 `X == 0`
3. 使用 xunit 的 `Assert.Equal`

#### 验证

- `dotnet test` 通过

### TASK-051：执行完整自测

- [ ] **TASK-051** 执行集成自测清单

#### 动作

逐项测试并记录通过/失败：

1. 启动后托盘有图标
2. Ctrl+Alt+Left 使记事本左半屏
3. Ctrl+Alt+Right 使记事本右半屏
4. Ctrl+Alt+Up 使记事本上半屏
5. Ctrl+Alt+Down 使记事本下半屏
6. Ctrl+Alt+Enter 最大化
7. Ctrl+Alt+Backspace 恢复（或配置的 Restore 快捷键）
8. 四角快捷键正确
9. 三分之一快捷键正确
10. 设置中修改间隙后重启生效
11. 双屏时 NextDisplay 有效（无多屏则跳过此项）

### TASK-052：无前台窗口时的处理

- [ ] **TASK-052** 确认 GetForegroundWindow 返回 Zero 时播放提示音且不崩溃

### TASK-053：热键冲突处理

- [ ] **TASK-053** RegisterHotKey 返回 false 时记录日志或提示，不崩溃

### TASK-054：配置 MSIX 打包

- [ ] **TASK-054** 配置 MSIX 打包

#### 动作

```bash
dotnet publish src/Rectangle.Windows/Rectangle.Windows.csproj -c Release
```

#### 验证

- 输出目录中有 .msix 或 .exe

---

## 任务依赖关系（供 AI 参考）

> **建议**：TASK-036（ConfigService）可提前至 TASK-029 之后执行，以便 TASK-035 的 HotkeyManager 从配置加载快捷键；TASK-024a 依赖 TASK-036。

```
TASK-000
  └─ TASK-001 → TASK-002 → TASK-003 → TASK-004 → TASK-005 → TASK-006 → TASK-007 → TASK-007a
       └─ TASK-008 → TASK-009
            └─ TASK-010 → TASK-011 → TASK-012 → TASK-013 → TASK-014
                 └─ TASK-015 → TASK-016 → TASK-017 → TASK-018 → TASK-019 → TASK-020 → TASK-021 → TASK-022
                      └─ TASK-023 → TASK-024 → TASK-025 → TASK-026
                           └─ TASK-027 → TASK-028 → TASK-029
                                └─ TASK-024a（依赖 TASK-036，实现忽略列表检查）
                                └─ TASK-030 → TASK-031 → TASK-032 → TASK-033 → TASK-034 → TASK-035
                                          └─ TASK-036 → TASK-037 → TASK-037a → TASK-037b → TASK-038 → TASK-038a → TASK-038b → TASK-039
                                          └─ TASK-040 → TASK-041 → TASK-041a → TASK-041b → TASK-041c → TASK-041d → TASK-042
                                               └─ TASK-043 → TASK-044
                                                    └─ TASK-045～048
                                                         └─ TASK-049～054
```

---

## 附录 A：快捷键功能对照表（参考 macOS）

> 修饰键：macOS `^⌥` = Windows `Ctrl+Alt`，macOS `⌘` = Windows `Win`。Restore 建议用 Ctrl+Alt+Backspace 避免与系统冲突。

### A.1 有默认快捷键的 WindowAction

| WindowAction | 默认快捷键 | VK |
|--------------|------------|-----|
| LeftHalf, RightHalf, TopHalf, BottomHalf | Ctrl+Alt+←/→/↑/↓ | 0x25/27/26/28 |
| TopLeft, TopRight, BottomLeft, BottomRight | Ctrl+Alt+U/I/J/K | 0x55/49/4A/4B |
| FirstThird, CenterThird, LastThird | Ctrl+Alt+D/F/G | 0x44/46/47 |
| FirstTwoThirds, CenterTwoThirds, LastTwoThirds | Ctrl+Alt+E/R/T | 0x45/52/54 |
| Maximize, Center, Restore | Ctrl+Alt+Enter/C/Backspace | 0x0D/43/08 |
| MaximizeHeight, Larger, Smaller | Ctrl+Alt+Shift+↑ / = / - | 0x26/BB/BD |
| NextDisplay, PreviousDisplay | Ctrl+Alt+Win+→/← | 0x27/25 |

### A.2 无默认快捷键（可配置）

CenterHalf, AlmostMaximize, MoveLeft/Right/Up/Down, 四等分 7 项, 六等分 6 项。

### A.3 应用控制

忽略 [应用名]、偏好设置、查看日志、检查更新、退出。托盘菜单 Click 实现。

### A.4 常用 VK 与修饰键

VK：←0x25 ↑0x26 →0x27 ↓0x28 Enter0x0D =0xBB -0xBD D0x44 F0x46 G0x47 U0x55 I0x49 J0x4A K0x4B。修饰：MOD_ALT 0x0001, MOD_CONTROL 0x0002, MOD_SHIFT 0x0004, MOD_WIN 0x0008。

---

## 附录 B：偏好设置 UI 结构（参考 macOS）

**三选项卡**：键盘快捷键 | 吸附区域 | 设置（间隙 Slider 0–20、开机启动 ToggleSwitch）

**键盘快捷键**：两列布局，每项含图标、名称、ToggleSwitch、快捷键输入框、清除按钮。分组：半屏、四角、三分之一、最大化与缩放、显示器。

**吸附区域**：开关（拖移以吸附、结束吸附时恢复窗口大小）；网格 8 区（左上/上/右上、左/右、左下/下/右下），每区下拉选 WindowAction。默认：四角→TopLeft 等，上→Maximize，左/右→LeftHalf/RightHalf，下→⅓/⅔。

**配置类**：AppConfig { GapSize, LaunchOnLogin, IgnoredApps, Shortcuts, SnapAreas }；ShortcutConfig { Enabled, KeyCode, ModifierFlags }；SnapAreaConfig { DragToSnap, RestoreSizeOnSnapEnd, AreaActions }。

---

## 附录 C：补充与遗漏项汇总（审查后新增）

以下为文档审查时补充的任务与配置项，确保与 macOS 版功能对齐：

| 补充项 | 说明 | 对应任务/位置 |
|--------|------|---------------|
| 忽略应用列表 | AppConfig.IgnoredApps，WindowManager 执行前检查 | TASK-024a，TASK-028 第 3 点 |
| GapSize 在布局中的应用 | 计算器需应用间隙，相邻窗口间留 gap 像素 | TASK-038a |
| 开机启动 | LaunchOnLogin 写入注册表 Run 键 | TASK-038b |
| HotkeyManager 从 Config 加载 | 启动时从 ConfigService.Shortcuts 加载，支持 Enabled 开关 | TASK-035 依赖说明 |
| ConfigService 提前创建 | 建议 TASK-036 在 TASK-035 之前，以便 HotkeyManager 使用配置 | 任务依赖关系说明 |
| SnapDetectionService 读取配置 | 吸附区域映射从 SnapAreas.AreaActions 读取 | TASK-046 第 7 点 |
| 自测清单 Restore 快捷键 | 已改为 Ctrl+Alt+Backspace | TASK-051 |
| 全部快捷键恢复默认 | 键盘快捷键选项卡「恢复默认」按钮 | TASK-037a |

---

## 附录 D：图片资源（从 macOS 版获取）

**来源**：`Rectangle/Assets.xcassets/StatusTemplate.imageset/` → 托盘图标（转 .ico）；`Rectangle/Assets.xcassets/WindowPositions/*.imageset/*.png` → `Assets/WindowPositions/`。

**WindowAction→文件名**：见 `Rectangle/WindowAction.swift` 的 `image` 属性。命名规律：LeftHalf→leftHalfTemplate.png，TopLeft→topLeftTemplate.png，FirstThird→firstThirdTemplate.png 等。四等分：leftFourthTemplate、centerLeftFourthTemplate、centerRightFourthTemplate、rightFourthTemplate、firstThreeFourthsTemplate、centerThreeFourthsTemplate、lastThreeFourthsTemplate。六等分：topLeftSixthTemplate 等。

**复制脚本**：`for dir in Rectangle/Assets.xcassets/WindowPositions/*.imageset; do name=$(basename "$dir" .imageset); [ -f "$dir/$name.png" ] && cp "$dir/$name.png" src/Rectangle.Windows/Assets/WindowPositions/; done`

---

## 附录 E：macOS 参考文件索引

> 以下功能实现时，需参考 macOS 版 Rectangle 对应文件的内容或资源。路径相对于仓库根目录。

| 功能 | macOS 参考路径 | 说明 |
|------|----------------|------|
| WindowAction 枚举与动作定义 | `Rectangle/WindowAction.swift` | 完整 action 列表、`image` 属性（图标文件名映射） |
| 布局计算逻辑 | `Rectangle/WindowCalculation/*.swift` | 各计算器实现：`LeftRightHalfCalculation`、`FirstThirdCalculation`、`CenterHalfCalculation`、`FirstFourthCalculation`、`TopLeftSixthCalculation` 等 |
| 间隙 (Gap) 应用 | `Rectangle/WindowCalculation/GapCalculation.swift` | gap 如何应用到矩形计算 |
| 托盘/状态栏菜单结构 | `Rectangle/RectangleStatusItem.swift` | 菜单项组织、子菜单结构 |
| 键盘快捷键 UI | `Rectangle/PrefsWindow/KeyboardShortcutsViewController.swift` | 快捷键选项卡布局、录入逻辑 |
| 吸附区域 UI | `Rectangle/PrefsWindow/SnapAreaViewController.swift` | 吸附区域网格、区域与动作映射 |
| 设置选项卡 | `Rectangle/PrefsWindow/SettingsViewController.swift` | 间隙、开机启动等通用设置 |
| 窗口布局图标 | `Rectangle/Assets.xcassets/WindowPositions/*.imageset/` | 各 action 对应 PNG，见 `Contents.json` 中的 filename |
| 托盘图标 | `Rectangle/Assets.xcassets/StatusTemplate.imageset/` | RectangleStatusTemplate.png 等 |
| 吸附区域背景图 | `Rectangle/Assets.xcassets/wallpaperTiger.imageset/` | wallpaperTiger.png |
| 默认快捷键配置 | `Rectangle/ShortcutManager.swift`、`Rectangle/Defaults.swift` | 默认快捷键映射 |
| 忽略应用逻辑 | `Rectangle/Defaults.swift`（`disabledApps`、`fullIgnoreBundleIds`）、`Rectangle/TitleBarManager.swift` | 忽略列表的存储与检查 |

**使用方式**：实现对应功能前，用 `@Rectangle/xxx.swift` 或 `@Rectangle/Assets.xcassets/...` 引用并读取 macOS 源码，确保行为与 macOS 版一致。

---

## 自修正指引汇总

| 现象 | 可能原因 | 修正动作 |
|------|----------|----------|
| dotnet: command not found | 未安装 .NET | 安装 .NET 8 SDK |
| Template "winui" could not be found | 缺少 WinUI 模板 | `dotnet new install Microsoft.WindowsAppSDK.Templates` |
| error NETSDK1147 | 缺少 workload | `dotnet workload install windowsdesktop` |
| PInvoke/CsWin32 找不到类型 | NativeMethods.txt 未声明或拼写错误 | 检查 API 名，注意 W/A 后缀；`using Windows.Win32` |
| 热键无响应 | 窗口句柄为 0 或未接收 WM_HOTKEY | 确认窗口 Show 后再注册，检查消息钩子 |
| 窗口移动位置错 | 坐标或 DPI 问题 | 确认使用屏幕坐标，检查 DPI 感知 |
| 编译错误 CS0246 | 类型未找到 | 检查 using、命名空间、CsWin32 生成 |
| 运行时崩溃 | 空引用或 P/Invoke 参数错误 | 添加 null 检查，核对结构体布局 |
| GetMonitorInfo 失败 | cbSize 未正确设置 | 设置 `mi.cbSize = (uint)Marshal.SizeOf<MONITORINFO>()` |

---

## 执行指令（给 AI）

```
请严格按本文档任务顺序执行。每完成一项任务：
1. 在对应 [ ] 中标记为 [x]
2. 执行「验证」步骤
3. 若验证失败，执行「若失败」或「自修正指引」中的对应项
4. 验证通过后再进行下一项
5. 不要跳过任务，不要合并任务
6. 使用任务中给出的精确路径、类名、命令
```
