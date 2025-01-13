using Microsoft.Extensions.Logging;
using Prism.DryIoc;
using Prism.Ioc;
using Serilog;
using System.Text;
using System.Windows;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Config;
using Xanadu.KorabliChsMod.DI;
using Xanadu.KorabliChsMod.ViewModels;
using Xanadu.KorabliChsMod.Views;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod
{
    public class Bootstrapper : PrismBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册日志服务
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.File(IKorabliFileHub.LogFilePath,
                        encoding: Encoding.UTF8,
                        fileSizeLimitBytes: 50331648)
                    .CreateLogger());
            });
            // 注册 ILoggerFactory 到容器
            containerRegistry.RegisterInstance(loggerFactory);

            // 注册配置文件
            containerRegistry.RegisterSingleton<IKorabliFileHub, KorabliFileHub>();
            // 注册游戏探查服务
            containerRegistry.RegisterSingleton<IGameDetector, GameDetector>();
            // 注册Lesta Game Center探查服务
            containerRegistry.RegisterSingleton<ILgcIntegrator, LgcIntegrator>();
            // 注册网络引擎服务
            containerRegistry.RegisterSingleton<INetworkEngine, NetworkEngine>();
            // 注册缓存池服务
            containerRegistry.RegisterSingleton<IFileCachePool, FileCachePool>();
            // 注册更新助理服务
            containerRegistry.RegisterSingleton<IUpdateHelper, UpdateHelper>();
            // 注册Mod安装服务
            containerRegistry.RegisterSingleton<IChsModInstaller, ChsModInstaller>();
            // 注册主窗口日志服务
            containerRegistry.RegisterSingleton<ILogger<MainWindowViewModel>, Logger<MainWindowViewModel>>();
            containerRegistry.RegisterSingleton<ILogger<FileCachePool>, Logger<FileCachePool>>();
            containerRegistry.RegisterForNavigation<MainWindowViewModel>();
        }
    }
}
