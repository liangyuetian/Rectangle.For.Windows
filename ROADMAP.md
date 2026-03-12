# Rectangle For Windows - 功能开发路线图

> 基于与 macOS 版本的对比分析，Windows 版本当前完成度约 60%

## 📊 当前状态

### ✅ 已完成的核心功能
- [x] 基础窗口布局（半屏、四角、三分之一、四等分、六等分）
- [x] 最大化相关（最大化、接近最大化、最大化高度）
- [x] 窗口缩放（放大、缩小、居中）
- [x] 窗口恢复功能
- [x] 多显示器支持（下一个/上一个显示器）
- [x] 窗口移动到边缘
- [x] 快捷键系统
- [x] 基础配置系统（间隙、忽略应用）
- [x] 托盘菜单
- [x] 窗口历史记录系统（已重构）
- [x] LastActiveWindowService（窗口跟踪）
- [x] SetWindowPos 健壮性增强

### ❌ 缺失的重要功能
- [ ] 拖拽吸附（Drag to Snap）⭐⭐⭐
- [ ] 重复执行模式（循环尺寸）⭐⭐⭐
- [ ] 高级窗口布局（九等分、八等分等）⭐⭐
- [ ] 多窗口管理（平铺、层叠）⭐⭐
- [ ] 高级窗口操作（双倍/减半尺寸）⭐
- [ ] 高级配置选项（90+ 个）⭐⭐
- [ ] Todo 模式⭐
- [ ] 完整的日志系统⭐

---

## 🎯 Phase 1: 核心功能增强（优先级：🔴 高）

### 1.1 重复执行模式（Subsequent Execution Mode）

**目标**：连续按同一快捷键时循环不同尺寸或行为

**实现步骤**：

#### 任务 1.1.1: 创建重复执行模式枚举 ✅
- [x] 创建 `SubsequentExecutionMode.cs`
  ```csharp
  public enum SubsequentExecutionMode
  {
      None,           // 无操作（重复执行相同操作）
      CycleSize,      // 循环尺寸（如：1/2 → 2/3 → 1/3）
      AcrossMonitor,  // 跨显示器
      Resize          // 调整大小
  }
  ```
- **提交**: `6978b7f` - feat: 添加重复执行模式基础架构

#### 任务 1.1.2: 扩展 WindowHistory 记录操作次数 ✅
- [x] 在 `WindowHistory.cs` 中添加 `RectangleAction` 结构
  ```csharp
  public struct RectangleAction
  {
      public WindowAction Action;
      public int Count;  // 连续执行次数
      public DateTime LastExecutionTime;
  }
  ```
- [x] 添加 `TryGetLastAction(nint hwnd)` 方法
- [x] 添加 `RecordAction(nint hwnd, WindowAction action)` 方法
- [x] 实现自动计数和超时重置逻辑
- **提交**: `6978b7f` - feat: 添加重复执行模式基础架构

#### 任务 1.1.3: 实现循环尺寸计算器 ✅
- [x] 创建 `RepeatedExecutionsCalculator.cs`
- [x] 实现三分之一循环：FirstThird → CenterThird → LastThird
- [x] 实现三分之二循环：FirstTwoThirds → CenterTwoThirds → LastTwoThirds
- [x] 实现左右半屏循环：LeftHalf → RightHalf
- [x] 实现上下半屏循环：TopHalf → BottomHalf
- [x] 实现四等分循环：FirstFourth → ... → LastFourth
- [x] 实现四分之三循环：FirstThreeFourths → ... → LastThreeFourths
- **提交**: `c6328f8` - feat: 实现循环尺寸功能

#### 任务 1.1.4: 集成到 WindowManager ✅
- [x] 在 `WindowManager.Execute()` 中检测重复执行
- [x] 添加 `GetActualAction()` 方法处理循环逻辑
- [x] 根据配置选择执行模式
- [x] 实现超时机制（2秒内算重复执行）
- [x] 支持同一循环组内任意操作触发循环
- **提交**: `c6328f8` - feat: 实现循环尺寸功能

#### 任务 1.1.5: 添加配置选项 ⏸️
- [x] 在 `AppConfig` 中添加 `SubsequentExecutionMode` 配置
- [ ] ~~在设置界面添加选项~~ (暂时跳过 UI 任务)

