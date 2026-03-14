# 迁移计划：从 WinUI.back → WinUI

每完成一步后执行 `dotnet build` 验证编译通过，再进行下一步。

构建命令（在新项目目录下执行）：
```
dotnet build Rectangle.Windows/src/Rectangle.Windows.WinUI/Rectangle.Windows.WinUI.csproj
```

---

## 当前新项目状态

新项目已有：
- `App.xaml` / `App.xaml.cs` — 基础 App 入口（简单版）
- `Views/MainPage.xaml` / `MainPage.xaml.cs` — 空白主页
- `Imports.cs` — 全局 using
- `Package.appxmanifest`、`app.manifest`

---

## ~~Step 1 — 迁移 NativeMethods.txt~~ ✅ 已完成

**操作：** 将 `WinUI.back/NativeMethods.txt` 复制到新项目根目录，覆盖（如有）。

**文件：**
- `NativeMethods.txt`

**验证：** `dotnet build` ✅

---

## ~~Step 2 — 迁移 Core 层（无依赖的基础类型）~~ ✅ 已完成

**操作：** 复制以下文件到新项目 `Core/` 目录（需新建目录）：

- `Core/WindowAction.cs` — WindowAction 枚举
- `Core/WindowRect.cs` — WorkArea / WindowRect 结构体
- `Core/WindowHistory.cs` — 窗口历史记录
- `Core/IRectCalculator.cs` — 计算器接口
- `Core/CalculatorFactory.cs` — 计算器工厂

> ⚠️ 注意：`WinUI.back/Core/Enums.cs` 中的 `WorkArea` struct 与 `WindowRect.cs` 中的 `WorkArea` record 重复定义，迁移时只保留 `WindowRect.cs` 中的版本，跳过 `Enums.cs` 中的 `WorkArea`，其余枚举单独处理。

**验证：** `dotnet build` ✅

---

## ~~Step 3 — 迁移 Core/Enums.cs（其余枚举）~~ ✅ 已完成

**操作：** 将 `Enums.cs` 中除 `WorkArea` struct 之外的枚举迁移到新项目：
- `SubsequentExecutionMode`
- `TodoSidebarSide`
- `DragState`
- `WindowActionType`
- `DisplayInfo` class

可以新建 `Core/Enums.cs`，去掉与 `WindowRect.cs` 冲突的 `WorkArea` 定义。

**验证：** `dotnet build` ✅

---

## ~~Step 4 — 迁移 Core/Calculators~~ ✅ 已完成（与 Step 2 合并）

**操作：** 复制整个 `Core/Calculators/` 目录到新项目：

- `Calculators/LeftHalfCalculator.cs`
- `Calculators/RightHalfCalculator.cs`
- `Calculators/AdvancedCalculators.cs`
- `Calculators/ExtendedCalculators.cs`
- `Calculators/MoreCalculators.cs`
- `Calculators/SpecialCalculators.cs`

**验证：** `dotnet build` ✅

---

## ~~Step 5 — 迁移 Services/Logger.cs~~ ✅ 已完成

**操作：** 复制 `Services/Logger.cs` 到新项目 `Services/` 目录（需新建目录）。

依赖：无外部依赖（仅 System.Text.Json，已在 SDK 中）

**验证：** `dotnet build` ✅

---

## ~~Step 6 — 迁移 Services/ConfigService.cs~~ ✅ 已完成

**操作：** 复制 `Services/ConfigService.cs`（包含 `AppConfig`、`ShortcutConfig`、`SnapAreaConfig` 等所有配置类）。

依赖：`Logger`（Step 5）

**验证：** `dotnet build` ✅

---

## ~~Step 7 — 迁移 Services/Win32WindowService.cs~~ ✅ 已完成

**操作：** 复制 `Services/Win32WindowService.cs`。

依赖：`Core/WindowRect.cs`（Step 2）、`NativeMethods.txt`（Step 1）

**验证：** `dotnet build` ✅

---

## ~~Step 8 — 迁移 Services/WindowManager.cs~~ ✅ 已完成

**操作：** 复制 `Services/WindowManager.cs`。

依赖：`Win32WindowService`（Step 7）、`CalculatorFactory`（Step 2）、`WindowHistory`（Step 2）

**验证：** `dotnet build` ✅

---

## ~~Step 9 — 迁移 Services/HotkeyManager.cs~~ ✅ 已完成

**操作：** 复制 `Services/HotkeyManager.cs`。

依赖：`WindowManager`（Step 8）、`NativeMethods.txt`（Step 1）

**验证：** `dotnet build` ✅

---

## ~~Step 10 — 迁移 Services/ThemeService.cs~~ ✅ 已完成

**操作：** 复制 `Services/ThemeService.cs`。

依赖：`ConfigService`（Step 6）

**验证：** `dotnet build` ✅

---

## ~~Step 11 — 迁移 ViewModels 层~~ ✅ 已完成

**操作：** 新建 `ViewModels/` 目录，复制：

- `ViewModels/ObservableObject.cs`
- `ViewModels/SettingsViewModel.cs`
- `ViewModels/SnapAreasViewModel.cs`

依赖：`ConfigService`（Step 6）、`WindowManager`（Step 8）

**验证：** `dotnet build` ✅

---

