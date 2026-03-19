# 实施计划：Rust + WinUI 3 重写 Rectangle

**状态：核心功能已完成，WinUI 3 框架已建立**

参考 `Rectangle.Windows/src/Rectangle.Windows.WinUI` 的功能与架构，按阶段实现。

---

## 阶段 0：技术选型与项目初始化 ✅

### 0.1 项目初始化

- [x] 配置 `Cargo.toml` 使用 Rust 2024 Edition
- [x] 添加 `windows` crate 及 WinUI/WinRT 相关 features
- [x] 添加依赖：`serde`, `chrono`, `dirs`, `log`, `simplelog`, `anyhow`, `windows-core`

---

## 阶段 1：核心层（Core）✅

### 1.1 基础类型

| 文件 | 职责 | 状态 |
|------|------|------|
| `src/core/rect.rs` | `WorkArea`、`WindowRect` 结构体 | ✅ |
| `src/core/action.rs` | `WindowAction` 枚举 | ✅ |
| `src/core/history.rs` | 窗口历史记录 | ✅ |

### 1.2 计算器接口与工厂

| 文件 | 职责 | 状态 |
|------|------|------|
| `src/core/calculator.rs` | `RectCalculator` trait | ✅ |
| `src/core/calculator_factory.rs` | 计算器工厂 | ✅ |

### 1.3 布局计算器实现

| 分类 | 实现文件 | 状态 |
|------|----------|------|
| 半屏 | `src/calculators/half.rs` | ✅ |
| 四角 | `src/calculators/corner.rs` | ✅ |
| 三分屏 | `src/calculators/third.rs` | ✅ |
| 四等分 | `src/calculators/fourth.rs` | ✅ |
| 六等分 | `src/calculators/sixth.rs` | ✅ |
| 最大化/居中/尺寸调整 | `src/calculators/misc.rs` | ✅ |
| 移动 | `src/calculators/move.rs` | ✅ |

---

## 阶段 2：服务层（Services）✅

### 2.1 基础服务

| 服务 | 职责 | 状态 |
|------|------|------|
| `config.rs` | 配置加载/保存 | ✅ |
| `logger.rs` | 日志输出 | ✅ |
| `win32.rs` | Win32 API 封装 | ✅ |

### 2.2 窗口管理

| 服务 | 职责 | 状态 |
|------|------|------|
| `window_manager.rs` | 窗口管理 | ✅ |
| `operation_history` | 撤销/重做 | ✅ (内置于 window_manager) |

### 2.3 输入与交互

| 服务 | 职责 | 状态 |
|------|------|------|
| `hotkey.rs` | 热键管理 | ✅ |
| `tray.rs` | 基础托盘图标 | ✅ |

### 2.4 辅助服务

| 服务 | 职责 | 状态 |
|------|------|------|
| `screen.rs` | 多显示器支持 | ✅ |

---

## 阶段 3：WinUI 3 UI 层 ✅ (框架完成)

### 3.1 应用入口

| 组件 | 职责 | 状态 |
|------|------|------|
| `WinUIApp` | 初始化 ConfigService、WindowManager、HotkeyManager、TrayIcon | ✅ |
| `main.rs` | 应用入口，启动 WinUI 3 | ✅ |

### 3.2 主窗口与页面

| 组件 | 职责 | 状态 |
|------|------|------|
| `MainWindow` | 主窗口（NavigationView 导航） | ✅ (框架) |
| `ShortcutsPage` | 快捷键设置页面 | ✅ (框架) |
| `SnapAreasPage` | 吸附区域设置页面 | ✅ (框架) |
| `GeneralSettingsPage` | 通用设置页面 | ✅ (框架) |

### 3.3 XAML 文件

| 文件 | 职责 | 状态 |
|------|------|------|
| `MainWindow.xaml` | 主窗口布局（NavigationView） | ✅ |
| `ShortcutsPage.xaml` | 快捷键页面布局 | ✅ |
| `SnapAreasPage.xaml` | 吸附区域页面布局 | ✅ |
| `GeneralSettingsPage.xaml` | 通用设置页面布局 | ✅ |
| `ShortcutEditor.xaml` | 快捷键编辑器控件 | ✅ |

### 3.4 控件

