# Rectangle.Windows WinUI 3 (Windows App SDK) 重构计划

## 1. 项目概述

### 1.1 当前架构
| 项目 | 技术栈 | 状态 |
|------|--------|------|
| `Rectangle.Windows` | Windows Forms (.NET 9) | 主项目，功能完整 |
| `Rectangle.Windows.WinUI` | WinUI 3 / Windows App SDK | 初始框架，仅基础功能 |

### 1.2 WinUI 3 优势
- **现代 UI**: 原生支持 Windows 11 Fluent Design
- **性能**: 基于 DirectX 的硬件加速渲染
- **可访问性**: 更好的屏幕阅读器支持
- **未来证明**: 微软推荐的现代 Windows 桌面开发框架
- **亚克力/云母材质**: 内置支持现代透明材质效果

---

## 2. 项目结构对比

### 2.1 当前 Windows Forms 结构
```
Rectangle.Windows/
├── Views/
│   ├── SettingsForm.cs              # 设置窗口（主界面）
│   ├── Controls/
│   │   ├── SettingsTheme.cs         # 主题颜色和工具方法
│   │   ├── ModernCard.cs            # 卡片控件
│   │   ├── ModernButton.cs          # 按钮控件
│   │   ├── ModernCheckBox.cs        # iOS风格开关
│   │   ├── ModernSlider.cs          # 滑块控件
│   │   ├── NavButton.cs             # 导航按钮
│   │   └── SnapAreaPreview.cs       # 吸附区域预览
│   ├── SnapPreviewWindow.cs         # 吸附预览窗口
│   ├── FootprintWindow.cs           # 足迹窗口
│   ├── LogViewerForm.cs             # 日志查看器
│   ├── AcrylicContextMenu.cs        # 亚克力右键菜单
│   └── MenuIconGenerator.cs         # 菜单图标生成器
├── Services/
│   ├── ConfigService.cs             # 配置服务
│   ├── HotkeyManager.cs             # 热键管理
│   ├── WindowManager.cs             # 窗口管理
│   └── SnapDetectionService.cs      # 吸附检测
└── Core/
    ├── Calculators/                  # 窗口位置计算器
    ├── IRectCalculator.cs
    ├── WindowRect.cs
    └── WindowAction.cs
```

### 2.2 目标 WinUI 3 结构
```
Rectangle.Windows.WinUI/
├── App.xaml                         # 应用资源（已存在）
├── App.xaml.cs                      # 应用启动（已存在）
├── Views/
│   ├── MainWindow.xaml              # 主窗口（新增）
│   ├── MainWindow.xaml.cs
│   ├── SettingsPage.xaml            # 设置页面（迁移）
│   ├── SettingsPage.xaml.cs
│   ├── SnapAreasPage.xaml           # 吸附区域页面（新增）
│   ├── SnapAreasPage.xaml.cs
│   └── Controls/
│       ├── ModernCard.xaml          # WinUI 卡片控件
│       ├── ModernCard.xaml.cs
│       ├── ShortcutEditor.xaml      # 快捷键编辑器（新增）
│       ├── ShortcutEditor.xaml.cs
│       ├── SnapAreaPreview.xaml     # 吸附区域预览
│       └── SnapAreaPreview.xaml.cs
├── ViewModels/                      # 新增：MVVM 视图模型
│   ├── SettingsViewModel.cs
│   ├── ShortcutViewModel.cs
│   └── SnapAreasViewModel.cs
├── Services/                        # 大部分可直接复用
│   ├── ConfigService.cs             # 需适配 WinUI
│   ├── HotkeyManager.cs             # 需适配 WinUI
│   ├── WindowManager.cs             # 已存在
│   └── Win32WindowService.cs        # 已存在
└── Core/                            # 可直接复用
    ├── Calculators/
    ├── WindowAction.cs
    └── WindowHistory.cs
```

---

## 3. UI 控件映射表