**预计工作量**：4-6 小时

---

### 1.2 高级配置选项

**目标**：补充 macOS 版本的重要配置项

#### 任务 1.2.1: 扩展 AppConfig ✅
- [x] 添加 `AlmostMaximizeHeight` (float, 默认 0.9)
- [x] 添加 `AlmostMaximizeWidth` (float, 默认 0.9)
- [x] 添加 `MinimumWindowWidth` (float, 默认 0)
- [x] 添加 `MinimumWindowHeight` (float, 默认 0)
- [x] 添加 `SizeOffset` (float, 默认 30) - 放大缩小步长
- [x] 添加 `CenteredDirectionalMove` (bool, 默认 false)
- [x] 添加 `ResizeOnDirectionalMove` (bool, 默认 false)
- [x] 添加 `UseCursorScreenDetection` (bool, 默认 false)
- [x] 添加 `MoveCursor` (bool, 默认 false) - 移动窗口后移动光标
- [x] 添加 `MoveCursorAcrossDisplays` (bool, 默认 false)
- **提交**: `6a34a87` - feat: 添加高级配置选项

#### 任务 1.2.2: 实现接近最大化配置 ✅
- [x] 修改 `AlmostMaximizeCalculator` 使用配置的比例
- [x] 添加配置验证（0.5 - 1.0）
- [x] 重构 CalculatorFactory 支持依赖注入
- **提交**: `7f017e3` - feat: 实现接近最大化配置

#### 任务 1.2.3: 实现最小窗口尺寸限制 ✅
- [x] 创建 `WindowRectExtensions.cs` 扩展方法
- [x] 实现 `ApplyMinimumSize()` 应用最小尺寸限制
- [x] 实现 `ClampToWorkArea()` 确保窗口在屏幕内
- [x] 在 WindowManager 中集成应用
- **提交**: `fb789ec` - feat: 实现最小窗口尺寸限制

#### 任务 1.2.4: 实现光标位置检测 ✅
- [x] 创建 `ScreenDetectionService.cs`
- [x] 实现 `GetWorkAreaFromCursor()` 方法
- [x] 实现 `GetTargetWorkArea()` 智能选择
- [x] 在 `WindowManager` 中集成
- **提交**: `bf1fdfb` - feat: 实现光标位置检测

#### 任务 1.2.5: 实现光标移动功能 ✅
- [x] 添加 `SetCursorPos` P/Invoke
- [x] 实现 `MoveCursorToWindowCenter()` 方法
- [x] 在窗口操作后移动光标到窗口中心
- [x] 支持跨显示器光标移动配置
- **提交**: `dd66f06` - feat: 实现光标移动功能

#### 任务 1.2.6: 更新设置界面 ⏸️
- [ ] ~~在设置界面添加选项~~ (跳过 UI 任务)
- [ ] 添加配置开关

#### 任务 1.2.6: 更新设置界面
- [ ] 添加"高级选项"标签页
- [ ] 添加所有新配置项的 UI 控件
- [ ] 添加说明文本

**预计工作量**：6-8 小时

---

### 1.3 窗口类型检测和过滤

**目标**：更智能地处理不同类型的窗口

#### 任务 1.3.1: 扩展窗口检测 ✅
- [x] 创建 `WindowTypeService.cs`
- [x] 实现 `IsResizable()` 检测（WS_THICKFRAME）
- [x] 实现 `IsDialog()` 检测（WS_EX_DLGMODALFRAME）
- [x] 实现 `IsModalDialog()` 检测
- [x] 实现 `IsToolWindow()` 检测
- [x] 实现 `HasCaption()` 检测
- [x] 实现 `IsMinimized()` / `IsMaximized()` 检测
- **提交**: `309b5d3` - feat: 实现窗口类型检测服务

#### 任务 1.3.2: 固定尺寸窗口特殊处理 ✅
- [x] 集成到 WindowManager
- [x] 检测固定尺寸窗口（!IsResizable）
- [x] 实现 HandleFixedSizeWindow() 方法
- [x] 对固定尺寸窗口只移动不调整大小
- [x] 居中固定尺寸窗口
- **提交**: `63e2cf2` - feat: 集成窗口类型检测到 WindowManager

