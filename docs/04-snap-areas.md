# 拖拽吸附区域（Snap Areas）

## 概述

将窗口拖至屏幕边缘时，会显示一个「足迹」预览，松手后窗口将自动调整到对应布局。

## 默认吸附区域映射

### 横屏模式（Landscape）

| 吸附区域 | 结果操作 |
|----------|----------------|
| 左边缘 | 左半屏 / 上半屏 / 下半屏（复合） |
| 右边缘 | 右半屏 / 上半屏 / 下半屏（复合） |
| 上边缘 | 最大化 |
| 下边缘 | 三分之一（左/中/右） |
| 左上角 | 左上四分之一 |
| 右上角 | 右上四分之一 |
| 左下角 | 左下四分之一 |
| 右下角 | 右下四分之一 |
| 左下角上方、中间、右方 | 左三分之一 / 中三分之一 / 右三分之一 |
| 右下角上方、中间、左方 | 左三分之一 / 中三分之一 / 右三分之一 |
| 左下角拖至底部中央 | 前三分之二 |
| 右下角拖至底部中央 | 后三分之二 |

### 竖屏模式（Portrait）

| 吸附区域 | 结果操作 |
|----------|----------------|
| 左/右边缘 | 竖屏三分之一（复合） |
| 下边缘 | 上半屏 / 下半屏（复合） |
| 其他 | 与横屏类似 |

## 六分之一吸附（可选）

启用 `sixthsSnapArea` 后，可将窗口拖至角落，再沿边缘向三分之一区域移动，以吸附到六分之一布局。

```bash
defaults write com.knollsoft.Rectangle sixthsSnapArea -bool true
```

## 吸附区域边距

各边缘边距可单独配置，默认 5 像素：

```bash
defaults write com.knollsoft.Rectangle snapEdgeMarginTop -int 10
defaults write com.knollsoft.Rectangle snapEdgeMarginBottom -int 10
defaults write com.knollsoft.Rectangle snapEdgeMarginLeft -int 10
defaults write com.knollsoft.Rectangle snapEdgeMarginRight -int 10
```

## 忽略特定吸附区域

使用位域 `ignoredSnapAreas` 禁用特定区域：

| 位 | 吸附区域 | 窗口操作 |
|----|----------|----------|
| 0 | 顶部 | 最大化 |
| 1 | 底部 | 三分之一 |
| 2 | 左侧 | 左半屏 |
| 3 | 右侧 | 右半屏 |
| 4 | 左上角 | 左上角 |
| 5 | 右上角 | 右上角 |
| 6 | 左下角 | 左下角 |
| 7 | 右下角 | 右下角 |
| 8 | 左上角下方 | 上半屏 |
| 9 | 右上角下方 | 上半屏 |
| 10 | 左下角上方 | 下半屏 |
| 11 | 右下角上方 | 下半屏 |

禁用顶部最大化：`ignoredSnapAreas = 1`  
禁用上半屏和下半屏：`ignoredSnapAreas = 3840`（位 8-11）

## 修饰键限制

可限制仅在按下特定修饰键时启用拖拽吸附：

```bash
# 仅 Cmd 键
defaults write com.knollsoft.Rectangle snapModifiers -int 1048576
```

## 足迹外观

- **透明度**：`footprintAlpha`（默认 0.3）
- **边框宽度**：`footprintBorderWidth`（默认 2）
- **颜色**：`footprintColor`
- **动画**：`footprintAnimationDurationMultiplier`（0 = 无动画）
- **淡入淡出**：`footprintFade`

## 禁用拖拽吸附

在偏好设置的 Snap Areas 标签页中取消勾选「Snap windows by dragging」。
