# Rectangle.Windows WinUI 3 重构说明

## 重构完成情况（已更新）

### Phase 1: 基础架构 ✅
- [x] 创建 MainWindow.xaml - 主窗口带 NavigationView 导航
- [x] 创建 Styles/ThemeResources.xaml - 深色/浅色主题资源字典
- [x] 更新 App.xaml - 整合主题资源
- [x] 更新 App.xaml.cs - 托盘集成、设置窗口管理、主题加载
- [x] 创建 ThemeService.cs - 主题管理服务

### Phase 2: 设置页面迁移 ✅
- [x] 创建 SettingsPage.xaml - 键盘快捷键设置页面（完整版）
- [x] 创建 SnapAreasPage.xaml - 吸附区域设置页面（增强版）
- [x] 创建 GeneralSettingsPage.xaml - 通用设置页面（含主题切换）

### Phase 3: 自定义控件 ✅
- [x] 创建 ShortcutEditor.xaml - 快捷键编辑器控件（支持图标）
- [x] 创建 ShortcutCaptureDialog.xaml - 快捷键捕获对话框
- [x] 创建 SnapAreaPreview.xaml - 吸附区域预览控件
- [x] 创建 AnimatedCard.xaml - 带动画效果的卡片控件

### Phase 4: MVVM 架构 ✅
- [x] 创建 ObservableObject.cs - MVVM 基类
- [x] 创建 SettingsViewModel.cs - 设置页面视图模型（完整）
- [x] 创建 SnapAreasViewModel.cs - 吸附区域视图模型（增强）

### Phase 5: 服务层适配 ✅
- [x] 创建 Services/ConfigService.cs - 配置服务 WinUI 版本（含主题配置）

### Phase 6: 附加功能 ✅
- [x] 主题切换（跟随系统/深色/浅色）
- [x] 动画效果（卡片悬停动画）
- [x] 完整快捷键分组（半屏、四角、三分屏、四等分、六等分、八等分、九宫格、移动、缩放、显示器、其他）
- [x] 吸附区域高级设置（边距、角落大小、恢复设置）
- [x] 日志设置（启用/级别）

## 项目结构

```
Rectangle.Windows.WinUI/
├── App.xaml                      # 应用资源
├── App.xaml.cs                   # 应用启动
├── Styles/
│   └── ThemeResources.xaml       # 主题资源字典（支持深色/浅色）
├── Views/
│   ├── MainWindow.xaml           # 主窗口
│   ├── MainPage.xaml             # 原有主页面
│   ├── SettingsPage.xaml         # 键盘快捷键设置
│   ├── SnapAreasPage.xaml        # 吸附区域设置
│   ├── GeneralSettingsPage.xaml  # 通用设置（主题、日志等）
│   └── Controls/
│       ├── ShortcutEditor.xaml   # 快捷键编辑器
│       ├── ShortcutCaptureDialog.xaml  # 快捷键捕获对话框
│       ├── SnapAreaPreview.xaml  # 吸附区域预览
│       └── AnimatedCard.xaml     # 动画卡片
├── ViewModels/
│   ├── ObservableObject.cs       # MVVM 基类
│   ├── SettingsViewModel.cs      # 设置视图模型
│   └── SnapAreasViewModel.cs     # 吸附区域视图模型
├── Services/
│   ├── ConfigService.cs          # 配置服务
│   ├── ThemeService.cs           # 主题服务
│   ├── WindowManager.cs          # 窗口管理
│   ├── HotkeyManager.cs          # 热键管理
│   └── Win32WindowService.cs     # Win32 服务
└── Core/                         # 核心计算逻辑
```

## 功能特性

### 1. 主题系统
- **深色主题**: 现代深色配色方案
- **浅色主题**: 明亮配色方案
- **跟随系统**: 自动根据系统设置切换
- **实时切换**: 无需重启应用

### 2. 快捷键设置
支持 50+ 种窗口操作：
- 半屏操作（左/右/上/下/中）
- 四角定位（左上/右上/左下/右下）
- 三分屏（1/3, 2/3, 左/中/右）
- 四等分（1/4, 3/4）
- 六等分（1/6）
- 八等分（1/8）
- 九宫格（1/9）
- 移动操作（上/下/左/右）
- 缩放操作（放大/缩小/加宽/收窄/加高/缩短）
- 显示器切换（下一个/上一个）
- 其他（层叠、最大化、恢复等）

### 3. 吸附区域设置
- 拖移以吸附开关
- 恢复窗口大小选项
- 预览动画
- 触觉反馈
- 窗口间隙调节（0-30px）
- 边缘边距设置（上/下/左/右）
- 角落区域大小
- Unsnap 恢复

### 4. 通用设置
- 开机启动
- 语言选择（预留）
- 日志设置（启用/级别）
- 重置所有设置
- 主题选择
- 关于信息

### 5. UI 特性
- **Fluent Design**: 现代 Windows 11 风格
- **卡片布局**: 清晰的分类组织
- **动画效果**: 悬停缩放和颜色过渡
- **图标支持**: Segoe Fluent Icons 字体图标
- **数据绑定**: 双向绑定，实时保存
- **深色/浅色**: 完整支持两种主题

## 数据模型

### 配置结构
```json
{
  "Theme": "Dark",
  "LaunchOnLogin": true,
  "GapSize": 0,
  "LogToFile": false,
  "LogLevel": 1,
  "SnapAreas": {
    "DragToSnap": true,
    "RestoreSizeOnSnapEnd": true,
    "SnapAnimation": false,
    "HapticFeedback": false
  },
  "Shortcuts": {
    "LeftHalf": { "KeyCode": 37, "ModifierFlags": 3, "Enabled": true }
    // ... 更多快捷键
  }
}
```

## 使用说明

### 构建项目
```bash
cd Rectangle.Windows/src/Rectangle.Windows.WinUI
dotnet restore
dotnet build
```

### 运行项目
```bash
dotnet run
```

### 发布项目
```bash
dotnet publish -c Release --self-contained false
```

## 快捷键编辑

1. 点击快捷键按钮显示"记录快捷键"
2. 按下快捷键组合（如 Ctrl+Alt+左箭头）
3. 自动保存并显示快捷键文本
4. 点击清除按钮（X）清除快捷键

## 主题切换

1. 打开"设置"页面
2. 在"外观"部分选择主题
3. 主题立即应用，无需重启

## 动画效果

- 卡片悬停：轻微放大 + 背景色变化
- 页面切换：平滑过渡（NavigationView 默认）
- 控件交互：标准 Fluent Design 动画

## 后续优化建议

1. **性能优化**
   - 虚拟化长列表
   - 延迟加载设置项
   - 图片资源缓存

2. **功能增强**
   - 快捷键冲突检测
   - 自定义窗口布局
   - 多显示器高级设置
   - 插件系统

3. **UI 优化**
   - 更多动画效果
   - 亚克力/云母材质
   - 自定义窗口样式

4. **测试**
   - 单元测试
   - UI 自动化测试
   - 性能测试

## 参考文档

- [WinUI 3 文档](https://docs.microsoft.com/windows/apps/winui/winui3/)
- [Windows App SDK](https://docs.microsoft.com/windows/apps/windows-app-sdk/)
- [Fluent Design System](https://www.microsoft.com/design/fluent/)
- [Segoe Fluent Icons](https://docs.microsoft.com/windows/apps/design/style/segoe-fluent-icons-font)