#### 任务 1.3.3: 对话框过滤 ✅
- [x] 在 Execute() 中排除模态对话框
- [x] 添加日志输出
- [ ] ~~添加配置选项~~ (暂不需要)
- **提交**: `63e2cf2` - feat: 集成窗口类型检测到 WindowManager

**预计工作量**：3-4 小时

---

## 🎯 Phase 2: 拖拽吸附功能（优先级：🔴 最高）

> ⚠️ 这是 Rectangle 的标志性功能，对用户体验影响最大

### 2.1 基础架构

#### 任务 2.1.1: 全局鼠标钩子 ✅
- [x] 创建 `MouseHookService.cs`
- [x] 实现 `SetWindowsHookEx` 鼠标钩子（WH_MOUSE_LL）
- [x] 监听 `WM_LBUTTONDOWN`, `WM_MOUSEMOVE`, `WM_LBUTTONUP`
- [x] 实现 `MouseDown`, `MouseUp`, `MouseMove` 事件
- [x] 实现线程安全的事件通知
- [x] 添加 `GetCursorPos()` 和 `GetWindowUnderCursor()` 辅助方法
- **提交**: `a5655d2` - feat: 实现全局鼠标钩子服务

#### 任务 2.1.2: 拖拽状态管理 ✅
- [x] 创建 `DragState.cs`
- [x] 实现拖拽状态属性（IsDragging, DraggedWindow 等）
- [x] 实现 `Reset()` 方法重置状态
- [x] 实现 `GetDragOffset()` 计算偏移量
- [x] 实现 `GetDragDurationMs()` 计算持续时间
- [x] 实现 `GetDragDistance()` 计算拖拽距离
- [x] 实现 `CalculateNewRect()` 计算新位置
- [x] 创建 `SnapArea` 类定义吸附区域
- [x] 创建 `MouseButton` 和 `SnapAreaType` 枚举
- **提交**: `1b3350d` - feat: 实现拖拽状态管理

#### 任务 2.1.3: 创建 SnappingManager ✅
- [x] 创建 `SnappingManager.cs`
- [x] 实现 `Enable()` / `Disable()` 方法
- [x] 实现 `OnMouseDown()` 开始拖拽
- [x] 实现 `OnMouseMove()` 更新拖拽
- [x] 实现 `OnMouseUp()` 结束拖拽
- [x] 实现 `CalculateSnapArea()` 计算吸附区域
- [x] 实现 `CheckScreenEdges()` 边缘检测
- [x] 实现 `CheckScreenCorners()` 角落检测
- [x] 实现 `ExecuteSnap()` 执行吸附
- [x] 添加 `SnapTriggered`, `DragStarted`, `DragEnded` 事件
- **提交**: `762101f` - feat: 创建 SnappingManager

**预计工作量**：6-8 小时

---

### 2.2 吸附区域检测

#### 任务 2.2.1: 定义吸附区域
- [ ] 创建 `SnapArea.cs`
  ```csharp
  public class SnapArea
  {
      public Rectangle Bounds { get; set; }
      public WindowAction Action { get; set; }
      public SnapAreaType Type { get; set; }  // Edge, Corner
  }
  ```

#### 任务 2.2.2: 实现吸附区域计算
- [ ] 创建 `SnapAreaCalculator.cs`
- [ ] 计算屏幕边缘吸附区域（上、下、左、右）
- [ ] 计算屏幕角落吸附区域（四个角）
- [ ] 支持多显示器
- [ ] 可配置吸附区域大小（默认 5 像素）

#### 任务 2.2.3: 光标位置检测
- [ ] 实现 `GetSnapAreaAtCursor()` 方法
- [ ] 实时检测光标是否在吸附区域内
- [ ] 返回对应的 WindowAction

**预计工作量**：4-6 小时

---

### 2.3 预览窗口（Footprint）

