# Rectangle 项目概述

## 简介

Rectangle 是一款基于 Spectacle 的 macOS 窗口管理应用，使用 Swift 编写。它允许用户通过键盘快捷键和拖拽操作快速调整窗口的位置和大小。

## 系统要求

- **macOS**: v10.15 及以上
- **旧版本支持**: macOS 10.13 和 10.14 的最后支持版本为 [v0.73](https://github.com/rxhanson/Rectangle/releases/tag/v0.73)

## 安装方式

### 方式一：官网下载

从 [rectangleapp.com](https://rectangleapp.com) 或 [Releases 页面](https://github.com/rxhanson/Rectangle/releases) 下载最新 DMG 安装包。

### 方式二：Homebrew

```bash
brew install --cask rectangle
```

## 核心特性

1. **键盘快捷键** - 通过全局快捷键快速调整窗口布局
2. **拖拽吸附** - 将窗口拖至屏幕边缘/角落时自动吸附
3. **多显示器** - 支持在多显示器间移动窗口
4. **忽略应用** - 可对特定应用禁用 Rectangle 快捷键
5. **URL 执行** - 通过 URL 方案执行窗口操作
6. **配置导入导出** - JSON 格式配置可在设备间迁移

## 与 Spectacle 的差异

- 使用 [MASShortcut](https://github.com/rxhanson/MASShortcut) 录制快捷键（Spectacle 使用自有录制器）
- 新增窗口操作：边缘移动不调整大小、仅最大化高度、几乎最大化
- 三分之一布局改为明确的第一/中心/最后三分之一，考虑屏幕方向
- 可通过选项让左右操作在显示器间循环切换
- 支持拖拽到边缘/角落时吸附（可禁用）

## 相关产品

- **Rectangle Pro** - 基于 Rectangle 构建的付费版本
- **Multitouch** - 使用 Rectangle 逻辑的多点触控应用
