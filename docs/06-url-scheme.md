# URL 方案与自动化

## 概述

Rectangle 支持通过 URL 方案执行窗口操作和任务，便于脚本和自动化集成。

## 执行窗口操作

### 基本格式

```
rectangle://execute-action?name=[action-name]
```

- 尽量不激活 Rectangle 窗口（使用 `open -g`）
- `name` 使用连字符格式，如 `left-half`、`top-right`

### 示例

```bash
# 左半屏
open -g "rectangle://execute-action?name=left-half"

# 最大化
open -g "rectangle://execute-action?name=maximize"

# 居中
open -g "rectangle://execute-action?name=center"
```

### 支持的操作名称

`left-half`, `right-half`, `center-half`, `top-half`, `bottom-half`,  
`top-left`, `top-right`, `bottom-left`, `bottom-right`,  
`first-third`, `center-third`, `last-third`, `first-two-thirds`, `last-two-thirds`,  
`maximize`, `almost-maximize`, `maximize-height`,  
`smaller`, `larger`, `center`, `center-prominently`, `restore`,  
`next-display`, `previous-display`,  
`move-left`, `move-right`, `move-up`, `move-down`,  
`first-fourth`, `second-fourth`, `third-fourth`, `last-fourth`,  
`first-three-fourths`, `last-three-fourths`,  
`top-left-sixth`, `top-center-sixth`, `top-right-sixth`,  
`bottom-left-sixth`, `bottom-center-sixth`, `bottom-right-sixth`,  
`specified`, `reverse-all`,  
`top-left-ninth`, `top-center-ninth`, `top-right-ninth`,  
`middle-left-ninth`, `middle-center-ninth`, `middle-right-ninth`,  
`bottom-left-ninth`, `bottom-center-ninth`, `bottom-right-ninth`,  
`top-left-third`, `top-right-third`, `bottom-left-third`, `bottom-right-third`,  
`top-left-eighth`, `top-center-left-eighth`, `top-center-right-eighth`, `top-right-eighth`,  
`bottom-left-eighth`, `bottom-center-left-eighth`, `bottom-center-right-eighth`, `bottom-right-eighth`,  
`tile-all`, `cascade-all`, `cascade-active-app`

## 执行任务

### 忽略应用

```
rectangle://execute-task?name=ignore-app
```

忽略当前前台应用。可选参数 `app-bundle-id` 指定应用：

```
rectangle://execute-task?name=ignore-app&app-bundle-id=com.apple.Safari
```

### 取消忽略应用

```
rectangle://execute-task?name=unignore-app
rectangle://execute-task?name=unignore-app&app-bundle-id=com.apple.Safari
```

## 名称转换规则

URL 中的 `name` 使用连字符格式。若传入驼峰格式（如 `leftHalf`），会自动转换为 `left-half`。

## 使用场景

- 自动化脚本（Shell、AppleScript、快捷指令等）
- 外部应用触发窗口布局
- 键盘/触控板宏
- 工作流工具集成
