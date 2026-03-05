# 配置导入与导出

## 概述

Rectangle 支持以 JSON 格式导入和导出配置，便于在不同设备间迁移或备份。

## 配置存储位置

- **Preferences**: `~/Library/Preferences/com.knollsoft.Rectangle.plist`
- **v0.41+ 快捷键**: 使用新格式存储，无法在旧版本中加载

## 导出配置

1. 打开 Rectangle 偏好设置
2. 在 General 标签页底部找到「Import」/「Export」按钮
3. 点击「Export」导出 JSON 文件

配置包含：
- 应用 Bundle ID
- 版本号
- 所有快捷键
- 可导出的偏好设置

## 导入配置

### 方式一：偏好设置界面

1. 打开 Rectangle 偏好设置
2. 点击「Import」按钮
3. 选择 JSON 配置文件

### 方式二：启动时自动加载

将配置文件命名为 `RectangleConfig.json` 并放在以下目录：

```
~/Library/Application Support/Rectangle/RectangleConfig.json
```

启动 Rectangle 时会自动加载该配置，加载后文件会被重命名为带时间戳的备份（如 `RectangleConfig2025-02-27_14-30-00-1234.json`），避免重复加载。

## 配置结构

```json
{
  "bundleId": "com.knollsoft.Rectangle",
  "version": "1.0.0",
  "shortcuts": {
    "leftHalf": { "keyCode": 123, "modifierFlags": 786432 },
    "rightHalf": { "keyCode": 124, "modifierFlags": 786432 }
  },
  "defaults": {
    "launchOnLogin": { "bool": true },
    "gapSize": { "float": 10 }
  }
}
```

## 可导出的偏好设置

参见 [05-preferences.md](05-preferences.md) 中 `Defaults.array` 列出的项。

## Homebrew 用户注意

若通过 Homebrew 安装且配置选项在重启后无法保持，请使用 `--zap` 卸载后重装：

```bash
brew uninstall --zap rectangle
brew install rectangle
```