## ~~Step 12 — 迁移 Styles/ThemeResources.xaml~~ ✅ 已完成（资源内联到 App.xaml，独立文件有 XAML 编译器兼容问题）

**操作：** 新建 `Styles/` 目录，复制 `Styles/ThemeResources.xaml`。

需要在 `App.xaml` 的 `MergedDictionaries` 中引用该资源字典。

**验证：** `dotnet build` ✅

---

## Step 13 — 迁移 Views/Controls（自定义控件）

**操作：** 新建 `Views/Controls/` 目录，复制：

- `Views/Controls/AnimatedCard.xaml` / `.cs`
- `Views/Controls/ShortcutCaptureDialog.xaml` / `.cs`
- `Views/Controls/ShortcutEditor.xaml` / `.cs`
- `Views/Controls/SnapAreaPreview.xaml` / `.cs`

依赖：`ViewModels`（Step 11）、`ThemeResources`（Step 12）

**验证：** `dotnet build` ✅

---

## Step 14 — 迁移 Views/SnapPreviewWindow.cs

**操作：** 复制 `Views/SnapPreviewWindow.cs`（纯代码，无 XAML）。

**验证：** `dotnet build` ✅

---

## Step 15 — 迁移 Views/SettingsPage

**操作：** 复制：
- `Views/SettingsPage.xaml` / `.cs`

依赖：`SettingsViewModel`（Step 11）、Controls（Step 13）

**验证：** `dotnet build` ✅

---

## Step 16 — 迁移 Views/SnapAreasPage

**操作：** 复制：
- `Views/SnapAreasPage.xaml` / `.cs`

依赖：`SnapAreasViewModel`（Step 11）、Controls（Step 13）

**验证：** `dotnet build` ✅

---

## Step 17 — 迁移 Views/GeneralSettingsPage

**操作：** 复制：
- `Views/GeneralSettingsPage.xaml` / `.cs`

依赖：`ConfigService`（Step 6）、`SettingsViewModel`（Step 11）

**验证：** `dotnet build` ✅

---

## Step 18 — 迁移 Views/LogViewerWindow

**操作：** 复制：
- `Views/LogViewerWindow.xaml` / `.cs`

依赖：`Logger`（Step 5）

**验证：** `dotnet build` ✅

---

## Step 19 — 迁移 Views/MainWindow

**操作：** 复制：
- `Views/MainWindow.xaml` / `.cs`

这是设置窗口的主框架，包含 NavigationView，导航到 SettingsPage / SnapAreasPage / GeneralSettingsPage。

依赖：所有 Views（Step 15-18）

**验证：** `dotnet build` ✅

---

## Step 20 — 迁移剩余 Services（高级功能）

**操作：** 按依赖顺序逐个复制，每个单独 build 一次：

1. `Services/DpiAwarenessService.cs`
2. `Services/MouseHookService.cs`
3. `Services/SnapDetectionService.cs`
4. `Services/EdgeIndicatorService.cs`
5. `Services/WindowAnimationService.cs`
6. `Services/WindowEnumerator.cs`
7. `Services/WindowManagers.cs`（注意与 WindowManager.cs 区分）
8. `Services/LastActiveWindowService.cs`
9. `Services/LayoutManager.cs`
10. `Services/OperationHistoryManager.cs`
11. `Services/ScreenDetectionService.cs`
12. `Services/HotkeyConflictDetector.cs`
13. `Services/TodoManager.cs`
14. `Services/UpdateService.cs`
15. `Services/WindowStatisticsService.cs`
16. `Services/EnhancedServiceManager.cs`

**验证：** 每个文件复制后 `dotnet build` ✅

---

## Step 21 — 迁移 Services/TrayIconService.cs

**操作：** 复制 `Services/TrayIconService.cs`。

依赖：`WindowManager`（Step 8）、`WindowAction`（Step 2）

> ⚠️ 依赖 `RelayCommand`，该类目前在 `App.xaml.cs` 中定义，迁移时可先保留在 App.xaml.cs，或提取到单独文件。

**验证：** `dotnet build` ✅

---

## Step 22 — 更新 App.xaml.cs（完整版）

**操作：** 将新项目的 `App.xaml.cs` 替换为老项目的完整版本（包含 WindowManager、HotkeyManager、TrayIconService 初始化逻辑）。

同时更新 `Imports.cs` 的 global using，对齐老项目。

**验证：** `dotnet build` ✅

---

## Step 23 — 更新 Views/MainPage（可选）

**操作：** 如果老项目的 MainPage 有实际内容，替换新项目的 MainPage。

**验证：** `dotnet build` ✅

---

## 完成 🎉

全部迁移完成后，执行完整构建：
```
dotnet build Rectangle.Windows/src/Rectangle.Windows.WinUI/Rectangle.Windows.WinUI.csproj
```

---

## 常见问题排查

- **重复类型定义**：`WorkArea` 在 `Enums.cs` 和 `WindowRect.cs` 中都有定义，迁移时删除 `Enums.cs` 中的版本
- **命名空间不一致**：确保所有文件的 namespace 都是 `Rectangle.Windows.WinUI.*`
- **XAML 资源引用**：迁移 XAML 文件后如有资源找不到，检查 `App.xaml` 的 MergedDictionaries
- **CsWin32 生成**：`NativeMethods.txt` 变更后需要重新 build 才能生成 P/Invoke 代码
