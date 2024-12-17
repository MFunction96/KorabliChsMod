using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Text;
using System.Windows;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Config;
using Xanadu.KorabliChsMod.DI;
using Xanadu.Skidbladnir.IO.File.Cache;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace KorabliChsMod
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            var services = new ServiceCollection();
            // 注册日志组件
            services.AddLogging(configuration => configuration.AddConsole().AddSerilog());
            // 注册日志文件输出，48MB大小限制
            services.AddSerilog(configuration => configuration
                .Enrich.FromLogContext()
                .WriteTo.File(IKorabliFileHub.LogFilePath, encoding: Encoding.UTF8, fileSizeLimitBytes: 50331648));
            // 注册配置文件
            services.AddSingleton<IKorabliFileHub, KorabliFileHub>();
            // 注册游戏探查服务
            services.AddSingleton<IGameDetector, GameDetector>();
            // 注册Lesta Game Center探查服务
            services.AddSingleton<ILgcIntegrator, LgcIntegrator>();
            // 注册网络引擎服务
            services.AddSingleton<INetworkEngine, NetworkEngine>();
            // 注册缓存池服务
            services.AddSingleton<IFileCachePool, FileCachePool>();
            // 注册更新助理服务
            services.AddSingleton<IUpdateHelper, UpdateHelper>();
            // 注册Mod安装服务
            services.AddSingleton<IModInstaller, ModInstaller>();
            
            App.ServiceProvider = services.BuildServiceProvider();
        }
    }

}