#### 任务 2.3.1: 创建预览窗口 ✅
- [x] 创建 `FootprintWindow.cs`
- [x] 半透明窗口（Opacity = 0.3）
- [x] 边框高亮（2px）
- [x] 无标题栏、无任务栏图标
- [x] 始终置顶（TopMost）
- [x] 淡入淡出动画
- [x] 自定义绘制
- **提交**: `7fa58b1` - feat: 创建预览窗口 FootprintWindow

#### 任务 2.3.2: 预览窗口样式配置 ⏸️
- [ ] 添加 `FootprintAlpha` 配置（默认 0.3）
- [ ] 添加 `FootprintBorderWidth` 配置（默认 2）
- [ ] 添加 `FootprintColor` 配置（默认蓝色）
- [ ] 支持自定义颜色
- **状态**: 非阻塞任务，暂时跳过

#### 任务 2.3.3: 预览窗口动画 ✅
- [x] 实现淡入淡出效果
- [x] 实现尺寸变化动画
- [ ] ~~添加 `FootprintFade` 配置开关~~
- [ ] ~~添加 `FootprintAnimationDuration` 配置~~
- **说明**: 基础动画已实现，配置项暂时跳过

**预计工作量**：6-8 小时

---

### 2.4 吸附执行

#### 任务 2.4.1: Unsnap 恢复功能 ✅
- [x] 检测窗口是否被程序调整过
- [x] 拖拽开始时保存原始尺寸
- [x] 拖拽结束时恢复原始尺寸（如果配置启用）
- [ ] ~~添加 `UnsnapRestore` 配置选项~~
- **提交**: `432247b` - feat: 集成预览窗口到 SnappingManager

#### 任务 2.4.2: 吸附执行逻辑 ✅
- [x] 鼠标释放时检测吸附区域
- [x] 执行对应的 WindowAction
- [x] 更新窗口历史记录
- [ ] ~~触发触觉反馈~~
- **说明**: 触觉反馈暂时跳过

#### 任务 2.4.3: 集成到 WindowManager ✅
- [x] 添加 `ExecuteSnapAction()` 方法
- [x] 标记为拖拽触发（不更新恢复点）
- [x] 处理多显示器情况
- **说明**: 已在 SnappingManager.ExecuteSnap 中实现

**预计工作量**：4-6 小时

---

### 2.5 配置和优化

#### 任务 2.5.1: 吸附配置
- [ ] `DragToSnap` (bool, 默认 true) - 启用拖拽吸附
- [ ] `SnapEdgeMarginTop/Bottom/Left/Right` (float, 默认 5)
- [ ] `CornerSnapAreaSize` (float, 默认 20)
- [ ] `SnapModifiers` (int) - 需要按住的修饰键
- [ ] `HapticFeedbackOnSnap` (bool) - 触觉反馈

#### 任务 2.5.2: 性能优化
- [ ] 限制预览窗口更新频率（60fps）
- [ ] 优化吸附区域检测性能
- [ ] 使用双缓冲避免闪烁

#### 任务 2.5.3: 设置界面
- [ ] 添加"拖拽吸附"标签页
- [ ] 可视化配置吸附区域大小
- [ ] 预览吸附效果

**预计工作量**：4-6 小时

---

**Phase 2 总预计工作量**：28-38 小时

---

## 🎯 Phase 3: 高级窗口布局（优先级：🟡 中）

### 3.1 九等分布局

#### 任务 3.1.1: 创建九等分计算器
- [ ] `TopLeftNinthCalculator.cs`
- [ ] `TopCenterNinthCalculator.cs`
- [ ] `TopRightNinthCalculator.cs`
- [ ] `MiddleLeftNinthCalculator.cs`
- [ ] `MiddleCenterNinthCalculator.cs`
- [ ] `MiddleRightNinthCalculator.cs`
- [ ] `BottomLeftNinthCalculator.cs`
- [ ] `BottomCenterNinthCalculator.cs`
- [ ] `BottomRightNinthCalculator.cs`

#### 任务 3.1.2: 添加到 WindowAction 枚举
- [ ] 添加 9 个新的枚举值
- [ ] 在 CalculatorFactory 中注册

#### 任务 3.1.3: 添加到菜单
- [ ] 创建"九等分"子菜单
- [ ] 添加默认快捷键（可选）

**预计工作量**：4-6 小时

---

### 3.2 八等分布局

