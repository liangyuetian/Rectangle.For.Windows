# 故障排除

## 快速修复

1. **锁定并解锁 Mac** – 系统更新后常见问题
2. **确保 macOS 为最新版本**
3. **重启 Mac** – 系统更新后通常有效

## 诊断步骤

1. **启用调试日志**

   - 按住 Option 键打开 Rectangle 菜单
   - 选择「View Logging...」（替代「About」）
   - 执行 Rectangle 操作时查看日志
   - 若计算出的 rect 与结果 rect 相同，问题可能来自其他应用

2. **检查冲突**

   - 确认没有其他窗口管理应用在运行
   - 检查目标应用是否有冲突的快捷键
   - 尝试通过菜单执行操作或更换快捷键，判断是否为快捷键问题

3. **高级排查**

   - 创建新用户测试
   - 保存日志以便提交问题

## 重置辅助功能权限

```bash
tccutil reset All com.knollsoft.Rectangle
```

或手动操作：

1. 关闭 Rectangle
2. 系统设置 → 隐私与安全性 → 辅助功能
3. 先禁用 Rectangle，再点击减号移除
4. 重启 Mac
5. 重新启动 Rectangle 并启用权限

## 常见问题

### 无法移至其他桌面/空间

Apple 未提供公开 API，Rectangle 无法实现此功能。Rectangle Pro 有此功能，暂无计划加入免费版。

### iTerm2 窗口调整略有偏差

iTerm2 默认按字符宽度缩放。可执行：

```bash
defaults write com.googlecode.iterm2 DisableWindowSizeSnap -integer 1
```

### 通知中心冻结

若出现此问题，请取消勾选「Snap windows by dragging」。参见 [issue #317](https://github.com/rxhanson/Rectangle/issues/317)。

### 配置无法持久化（Homebrew 安装）

```bash
brew uninstall --zap rectangle
brew install rectangle
```

## 卸载

1. 退出 Rectangle
2. 将应用移至废纸篓
3. 可选：删除配置

```bash
defaults delete com.knollsoft.Rectangle
```

Homebrew 安装：

```bash
brew uninstall --zap rectangle
```