### 3.1 自定义控件迁移映射

| WinForms 控件 | WinUI 3 等价物 | 迁移策略 |
|--------------|---------------|---------|
| `ModernCard` | `Border` + `Expander` | 使用 Border 实现圆角卡片，配合阴影效果 |
| `ModernButton` | `Button` (Fluent) | 使用系统 Button，应用 Fluent 样式 |
| `ModernCheckBox` | `ToggleSwitch` | WinUI 原生 iOS 风格开关 |
| `ModernSlider` | `Slider` | WinUI 原生滑块控件 |
| `NavButton` | `NavigationViewItem` | 使用 NavigationView 替代自定义导航 |
| `SettingsForm` | `Window` + `NavigationView` | 使用 WinUI NavigationView 实现左侧导航 |
| `FlowLayoutPanel` | `ItemsRepeater` / `StackPanel` | 使用 WinUI 布局面板 |
| `TableLayoutPanel` | `Grid` | WinUI Grid 替代 |

### 3.2 主题系统迁移

#### 当前 Windows Forms (SettingsTheme.cs)
```csharp
public static class SettingsTheme
{
    public static readonly Color BackgroundColor = Color.FromArgb(24, 24, 24);
    public static readonly Color CardColor = Color.FromArgb(38, 38, 38);
    public static readonly Color AccentColor = Color.FromArgb(0, 120, 212);
    // ...
}
```

#### 目标 WinUI 3 (ThemeResources.xaml)
```xml
<ResourceDictionary>
    <!-- 使用系统主题或自定义 -->
    <ResourceDictionary.ThemeDictionaries>
        <ResourceDictionary x:Key="Dark">
            <SolidColorBrush x:Key="CardBackgroundBrush" Color="#262626"/>
            <SolidColorBrush x:Key="CardHoverBrush" Color="#2D2D2D"/>
            <SolidColorBrush x:Key="AccentBrush" Color="#0078D4"/>
        </ResourceDictionary>
        <ResourceDictionary x:Key="Light">
            <!-- 亮色主题 -->
        </ResourceDictionary>
    </ResourceDictionary.ThemeDictionaries>
</ResourceDictionary>
```

---

## 4. 详细迁移步骤

### Phase 1: 基础架构 (1-2 周)

#### 1.1 更新项目文件
```xml
<!-- Rectangle.Windows.WinUI.csproj 已配置 -->
<!-- 确认以下关键配置 -->
<PropertyGroup>
    <TargetFramework>net9.0-windows10.0.19041.0</TargetFramework>
    <UseWinUI>true</UseWinUI>
    <WindowsPackageType>None</WindowsPackageType> <!-- 或 MSIX -->
</PropertyGroup>
```

#### 1.2 创建主窗口结构
**文件**: `Views/MainWindow.xaml`

```xml
<Window
    x:Class="Rectangle.Windows.WinUI.Views.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="Rectangle 设置">

    <NavigationView x:Name="NavView"
                    PaneDisplayMode="Left"
                    IsSettingsVisible="False"
                    SelectionChanged="NavView_SelectionChanged">
        <NavigationView.MenuItems>
            <NavigationViewItem Icon="Keyboard" Content="键盘快捷键" Tag="Shortcuts"/>
            <NavigationViewItem Icon="PreviewLink" Content="吸附区域" Tag="SnapAreas"/>
            <NavigationViewItem Icon="Setting" Content="设置" Tag="Settings"/>
        </NavigationView.MenuItems>

        <Frame x:Name="ContentFrame"/>
    </NavigationView>
</Window>
```

