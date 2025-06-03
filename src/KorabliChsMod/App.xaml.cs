using HandyControl.Themes;
using Microsoft.Extensions.Logging;
using Prism.Ioc;
using Serilog;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.KorabliChsMod.Core.Services;
using Xanadu.KorabliChsMod.Services;
using Xanadu.KorabliChsMod.ViewModels;
using Xanadu.KorabliChsMod.Views;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod
{
    public partial class App
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册日志服务
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.File(KorabliConfigModel.LogFilePath,
                        encoding: Encoding.UTF8,
                        fileSizeLimitBytes: 50331648)
                    .CreateLogger());
            });
            // 注册 ILoggerFactory 到容器
            containerRegistry.RegisterInstance(loggerFactory);
            // 注册配置文件
            containerRegistry.RegisterSingleton<KorabliConfigService>();
            // 注册Lesta Game Center探查服务
            containerRegistry.RegisterSingleton<LgcIntegratorService>();
            // 注册缓存池服务
            containerRegistry.RegisterSingleton<FileCachePool>();
            // 注册更新助理服务
            containerRegistry.RegisterSingleton<UpdateService>();
            // 注册游戏探查服务
            containerRegistry.Register<GameDetectorService>();
            // 注册网络引擎服务
            containerRegistry.Register<NetworkEngine>();
            // 注册元数据获取服务
            containerRegistry.Register<MetadataService>();
            // 注册Mod安装服务
            containerRegistry.Register<ChsModService>();
            // 注册主窗口日志服务
            containerRegistry.RegisterSingleton<ILogger<MainWindowViewModel>, Logger<MainWindowViewModel>>();
            containerRegistry.RegisterForNavigation<MainWindowViewModel>();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        internal void UpdateTheme(ApplicationTheme theme)
        {
            if (ThemeManager.Current.ApplicationTheme != theme)
            {
                ThemeManager.Current.ApplicationTheme = theme;
            }
        }

        internal void UpdateAccent(Brush accent)
        {
            if (ThemeManager.Current.AccentColor != accent)
            {
                ThemeManager.Current.AccentColor = accent;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