#### 任务 3.2.1: 创建八等分计算器
- [ ] `TopLeftEighthCalculator.cs`
- [ ] `TopCenterLeftEighthCalculator.cs`
- [ ] `TopCenterRightEighthCalculator.cs`
- [ ] `TopRightEighthCalculator.cs`
- [ ] `BottomLeftEighthCalculator.cs`
- [ ] `BottomCenterLeftEighthCalculator.cs`
- [ ] `BottomCenterRightEighthCalculator.cs`
- [ ] `BottomRightEighthCalculator.cs`

#### 任务 3.2.2: 集成
- [ ] 添加到 WindowAction 枚举
- [ ] 注册到 CalculatorFactory
- [ ] 添加到菜单

**预计工作量**：4-6 小时

---

### 3.3 其他高级布局

#### 任务 3.3.1: 角落三分之一
- [ ] `TopLeftThirdCalculator.cs`
- [ ] `TopRightThirdCalculator.cs`
- [ ] `BottomLeftThirdCalculator.cs`
- [ ] `BottomRightThirdCalculator.cs`

#### 任务 3.3.2: 垂直三分之一
- [ ] `TopVerticalThirdCalculator.cs`
- [ ] `MiddleVerticalThirdCalculator.cs`
- [ ] `BottomVerticalThirdCalculator.cs`
- [ ] `TopVerticalTwoThirdsCalculator.cs`
- [ ] `BottomVerticalTwoThirdsCalculator.cs`

#### 任务 3.3.3: 居中显著
- [ ] `CenterProminentlyCalculator.cs`
- [ ] 实现比居中更大的窗口（如 80% 宽度和高度）

**预计工作量**：4-6 小时

---

**Phase 3 总预计工作量**：12-18 小时

---

## 🎯 Phase 4: 高级窗口操作（优先级：🟡 中）

### 4.1 双倍/减半尺寸

#### 任务 4.1.1: 创建尺寸调整计算器
- [ ] `DoubleHeightUpCalculator.cs`
- [ ] `DoubleHeightDownCalculator.cs`
- [ ] `DoubleWidthLeftCalculator.cs`
- [ ] `DoubleWidthRightCalculator.cs`
- [ ] `HalveHeightUpCalculator.cs`
- [ ] `HalveHeightDownCalculator.cs`
- [ ] `HalveWidthLeftCalculator.cs`
- [ ] `HalveWidthRightCalculator.cs`

#### 任务 4.1.2: 实现逻辑
- [ ] 保持窗口一边固定，另一边扩展/收缩
- [ ] 处理屏幕边界限制
- [ ] 应用最小/最大尺寸限制

**预计工作量**：4-6 小时

---

### 4.2 单独调整宽度/高度

#### 任务 4.2.1: 创建单维度调整计算器
- [ ] `LargerWidthCalculator.cs`
- [ ] `SmallerWidthCalculator.cs`
- [ ] `LargerHeightCalculator.cs`
- [ ] `SmallerHeightCalculator.cs`

#### 任务 4.2.2: 实现逻辑
- [ ] 使用 `SizeOffset` 配置
- [ ] 保持窗口居中或保持位置
- [ ] 处理边界情况

**预计工作量**：3-4 小时

---

### 4.3 指定尺寸

#### 任务 4.3.1: 创建指定尺寸功能
- [ ] `SpecifiedCalculator.cs`
- [ ] 添加 `SpecifiedWidth` 配置（默认 1680）
- [ ] 添加 `SpecifiedHeight` 配置（默认 1050）
- [ ] 居中窗口

#### 任务 4.3.2: 设置界面
- [ ] 添加指定尺寸输入框
- [ ] 添加快捷键配置

**预计工作量**：2-3 小时

---

**Phase 4 总预计工作量**：9-13 小时

---

## 🎯 Phase 5: 多窗口管理（优先级：🟢 低）

### 5.1 平铺所有窗口

#### 任务 5.1.1: 枚举所有窗口
- [ ] 创建 `WindowEnumerator.cs`
- [ ] 使用 `EnumWindows` API
- [ ] 过滤有效窗口
- [ ] 排除最小化窗口