#### 1.3 迁移主题资源
创建 `Styles/ThemeResources.xaml`:
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">

    <!-- 自定义卡片样式 -->
    <Style x:Key="ModernCardStyle" TargetType="Border">
        <Setter Property="Background" Value="{ThemeResource CardBackgroundBrush}"/>
        <Setter Property="CornerRadius" Value="8"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="BorderBrush" Value="{ThemeResource CardBorderBrush}"/>
        <Setter Property="Shadow" Value="{StaticResource CardShadow}"/>
        <Setter Property="Padding" Value="16"/>
        <Setter Property="Margin" Value="0,0,0,20"/>
    </Style>

    <!-- 快捷键输入框样式 -->
    <Style x:Key="ShortcutInputStyle" TargetType="Button">
        <Setter Property="Width" Value="120"/>
        <Setter Property="Height" Value="32"/>
        <Setter Property="Background" Value="{ThemeResource InputBackgroundBrush}"/>
        <Setter Property="BorderBrush" Value="{ThemeResource BorderBrush}"/>
        <Setter Property="CornerRadius" Value="4"/>
    </Style>

</ResourceDictionary>
```

### Phase 2: 设置页面迁移 (2-3 周)

#### 2.1 键盘快捷键页面
**文件**: `Views/SettingsPage.xaml`

```xml
<Page x:Class="Rectangle.Windows.WinUI.Views.SettingsPage">
    <ScrollViewer>
        <Grid Padding="40,30" RowSpacing="20">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/> <!-- 标题 -->
                <RowDefinition Height="Auto"/> <!-- 两列布局 -->
                <RowDefinition Height="Auto"/> <!-- 恢复默认按钮 -->
            </Grid.RowDefinitions>

            <!-- 页面标题 -->
            <StackPanel Grid.Row="0" Margin="0,0,0,28">
                <TextBlock Text="键盘快捷键"
                          FontSize="24" FontWeight="Bold"
                          Foreground="{ThemeResource TextFillColorPrimaryBrush}"/>
                <TextBlock Text="配置窗口管理的快捷键"
                          FontSize="14"
                          Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
            </StackPanel>

            <!-- 两列卡片布局 -->
            <Grid Grid.Row="1" ColumnSpacing="20">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <!-- 左列 -->
                <StackPanel Grid.Column="0">
                    <!-- 半屏卡片 -->
                    <Border Style="{StaticResource ModernCardStyle}">
                        <StackPanel>
                            <TextBlock Text="半屏" FontWeight="Bold" Margin="0,0,0,12"/>
                            <ItemsRepeater ItemsSource="{x:Bind ViewModel.HalfScreenShortcuts}">
                                <ItemsRepeater.ItemTemplate>
                                    <DataTemplate x:DataType="local:ShortcutItem">
                                        <local:ShortcutEditor
                                            Action="{x:Bind Action}"
                                            DisplayName="{x:Bind DisplayName}"
                                            Icon="{x:Bind Icon}"
                                            Shortcut="{x:Bind Shortcut, Mode=TwoWay}"/>
                                    </DataTemplate>
                                </ItemsRepeater.ItemTemplate>
                            </ItemsRepeater>
                        </StackPanel>
                    </Border>

                    <!-- 四角卡片 -->
                    <Border Style="{StaticResource ModernCardStyle}">
                        <!-- 类似结构 -->
                    </Border>
                </StackPanel>

                <!-- 右列 -->
                <StackPanel Grid.Column="1">
                    <!-- 最大化卡片 -->
                    <Border Style="{StaticResource ModernCardStyle}">
                        <!-- 内容 -->
                    </Border>
                </StackPanel>
            </Grid>

            <!-- 恢复默认按钮 -->
            <Button Grid.Row="2" Content="恢复默认快捷键"
                   Click="RestoreDefaults_Click"
                   Style="{StaticResource AccentButtonStyle}"/>
        </Grid>
    </ScrollViewer>
