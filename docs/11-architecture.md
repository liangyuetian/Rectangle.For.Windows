# 技术架构

## 技术栈

- **语言**: Swift
- **框架**: Cocoa / AppKit
- **依赖**: 
  - [MASShortcut](https://github.com/rxhanson/MASShortcut) - 快捷键录制
  - [Sparkle](https://sparkle-project.org) - 自动更新

## 项目结构

```
Rectangle/
├── AppDelegate.swift          # 应用入口
├── WindowManager.swift       # 窗口管理核心
├── WindowAction.swift        # 窗口操作枚举
├── WindowHistory.swift       # 窗口历史（恢复）
├── WindowCalculation/        # 布局计算（50+ 种）
├── WindowMover/              # 窗口移动逻辑
├── Snapping/                 # 拖拽吸附
├── PrefsWindow/              # 偏好设置 UI
├── TodoMode/                 # Todo 模式
├── Defaults.swift            # 配置存储
├── Config.swift              # 配置导入导出
└── ...
```

## 核心模块

### WindowManager

- 执行窗口操作的入口
- 获取当前前台窗口（通过 Accessibility API）
- 检测屏幕（光标或窗口所在）
- 选择对应的 `WindowCalculation` 计算目标矩形
- 通过 `WindowMover` 责任链移动窗口

### WindowCalculation

- 协议：`calculate(_ params) -> WindowCalculationResult?`
- 每种布局对应一个实现类（如 `LeftRightHalfCalculation`、`UpperLeftCalculation`）
- 考虑屏幕方向、重复执行模式、间隙等

### WindowMover

责任链模式：

1. `StandardWindowMover` - 标准窗口
2. `CenteringFixedSizedWindowMover` - 固定尺寸窗口
3. `BestEffortWindowMover` - 兜底

### SnappingManager

- 监听拖拽事件
- 根据光标位置判断吸附区域
- 显示足迹预览
- 松手时执行对应 `WindowAction`

### ShortcutManager

- 基于 MASShortcut 注册全局快捷键
- 支持「忽略应用」时动态注册/注销

### AccessibilityElement

- 封装 macOS 辅助功能 API
- 获取窗口元素、位置、大小
- 设置窗口 frame

## 数据流

- 快捷键 / 菜单 / URL → `ExecutionParameters` → `WindowManager.execute()`
- 拖拽吸附 → `SnappingManager` → `WindowAction.postSnap()` → `WindowManager.execute()`
- 双击标题栏 → `TitleBarManager` → `WindowAction.postTitleBar()` → `WindowManager.execute()`

## 配置存储

- 使用 `NSUserDefaults`（UserDefaults）
- 存储路径：`~/Library/Preferences/com.knollsoft.Rectangle.plist`
- 支持 JSON 导入导出