| 控件 | 职责 | 状态 |
|------|------|------|
| `ShortcutEditor` | 单条快捷键编辑 | ✅ (框架) |
| `TrayIcon` | WinUI 3 风格托盘图标 | ✅ |

---

## WinUI 3 架构

```
┌─────────────────────────────────────────┐
│           WinUI 3 XAML UI               │
│  (MainWindow.xaml, ShortcutsPage.xaml)  │
├─────────────────────────────────────────┤
│         WinRT Interop Layer             │
│    (windows-core, windows-rs)           │
├─────────────────────────────────────────┤
│           Rust Core Logic               │
│  (WindowManager, HotkeyManager, etc.)   │
├─────────────────────────────────────────┤
│           Win32 API Layer               │
│    (window management, hotkeys)         │
└─────────────────────────────────────────┘
```

---

## 目录结构（目标）

```
Rectangle.Windows.WinUI.Rust/
├── Cargo.toml
├── README.md
├── IMPLEMENTATION_PLAN.md
├── src/
│   ├── main.rs
│   ├── lib.rs
│   ├── core/
│   │   ├── mod.rs
│   │   ├── rect.rs
│   │   ├── action.rs
│   │   ├── history.rs
│   │   ├── calculator.rs
│   │   └── calculator_factory.rs
│   ├── calculators/
│   │   ├── mod.rs
│   │   ├── half.rs
│   │   ├── corner.rs
│   │   ├── third.rs
│   │   ├── fourth.rs
│   │   ├── sixth.rs
│   │   ├── misc.rs
│   │   └── move.rs
│   ├── services/
│   │   ├── mod.rs
│   │   ├── config.rs
│   │   ├── logger.rs
│   │   ├── win32.rs
│   │   ├── window_manager.rs
│   │   ├── hotkey.rs
│   │   ├── tray.rs
│   │   └── screen.rs
│   └── ui/                  # WinUI 3 UI 层
│       ├── mod.rs
│       ├── app.rs           # WinUIApp
│       ├── main_window.rs
│       ├── shortcuts_page.rs
│       ├── snap_areas_page.rs
│       ├── general_settings_page.rs
│       ├── shortcut_editor.rs
│       ├── tray_icon.rs
│       └── xaml/            # XAML 文件
│           ├── MainWindow.xaml
│           ├── ShortcutsPage.xaml
│           ├── SnapAreasPage.xaml
│           ├── GeneralSettingsPage.xaml
│           └── ShortcutEditor.xaml
└── resources/
    └── icon.ico
```

---

## 完成标准

- [x] 热键可正常注册并触发窗口管理
- [x] 所有布局动作（左半屏、右半屏、四角、三分屏等）正确执行
- [x] 托盘图标显示，右键菜单可用
- [x] WinUI 3 应用框架
- [x] XAML 界面定义文件
- [ ] 完整的 WinRT/XAML 互操作绑定（需进一步开发）
- [ ] 拖拽吸附（含边缘预览）可用
- [x] 多显示器支持
- [x] 撤销/重做基础实现

---

## WinUI 3 说明

本项目使用 WinUI 3 作为 UI 框架：

1. **XAML 文件**: 定义 Fluent Design 风格的界面布局
2. **WinRT 互操作**: 通过 `windows-core` crate 连接 Rust 和 WinUI 3
3. **Windows App SDK**: 提供 WinUI 3 运行时支持

### XAML 文件说明

- `MainWindow.xaml`: 主窗口，包含 NavigationView 左侧导航
- `ShortcutsPage.xaml`: 快捷键设置，分类显示所有布局动作
- `SnapAreasPage.xaml`: 吸附区域配置，滑块控制边距
- `GeneralSettingsPage.xaml`: 通用设置，主题、动画、启动选项
- `ShortcutEditor.xaml`: 可复用的快捷键编辑控件

---

## 构建说明

```powershell
# 检查编译
cargo check

# 构建发布版本
cargo build --release

# 运行
cargo run --release
```

---

## 与 C# 版差异

1. **UI 层**: 使用 WinUI 3 XAML 定义界面
2. **语言**: Rust 替代 C#
3. **配置格式**: 与 C# 版 JSON 配置兼容
4. **性能**: 更小的运行时依赖

---

## 未来扩展

1. 完善 WinRT/XAML 互操作绑定
2. 实现拖拽吸附功能
3. 添加更多高级功能