#### 任务 5.1.2: 实现平铺算法
- [ ] 创建 `TileAllManager.cs`
- [ ] 计算网格布局（如 2x2, 3x2）
- [ ] 自动选择最佳布局
- [ ] 应用到所有窗口

#### 任务 5.1.3: 集成
- [ ] 添加到菜单
- [ ] 添加快捷键

**预计工作量**：6-8 小时

---

### 5.2 层叠所有窗口

#### 任务 5.2.1: 实现层叠算法
- [ ] 创建 `CascadeAllManager.cs`
- [ ] 计算层叠偏移（默认 30px）
- [ ] 添加 `CascadeAllDeltaSize` 配置
- [ ] 应用到所有窗口

#### 任务 5.2.2: 当前应用层叠/平铺
- [ ] 创建 `CascadeActiveAppManager.cs`
- [ ] 创建 `TileActiveAppManager.cs`
- [ ] 只处理当前应用的窗口

**预计工作量**：4-6 小时

---

### 5.3 反转所有窗口

#### 任务 5.3.1: 实现反转功能
- [ ] 创建 `ReverseAllManager.cs`
- [ ] 记录所有窗口位置
- [ ] 反转窗口位置映射
- [ ] 应用新位置

**预计工作量**：3-4 小时

---

**Phase 5 总预计工作量**：13-18 小时

---

## 🎯 Phase 6: Todo 模式（优先级：🟢 低）

### 6.1 Todo 侧边栏

#### 任务 6.1.1: 配置选项
- [ ] 添加 `TodoMode` (bool)
- [ ] 添加 `TodoApplication` (string) - 应用进程名
- [ ] 添加 `TodoSidebarWidth` (float, 默认 400)
- [ ] 添加 `TodoSidebarSide` (enum: Left/Right)

#### 任务 6.1.2: 实现 Todo 计算器
- [ ] `LeftTodoCalculator.cs`
- [ ] `RightTodoCalculator.cs`
- [ ] 为其他窗口预留 Todo 侧边栏空间

#### 任务 6.1.3: Todo 窗口管理
- [ ] 检测 Todo 应用窗口
- [ ] 自动调整 Todo 窗口到侧边栏
- [ ] 其他窗口避开 Todo 区域

**预计工作量**：6-8 小时

---

## 🎯 Phase 7: 完善和优化（优先级：🟢 低）

### 7.1 完整的日志系统

#### 任务 7.1.1: 结构化日志
- [ ] 创建 `Logger.cs`
- [ ] 日志级别（Debug, Info, Warning, Error）
- [ ] 日志格式化
- [ ] 写入文件

#### 任务 7.1.2: 日志查看器
- [ ] 创建日志查看窗口
- [ ] 实时日志显示
- [ ] 日志过滤和搜索
- [ ] 导出日志

**预计工作量**：4-6 小时

---

### 7.2 性能优化

#### 任务 7.2.1: 窗口操作优化
- [ ] 减少不必要的窗口查询
- [ ] 缓存窗口信息
- [ ] 批量窗口操作

#### 任务 7.2.2: 内存优化
- [ ] 及时清理窗口历史记录
- [ ] 限制历史记录数量
- [ ] 优化事件监听

**预计工作量**：4-6 小时

---

### 7.3 测试和文档

#### 任务 7.3.1: 单元测试
- [ ] Calculator 单元测试
- [ ] WindowHistory 单元测试
- [ ] ConfigService 单元测试

#### 任务 7.3.2: 集成测试
- [ ] 窗口操作集成测试
- [ ] 多显示器测试
- [ ] 拖拽吸附测试

#### 任务 7.3.3: 文档
- [ ] 用户手册
- [ ] 开发者文档
- [ ] API 文档

**预计工作量**：8-12 小时

---

**Phase 7 总预计工作量**：16-24 小时

---

## 📅 总体时间估算

| Phase | 功能 | 优先级 | 预计工作量 |
|-------|------|--------|-----------|
| Phase 1 | 核心功能增强 | 🔴 高 | 13-18 小时 |
| Phase 2 | 拖拽吸附 | 🔴 最高 | 28-38 小时 |
| Phase 3 | 高级布局 | 🟡 中 | 12-18 小时 |
| Phase 4 | 高级操作 | 🟡 中 | 9-13 小时 |
| Phase 5 | 多窗口管理 | 🟢 低 | 13-18 小时 |
| Phase 6 | Todo 模式 | 🟢 低 | 6-8 小时 |
| Phase 7 | 完善优化 | 🟢 低 | 16-24 小时 |
| **总计** | | | **97-137 小时** |