</Page>
```

#### 2.2 快捷键编辑器控件
**文件**: `Views/Controls/ShortcutEditor.xaml`

```xml
<UserControl x:Class="Rectangle.Windows.WinUI.Views.Controls.ShortcutEditor">
    <Grid ColumnDefinitions="Auto,*,Auto,Auto"
          Height="40"
          Background="{ThemeResource CardBackgroundBrush}"
          Padding="8,0">

        <!-- 图标 -->
        <Image Grid.Column="0"
               Source="{x:Bind Icon}"
               Width="18" Height="18"
               Margin="0,0,8,0"/>

        <!-- 名称 -->
        <TextBlock Grid.Column="1"
                  Text="{x:Bind DisplayName}"
                  VerticalAlignment="Center"/>

        <!-- 快捷键显示/输入 -->
        <Button Grid.Column="2"
                Content="{x:Bind ShortcutText, Mode=OneWay}"
                Style="{StaticResource ShortcutInputStyle}"
                Click="ShortcutButton_Click"/>

        <!-- 清除按钮 -->
        <Button Grid.Column="3"
                Content="✕"
                Style="{StaticResource TextButtonStyle}"
                Click="ClearButton_Click"/>
    </Grid>
</UserControl>
```

#### 2.3 吸附区域页面
```xml
<Page x:Class="Rectangle.Windows.WinUI.Views.SnapAreasPage">
    <ScrollViewer>
        <StackPanel Padding="40,30" Spacing="20">
            <!-- 标题 -->
            <StackPanel Margin="0,0,0,8">
                <TextBlock Text="吸附区域" FontSize="24" FontWeight="Bold"/>
                <TextBlock Text="配置窗口拖拽吸附行为"
                          Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
            </StackPanel>

            <!-- 选项卡片 -->
            <Border Style="{StaticResource ModernCardStyle}">
                <StackPanel Spacing="16">
                    <ToggleSwitch Header="拖移以吸附"
                                 OffContent="关闭" OnContent="开启"
                                 IsOn="{x:Bind ViewModel.DragToSnap, Mode=TwoWay}"/>
                    <ToggleSwitch Header="结束吸附时恢复窗口大小"
                                 IsOn="{x:Bind ViewModel.RestoreSizeOnSnapEnd, Mode=TwoWay}"/>
                    <ToggleSwitch Header="显示吸附预览动画"
                                 IsOn="{x:Bind ViewModel.SnapAnimation, Mode=TwoWay}"/>
                </StackPanel>
            </Border>

            <!-- 吸附区域预览卡片 -->
            <Border Style="{StaticResource ModernCardStyle}">
                <StackPanel>
                    <TextBlock Text="吸附区域示意" FontWeight="Bold" Margin="0,0,0,16"/>
                    <local:SnapAreaPreview Width="560" Height="220"/>
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>
</Page>
```

### Phase 3: 对话框和弹窗 (1 周)

#### 3.1 快捷键捕获对话框
```csharp
public sealed partial class ShortcutCaptureDialog : ContentDialog
{
    public KeyCombination CapturedShortcut { get; private set; }

    public ShortcutCaptureDialog()
    {
        this.InitializeComponent();
        this.PrimaryButtonText = "确定";
        this.SecondaryButtonText = "取消";
        this.DefaultButton = ContentDialogButton.Secondary;
    }

    protected override void OnKeyDown(KeyRoutedEventArgs e)
    {
        // 捕获键盘输入
        if (e.Key == VirtualKey.Escape)
        {
            this.Hide();
            return;
        }

        var modifiers = GetCurrentModifiers();
        var key = e.Key;

        if (modifiers != VirtualKeyModifiers.None && key != VirtualKey.None)
        {
            CapturedShortcut = new KeyCombination(key, modifiers);
            this.Hide();
        }

        e.Handled = true;
    }

