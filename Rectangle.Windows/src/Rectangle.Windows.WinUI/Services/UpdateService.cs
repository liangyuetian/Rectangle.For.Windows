using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 更新检查服务
    /// </summary>
    public class UpdateService
    {
        private const string GitHubApiUrl = "https://api.github.com/repos/rxhanson/Rectangle/releases/latest";
        private readonly ConfigService _configService;

        public event EventHandler<UpdateInfo>? UpdateAvailable;

        public UpdateService(ConfigService configService)
        {
            _configService = configService;
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        public async Task CheckForUpdatesAsync(bool silent = false)
        {
            try
            {
                var currentVersion = GetCurrentVersion();

                // 这里简化处理，实际应该调用 GitHub API
                // 模拟检查
                await Task.Delay(500);

                // 模拟发现新版本（实际实现需要 HTTP 请求）
                var latestVersion = "1.1.0"; // 从 API 获取

                if (IsNewerVersion(latestVersion, currentVersion))
                {
                    var updateInfo = new UpdateInfo
                    {
                        CurrentVersion = currentVersion,
                        LatestVersion = latestVersion,
                        ReleaseNotes = "\u65b0\u589e\u529f\u80fd\uff1a\n- \u4f18\u5316\u6027\u80fd\n- \u4fee\u590d bug\n- \u65b0\u589e\u5feb\u6377\u64cd\u4f5c\u9762\u677f",
                        DownloadUrl = "https://github.com/rxhanson/Rectangle/releases/latest"
                    };

                    UpdateAvailable?.Invoke(this, updateInfo);

                    if (!silent)
                    {
                        await ShowUpdateDialog(updateInfo);
                    }
                }
                else if (!silent)
                {
                    await ShowNoUpdateDialog();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateService", $"检查更新失败: {ex.Message}");

                if (!silent)
                {
                    await ShowErrorDialog();
                }
            }
        }

        private string GetCurrentVersion()
        {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        private bool IsNewerVersion(string latest, string current)
        {
            var latestParts = latest.Split('.');
            var currentParts = current.Split('.');

            for (int i = 0; i < Math.Min(latestParts.Length, currentParts.Length); i++)
            {
                if (int.Parse(latestParts[i]) > int.Parse(currentParts[i]))
                    return true;
                if (int.Parse(latestParts[i]) < int.Parse(currentParts[i]))
                    return false;
            }

            return latestParts.Length > currentParts.Length;
        }

        private async Task ShowUpdateDialog(UpdateInfo info)
        {
            var dialog = new ContentDialog
            {
                Title = "\u53d1现新版本",
                Content = $"当前版本: {info.CurrentVersion}\n\u6700新版本: {info.LatestVersion}\n\n\u66f4新内容:\n{info.ReleaseNotes}",
                PrimaryButtonText = "\u7acb即更新",
                SecondaryButtonText = "稍后",
                CloseButtonText = "忽略此版本",
                DefaultButton = ContentDialogButton.Primary
            };

            // 需要设置 XamlRoot
            if (App.MainWindow?.Content?.XamlRoot != null)
            {
                dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // 打开下载页面
                await Windows.System.Launcher.LaunchUriAsync(new Uri(info.DownloadUrl));
            }
        }

        private async Task ShowNoUpdateDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "检查完成",
                Content = "当前已是最新版本。",
                PrimaryButtonText = "确定",
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task ShowErrorDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "检查失败",
                Content = "无法检查更新，请稍后重试。",
                PrimaryButtonText = "确定",
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// 启动时检查（根据配置决定是否检查）
        /// </summary>
        public async Task CheckOnStartupAsync()
        {
            var config = _configService.Load();

            // 每天只检查一次
            if (config.CheckForUpdates && ShouldCheckToday(config.LastUpdateCheck))
            {
                config.LastUpdateCheck = DateTime.Now;
                _configService.Save(config);

                await CheckForUpdatesAsync(silent: true);
            }
        }

        private bool ShouldCheckToday(DateTime lastCheck)
        {
            return lastCheck.Date < DateTime.Now.Date;
        }
    }

    public class UpdateInfo
    {
        public string CurrentVersion { get; set; } = "";
        public string LatestVersion { get; set; } = "";
        public string ReleaseNotes { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
    }
}