---

## 🚀 建议实施顺序

### 第一阶段（2-3 周）
1. ✅ Phase 1.1: 重复执行模式（4-6h）
2. ✅ Phase 1.2: 高级配置选项（6-8h）
3. ✅ Phase 1.3: 窗口类型检测（3-4h）

**目标**：提升基础功能的用户体验

### 第二阶段（4-5 周）
4. ✅ Phase 2: 拖拽吸附功能（28-38h）

**目标**：实现 Rectangle 的标志性功能

### 第三阶段（2-3 周）
5. ✅ Phase 3: 高级窗口布局（12-18h）
6. ✅ Phase 4: 高级窗口操作（9-13h）

**目标**：补充完整的布局选项

### 第四阶段（按需）
7. Phase 5: 多窗口管理（13-18h）
8. Phase 6: Todo 模式（6-8h）
9. Phase 7: 完善优化（16-24h）

**目标**：添加高级功能和优化

---

## 📝 开发注意事项

### 代码质量
- 遵循现有代码风格
- 每个功能添加单元测试
- 及时更新文档
- 代码审查后再合并

### 性能考虑
- 避免频繁的窗口查询
- 使用异步操作避免阻塞 UI
- 优化事件处理性能
- 内存使用监控

### 兼容性
- 测试不同 Windows 版本（Win10, Win11）
- 测试多显示器配置
- 测试不同 DPI 设置
- 测试不同应用程序

### 用户体验
- 提供清晰的错误提示
- 添加操作反馈（声音、视觉）
- 保持操作的一致性
- 提供详细的帮助文档

---

## 🎯 里程碑

### Milestone 1: 核心增强 (v0.2.0)
- [x] 窗口历史记录重构
- [x] LastActiveWindowService
- [x] SetWindowPos 健壮性
- [ ] 重复执行模式
- [ ] 高级配置选项
- [ ] 窗口类型检测

**预计完成时间**：2-3 周

### Milestone 2: 拖拽吸附 (v0.3.0)
- [ ] 全局鼠标钩子
- [ ] 吸附区域检测
- [ ] 预览窗口
- [ ] Unsnap 恢复
- [ ] 完整配置选项

**预计完成时间**：4-5 周

### Milestone 3: 功能完善 (v0.4.0)
- [ ] 九等分/八等分布局
- [ ] 高级窗口操作
- [ ] 垂直布局
- [ ] 完整的配置系统

**预计完成时间**：2-3 周

### Milestone 4: 高级功能 (v1.0.0)
- [ ] 多窗口管理
- [ ] Todo 模式
- [ ] 完整日志系统
- [ ] 性能优化
- [ ] 完整测试覆盖

**预计完成时间**：按需

---

## 📊 当前进度

- **总体完成度**: ~72%
- **Phase 1 进度**: 3/3 (100%) - ✅ Phase 1 全部完成！
- **Phase 2 进度**: 0.6/5 (12%) - Phase 2.1 基础架构完成！
  - ✅ 任务 2.1.1: 全局鼠标钩子
  - ✅ 任务 2.1.2: 拖拽状态管理
  - ✅ 任务 2.1.3: SnappingManager

**最后更新**: 2026-03-12
**最新提交**: `762101f` - feat: 创建 SnappingManager

---

## 🔗 相关资源

- [Rectangle macOS 源码](https://github.com/rxhanson/Rectangle)
- [Windows API 文档](https://docs.microsoft.com/en-us/windows/win32/api/)
- [项目 Issues](https://github.com/your-repo/issues)
- [开发文档](./DEVELOPMENT.md)

---

## 📮 反馈和建议

如有任何问题或建议，请：
1. 提交 GitHub Issue
2. 发送邮件至开发者
3. 参与社区讨论

**让我们一起打造最好的 Windows 窗口管理工具！** 🚀