    private VirtualKeyModifiers GetCurrentModifiers()
    {
        var modifiers = VirtualKeyModifiers.None;
        var ctrl = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
        var alt = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu);
        var shift = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
        var win = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftWindows);

        if ((ctrl & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            modifiers |= VirtualKeyModifiers.Control;
        if ((alt & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            modifiers |= VirtualKeyModifiers.Menu;
        if ((shift & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            modifiers |= VirtualKeyModifiers.Shift;
        if ((win & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            modifiers |= VirtualKeyModifiers.Windows;

        return modifiers;
    }
}
```

### Phase 4: 服务层适配 (1 周)

#### 4.1 配置服务适配
```csharp
public class ConfigService
{
    // 大部分逻辑保持不变
    // 添加 WinUI 特定的存储路径处理

    private string GetConfigPath()
    {
        var localFolder = ApplicationData.Current.LocalFolder.Path;
        return Path.Combine(localFolder, "Rectangle", "config.json");
    }

    // Observable pattern for WinUI data binding
    public event TypedEventHandler<ConfigService, AppConfig>? ConfigChanged;
}
```

#### 4.2 热键管理器适配
```csharp
public class HotkeyManager
{
    private readonly Window _targetWindow;

    public HotkeyManager(Window targetWindow, WindowManager windowManager)
    {
        _targetWindow = targetWindow;
        // 使用 WinUI 的窗口句柄
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(targetWindow);
        // 注册 Win32 热键
        // ...
    }
}
```

### Phase 5: 托盘集成和后台 (1 周)

#### 5.1 托盘图标 (使用 H.NotifyIcon.WinUI)
```csharp
public partial class App : Application
{
    private TaskbarIcon? _trayIcon;

    private void CreateTrayIcon()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Rectangle",
            IconSource = new BitmapImage(new Uri("ms-appx:///Assets/AppIcon.ico"))
        };

        var menu = new MenuFlyout();
        // 添加菜单项...

        _trayIcon.ContextFlyout = menu;
    }
}
```

#### 5.2 窗口生命周期管理
```csharp
// 隐藏主窗口，仅在托盘显示
public void HideWindow()
{
    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
    PInvoke.ShowWindow(new HWND(hwnd), SHOW_WINDOW_CMD.SW_HIDE);
}

public void ShowSettingsWindow()
{
    var settingsWindow = new MainWindow();
    settingsWindow.Activate();
}
```

---

## 5. 关键代码示例

### 5.1 MVVM 绑定示例
```csharp
public class SettingsViewModel : ObservableObject
{
    private AppConfig _config;
    private readonly ConfigService _configService;

    public ObservableCollection<ShortcutItem> HalfScreenShortcuts { get; } = new();

    public bool LaunchOnLogin
    {
        get => _config.LaunchOnLogin;
        set
        {
            if (SetProperty(ref _config.LaunchOnLogin, value))
            {
                _configService.Save(_config);
            }
        }
    }

    public async Task RestoreDefaultsAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "确认",
            Content = "确定要恢复默认快捷键吗？",
            PrimaryButtonText = "确定",
            SecondaryButtonText = "取消",
            DefaultButton = ContentDialogButton.Secondary,
            XamlRoot = App.MainWindow?.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            _config.Shortcuts = ConfigService.GetDefaultShortcuts();
            _configService.Save(_config);
            LoadShortcuts();
        }
    }
}
```

### 5.2 自定义控件：快捷键编辑器
```csharp
public sealed partial class ShortcutEditor : UserControl
{
    public string Action { get; set; }
    public string DisplayName { get; set; }
    public ImageSource Icon { get; set; }

    public KeyCombination Shortcut
    {
        get => (KeyCombination)GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    public static readonly DependencyProperty ShortcutProperty =
        DependencyProperty.Register(nameof(Shortcut), typeof(KeyCombination),
            typeof(ShortcutEditor), new PropertyMetadata(null, OnShortcutChanged));

    private static void OnShortcutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (ShortcutEditor)d;
        editor.UpdateShortcutText();
    }

    private async void ShortcutButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ShortcutCaptureDialog();
        dialog.XamlRoot = this.XamlRoot;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.CapturedShortcut != null)
        {
            Shortcut = dialog.CapturedShortcut;
            ShortcutChanged?.Invoke(this, new ShortcutChangedEventArgs(Action, Shortcut));
        }
    }
}
```

---

## 6. 潜在问题和解决方案

### 6.1 P/Invoke 和 Win32 互操作
| 问题 | 解决方案 |
|------|---------|
| WinUI 3 窗口句柄获取 | 使用 `WinRT.Interop.WindowNative.GetWindowHandle` |
| CsWin32 兼容性 | 已配置，可直接使用 |
| 窗口消息处理 | 使用 `Microsoft.UI.Composition` 或 P/Invoke |

### 6.2 数据绑定差异
| WinForms | WinUI 3 |
|---------|---------|
| `INotifyPropertyChanged` | 相同，但使用 `ObservableObject` 基类 |
| `BindingSource` | 直接使用 `x:Bind` 和 `INotifyPropertyChanged` |
| 事件处理 | 使用命令模式 `ICommand` |

### 6.3 异步操作
```csharp
// WinUI 3 需要明确使用 IAsyncAction/IAsyncOperation
public IAsyncOperation<bool> SaveConfigAsync()
{
    return AsyncInfo.Run(async (token) =>
    {
        await FileIO.WriteTextAsync(file, json);
        return true;
    });
}
```

### 6.4 文件系统访问
```csharp
// WinUI 3 推荐使用 StorageFile API
var picker = new FileOpenPicker();
picker.FileTypeFilter.Add(".json");
var file = await picker.PickSingleFileAsync();
if (file != null)
{
    var json = await FileIO.ReadTextAsync(file);
}
```

---

## 7. 迁移检查清单

### 视图层
- [ ] 创建 MainWindow.xaml
- [ ] 迁移 SettingsForm 到 SettingsPage
- [ ] 创建现代卡片控件 (ModernCard)
- [ ] 创建快捷键编辑器 (ShortcutEditor)
- [ ] 迁移 SnapAreaPreview 控件
- [ ] 创建快捷键捕获对话框
- [ ] 设置主题资源字典

### 视图模型层
- [ ] 创建 SettingsViewModel
- [ ] 创建 SnapAreasViewModel
- [ ] 实现 ObservableObject 基类
- [ ] 设置数据绑定

### 服务层
- [ ] 适配 ConfigService 到 WinUI
- [ ] 适配 HotkeyManager
- [ ] 更新 WindowManager (如需要)
- [ ] 实现托盘图标集成

### 测试
- [ ] 验证所有快捷键功能
- [ ] 验证设置保存/加载
- [ ] 验证吸附区域预览
- [ ] 验证托盘菜单
- [ ] 验证深色/浅色主题

---

## 8. 参考资源

### 官方文档
- [WinUI 3 入门](https://docs.microsoft.com/windows/apps/winui/winui3/)
- [Windows App SDK](https://docs.microsoft.com/windows/apps/windows-app-sdk/)
- [Fluent Design System](https://www.microsoft.com/design/fluent/)

### 有用工具
- WinUI 3 Gallery (Microsoft Store)
- Windows Template Studio
- Community Toolkit (CommunityToolkit.WinUI)

### 迁移指南
- [从 WPF/WinForms 迁移](https://docs.microsoft.com/windows/apps/desktop/modernize/)
- [MVVM Toolkit](https://docs.microsoft.com/windows/communitytoolkit/mvvm/introduction)

---

## 9. 时间估算

| 阶段 | 预计时间 |
|------|---------|
| Phase 1: 基础架构 | 1-2 周 |
| Phase 2: 设置页面迁移 | 2-3 周 |
| Phase 3: 对话框和弹窗 | 1 周 |
| Phase 4: 服务层适配 | 1 周 |
| Phase 5: 托盘集成 | 1 周 |
| **总计** | **6-8 周** |

---

*文档生成日期: 2026-03-13*
*适用于: Rectangle.Windows 迁移到 WinUI 3 (Windows App SDK)*
