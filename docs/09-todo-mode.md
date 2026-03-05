# Todo 模式

## 概述

Todo 模式为特定应用（如 Things、Todoist等）提供专用布局：主应用占据一侧，侧边栏/任务列表占据另一侧。

## 启用方式

### 偏好设置

1. 打开 Rectangle 偏好设置
2. 在 General 标签页勾选「Todo Mode」
3. 选择要使用的 Todo 应用（Bundle ID）

### 终端命令

```bash
defaults write com.knollsoft.Rectangle todo -int 1
```

## 相关操作

| 操作 | 说明 |
|------|------|
| `left-todo` | 主应用在左侧，Todo 侧边栏在右侧 |
| `right-todo` | 主应用在右侧，Todo 侧边栏在左侧 |

## 配置项

| 键 | 说明 |
|----|------|
| todo | 启用 Todo 模式 |
| todoMode | Todo 模式开关 |
| todoApplication | Todo 应用 Bundle ID |
| todoSidebarWidth | 侧边栏宽度（默认 400） |
| todoSidebarWidthUnit | 宽度单位（像素/百分比） |
| todoSidebarSide | 侧边栏位置（左/右） |

## 快捷键

- **切换 Todo 模式**：默认 Ctrl + Option + B（可自定义）
- **Reflow 布局**：默认 Ctrl + Option + N（可自定义）

## 更多信息

详见 [Rectangle Wiki - Todo Mode](https://github.com/rxhanson/Rectangle/wiki/Todo-Mode)
