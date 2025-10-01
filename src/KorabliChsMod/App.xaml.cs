using HandyControl.Themes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prism.Ioc;
using Serilog;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.KorabliChsMod.Core.Services;
using Xanadu.KorabliChsMod.Services;
using Xanadu.KorabliChsMod.ViewModels;
using Xanadu.KorabliChsMod.Views;
using Xanadu.Skidbladnir.IO.File.Cache;
using Xanadu.Skidbladnir.Net.DevOps;
using Xanadu.Skidbladnir.Net.DevOps.Service;

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
            // 注册单例配置文件
            containerRegistry.RegisterSingleton<KorabliConfigService>();
            // 注册内存缓存服务
            containerRegistry.RegisterSingleton<IMemoryCache>(() => new MemoryCache(Options.Create(new MemoryCacheOptions())));
            // 注册HTTP客户端服务
            containerRegistry.RegisterScoped<HttpClient>(provider =>
            {
                var korabliConfigService = provider.Resolve<KorabliConfigService>();
                return RestApiClient.DefaultHttpClient(handler =>
                {
                    try
                    {
                        if (korabliConfigService.CurrentConfig.Proxy.Enabled &&
                            !string.IsNullOrEmpty(korabliConfigService.CurrentConfig.Proxy.Address))
                        {
                            handler.Proxy = new WebProxy(korabliConfigService.CurrentConfig.Proxy.Address,
                                true, null,
                                new NetworkCredential(korabliConfigService.CurrentConfig.Proxy.Username,
                                    korabliConfigService.CurrentConfig.Proxy.Password));
                        }
                    }
                    catch (Exception)
                    {
                        lock (korabliConfigService.CurrentConfig.Proxy)
                        {
                            korabliConfigService.CurrentConfig.Proxy.Enabled = false;
                            korabliConfigService.SaveAsync().ConfigureAwait(false);
                        }
                    }
                },
                GitHubRestApiClient.DefaultHttpClientAction);
            });
            
            // 注册Lesta Game Center探查服务
            containerRegistry.RegisterSingleton<LgcIntegratorService>();
            // 注册缓存池服务
            containerRegistry.RegisterSingleton<FileCachePool>();
            // 注册更新助理服务
            containerRegistry.RegisterSingleton<UpdateService>();
            // 注册游戏探查服务
            containerRegistry.Register<GameDetectorService>();
            // 注册GitHub API服务
            containerRegistry.Register<GitHubRestApiClient>();
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
