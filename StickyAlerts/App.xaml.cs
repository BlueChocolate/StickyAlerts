using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StickyAlerts.Models;
using StickyAlerts.Services;
using StickyAlerts.ViewModels;
using StickyAlerts.Views;
using System.Windows;

namespace StickyAlerts
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost Host { get; private set; }

        public App()
        {
            // 初始化主机
            var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    configuration.SetBasePath(context.HostingEnvironment.ContentRootPath);
                    configuration.AddJsonFile("Settings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddDebug();
                    logging.AddConsole();
                })
                .ConfigureServices((context, services) =>
                {
                    // 添加服务
                    services.AddSingleton<ISystemThemeService, SystemThemeService>();
                    services.AddSingleton<IAlertService, AlertService>();
                    services.AddSingleton<IShellService, ShellService>();
                    services.AddSingleton<ISettingsService<UserSettings>, UserSettingsService>();
                    // 添加视图和视图模型
                    services.AddSingleton<ShellWindow>();
                    services.AddSingleton<ShellViewModel>();
                    services.AddSingleton<AlertsView>();
                    services.AddSingleton<AlertsViewModel>();
                    services.AddSingleton<SettingsView>();
                    services.AddSingleton<SettingsViewModel>();
                    // 添加配置类
                    services.Configure<UserSettings>(context.Configuration.GetSection("UserSettings"));
                });
            Host = builder.Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 启动主机
            await Host.StartAsync();

            var userSettings = Host.Services.GetRequiredService<ISettingsService<UserSettings>>();
            var shellService = Host.Services.GetRequiredService<IShellService>();

            // 根据设置显示或隐藏主窗口
            shellService.ShowShell();
            if (!userSettings.Current.HideShell) shellService.HideShell();
            App.Current.MainWindow = Host.Services.GetRequiredService<ShellWindow>();

            // 应用所有设置（此时 ShellWindow 已经被创建，随之所有 AlertWindow 也被创建）
            userSettings.ApplyAll();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // 关闭主机
            Host.Dispose();
            await Host.StopAsync();
        }
    }
}
