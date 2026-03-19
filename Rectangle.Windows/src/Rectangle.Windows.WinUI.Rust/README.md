# Rectangle.Windows.WinUI.Rust

使用 **Rust + Win32 API** 重新实现的 Rectangle 窗口管理工具。

## 目标

- 用 Rust 替代 C#，减小运行时依赖，提升启动速度
- 完整实现 Rectangle.Windows.WinUI 的核心功能：热键、布局计算、多显示器、托盘、设置界面

## 技术栈

| 层级 | 技术 |
|------|------|
| 语言 | Rust (Edition 2024) |
| Win32 绑定 | `windows` crate |
| 配置 | JSON（`%APPDATA%\Rectangle\config.json`） |
| 日志 | `log` + `simplelog` |

## 系统要求

- Windows 10 1809 (17763) 或更高
- Rust 1.85+ (支持 Edition 2024)

## 已实现功能

### 核心层 (Core)
- [x] `WorkArea` / `WindowRect` 基础类型
- [x] `WindowAction` 枚举（所有布局动作）
- [x] `WindowHistory` 窗口历史记录（用于撤销/恢复）
- [x] `RectCalculator` trait 计算器接口
- [x] `CalculatorFactory` 计算器工厂

### 布局计算器 (Calculators)
- [x] 半屏: LeftHalf, RightHalf, TopHalf, BottomHalf, CenterHalf
- [x] 四角: TopLeft, TopRight, BottomLeft, BottomRight
- [x] 三分屏: FirstThird, CenterThird, LastThird, FirstTwoThirds, CenterTwoThirds, LastTwoThirds
- [x] 四等分: FirstFourth, SecondFourth, ThirdFourth, LastFourth, FirstThreeFourths, etc.
- [x] 六等分: TopLeftSixth, TopCenterSixth, TopRightSixth, BottomLeftSixth, BottomCenterSixth, BottomRightSixth
- [x] 最大化/居中: Maximize, AlmostMaximize, MaximizeHeight, Center, Larger, Smaller
- [x] 移动: MoveLeft, MoveRight, MoveUp, MoveDown
- [x] 尺寸调整: LargerWidth, SmallerWidth, LargerHeight, SmallerHeight
- [x] 双倍/减半: DoubleHeightUp, DoubleHeightDown, DoubleWidthLeft, DoubleWidthRight, etc.
- [x] 指定尺寸: Specified

### 服务层 (Services)
- [x] `ConfigService` 配置管理（加载/保存 JSON 配置）
- [x] `Logger` 日志服务
- [x] `Win32WindowService` Win32 API 封装
- [x] `WindowManager` 窗口管理（执行布局动作）
- [x] `HotkeyManager` 热键管理（注册/监听全局热键）
- [x] `TrayIconService` 托盘图标和菜单
- [x] `ScreenDetectionService` 多显示器支持

### 热键支持
- [x] 全局热键注册（使用 RegisterHotKey）
- [x] 支持 Ctrl/Alt/Shift/Win 修饰键
- [x] 默认快捷键配置

### 托盘功能
- [x] 系统托盘图标
- [x] 右键菜单（常用布局操作）
- [x] 设置和退出选项

## 构建与运行

```powershell
cd Rectangle.Windows.WinUI.Rust
cargo build --release
cargo run --release
```

## 配置

与 C# 版共用同一配置路径：`%APPDATA%\Rectangle\config.json`

配置项包括：
- 快捷键设置
- 间隙大小
- 分割比例
- 忽略的应用列表
- 动画设置

## 项目结构

```
Rectangle.Windows.WinUI.Rust/
├── Cargo.toml
├── README.md
├── IMPLEMENTATION_PLAN.md
└── src/
    ├── main.rs              # 入口：初始化、消息循环
    ├── lib.rs               # 库入口
    ├── core/                # 核心层
    │   ├── mod.rs
    │   ├── rect.rs          # WorkArea, WindowRect
    │   ├── action.rs        # WindowAction 枚举
    │   ├── history.rs       # 窗口历史记录
    │   ├── calculator.rs    # RectCalculator trait
    │   └── calculator_factory.rs
    ├── calculators/         # 布局计算器
    │   ├── mod.rs
    │   ├── half.rs          # 半屏计算器
    │   ├── corner.rs        # 四角计算器
    │   ├── third.rs         # 三分屏计算器
    │   ├── fourth.rs        # 四等分计算器
    │   ├── sixth.rs         # 六等分计算器
    │   ├── misc.rs          # 最大化/居中/尺寸调整
    │   └── move.rs          # 移动计算器
    └── services/            # 服务层
        ├── mod.rs
        ├── config.rs        # 配置管理
        ├── logger.rs        # 日志服务
        ├── win32.rs         # Win32 API 封装
        ├── window_manager.rs # 窗口管理
        ├── hotkey.rs        # 热键管理
        ├── tray.rs          # 托盘图标
        └── screen.rs        # 显示器检测
```

## 完成标准

- [x] 热键可正常注册并触发窗口管理
- [x] 所有布局动作（左半屏、右半屏、四角、三分屏等）正确执行
- [x] 托盘图标显示，右键菜单可用
- [ ] 设置窗口可打开，快捷键可配置并持久化（待实现 UI）
- [ ] 拖拽吸附（含边缘预览）可用（待实现）
- [x] 多显示器支持（基础实现）
- [x] 撤销/重做可用（基础实现）

## 注意事项

- 本项目使用 Rust 2024 Edition
- WinUI 3 绑定不成熟，本项目使用原生 Win32 API 实现
- 配置与 C# 版兼容，便于迁移

## 许可证

与主项目相同
