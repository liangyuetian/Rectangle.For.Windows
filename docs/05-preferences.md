# 偏好设置详解

## 可配置项列表

以下为 Rectangle 支持的所有偏好设置（基于 `Defaults.swift`）。

### 通用设置

| 键 | 类型 | 说明 |
|----|------|------|
| launchOnLogin | bool | 登录时启动 |
| hideMenubarIcon | bool | 隐藏菜单栏图标 |
| allowAnyShortcut | bool | 允许任意快捷键 |
| relaunchOpensMenu | bool | 重新启动时打开菜单 |
| obtainWindowOnClick | int | 点击获取窗口 |

### 窗口行为

| 键 | 类型 | 说明 |
|----|------|------|
| subsequentExecutionMode | int | 重复执行模式（0-4） |
| selectedCycleSizes | json | 循环尺寸选择 |
| cycleSizesIsChanged | bool | 循环尺寸已更改 |
| resizeOnDirectionalMove | bool | 方向移动时调整大小 |
| centeredDirectionalMove | int | 方向移动时居中 |
| traverseSingleScreen | int | 单屏时也遍历 |
| useCursorScreenDetection | bool | 使用光标检测屏幕 |
| attemptMatchOnNextPrevDisplay | int | 切换显示器时尝试匹配 |
| autoMaximize | int | 自动最大化 |

### 尺寸与间隙

| 键 | 类型 | 说明 |
|----|------|------|
| gapSize | float | 窗口间间隙（像素） |
| almostMaximizeHeight | float | 几乎最大化高度比例（0-1） |
| almostMaximizeWidth | float | 几乎最大化宽度比例（0-1） |
| minimumWindowWidth | float | 最小窗口宽度比例 |
| minimumWindowHeight | float | 最小窗口高度比例 |
| sizeOffset | float | 放大/缩小步长（像素） |
| widthStepSize | float | 宽度步长（默认 30） |
| horizontalSplitRatio | float | 水平分割比例（默认 50） |
| verticalSplitRatio | float | 垂直分割比例（默认 50） |
| specifiedHeight | float | 自定义居中高度（默认 1050） |
| specifiedWidth | float | 自定义居中宽度（默认 1680） |

### 屏幕边缘

| 键 | 类型 | 说明 |
|----|------|------|
| screenEdgeGapTop | float | 顶部边缘间隙 |
| screenEdgeGapBottom | float | 底部边缘间隙 |
| screenEdgeGapLeft | float | 左侧边缘间隙 |
| screenEdgeGapRight | float | 右侧边缘间隙 |
| screenEdgeGapsOnMainScreenOnly | bool | 仅主屏应用边缘间隙 |
| screenEdgeGapTopNotch | float | 刘海屏顶部间隙 |

### 吸附区域

| 键 | 类型 | 说明 |
|----|------|------|
| snapEdgeMarginTop | float | 吸附区域顶部边距（默认 5） |
| snapEdgeMarginBottom | float | 吸附区域底部边距 |
| snapEdgeMarginLeft | float | 吸附区域左侧边距 |
| snapEdgeMarginRight | float | 吸附区域右侧边距 |
| cornerSnapAreaSize | float | 角落吸附区域大小（默认 20） |
| shortEdgeSnapAreaSize | float | 短边吸附区域大小（默认 145） |
| snapModifiers | int | 吸附所需修饰键 |
| ignoredSnapAreas | int | 忽略的吸附区域位域 |
| sixthsSnapArea | int | 启用六分之一吸附 |
| landscapeSnapAreas | json | 横屏吸附配置 |
| portraitSnapAreas | json | 竖屏吸附配置 |

### 足迹外观

| 键 | 类型 | 说明 |
|----|------|------|
| footprintAlpha | float | 足迹透明度（默认 0.3） |
| footprintBorderWidth | float | 足迹边框宽度（默认 2） |
| footprintFade | int | 足迹淡入淡出 |
| footprintColor | json | 足迹颜色 |
| footprintAnimationDurationMultiplier | float | 足迹动画时长倍数 |

### 其他行为

| 键 | 类型 | 说明 |
|----|------|------|
| unsnapRestore | int | 取消吸附时恢复 |
| curtainChangeSize | int | 窗帘式调整大小 |
| moveCursorAcrossDisplays | int | 跨显示器移动光标 |
| moveCursor | int | 移动光标跟随窗口 |
| applyGapsToMaximize | int | 最大化时应用间隙 |
| applyGapsToMaximizeHeight | int | 最大化高度时应用间隙 |
| doubleClickTitleBar | int | 双击标题栏操作 |
| doubleClickTitleBarRestore | int | 双击标题栏恢复 |
| doubleClickTitleBarIgnoredApps | json | 双击标题栏忽略的应用 |
| missionControlDragging | int | 防止快速拖至菜单栏触发 Mission Control |
| missionControlDraggingAllowedOffscreenDistance | float | 允许离屏距离（默认 25） |
| missionControlDraggingDisallowedDuration | int | 禁止持续时间（毫秒，默认 250） |
| screensOrderedByX | int | 按 X 坐标排序显示器 |

### Stage Manager（macOS Ventura+）

| 键 | 类型 | 说明 |
|----|------|------|
| stageSize | float | Stage Manager 区域大小（默认 190） |
| dragFromStage | int | 从 Stage 拖拽 |
| alwaysAccountForStage | int | 始终考虑 Stage |

### Todo 模式

| 键 | 类型 | 说明 |
|----|------|------|
| todo | int | 启用 Todo 模式 |
| todoMode | bool | Todo 模式开关 |
| todoApplication | string | Todo 应用 Bundle ID |
| todoSidebarWidth | float | 侧边栏宽度（默认 400） |
| todoSidebarWidthUnit | int | 侧边栏宽度单位 |
| todoSidebarSide | int | 侧边栏位置（左/右） |

### 忽略应用

| 键 | 类型 | 说明 |
|----|------|------|
| disabledApps | string | 忽略的应用列表 |
| fullIgnoreBundleIds | json | 完全忽略的 Bundle ID |
| ignoreDragSnapToo | int | 忽略应用也禁用拖拽吸附 |

### 更新

| 键 | 类型 | 说明 |
|----|------|------|
| SUEnableAutomaticChecks | bool | 自动检查更新 |

### 内部/调试

| 键 | 类型 | 说明 |
|----|------|------|
| windowSnapping | int | 系统窗口吸附 |
| alternateDefaultShortcuts | bool | 使用推荐快捷键 |
| showAllActionsInMenu | int | 菜单显示所有操作 |
| showEighthsInMenu | int | 菜单显示八分之一 |
| cascadeAllDeltaSize | float | 层叠偏移（默认 30） |
| enhancedUI | int | 增强 UI |
| hapticFeedbackOnSnap | int | 吸附触觉反馈 |
| systemWideMouseDown | int | 全局鼠标按下 |
| systemWideMouseDownApps | json | 全局鼠标按下应用列表 |
