using Microsoft.Extensions.Logging;
using Prism.DryIoc;
using Prism.Ioc;
using Serilog;
using System.Text;
using System.Windows;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.KorabliChsMod.Core.Services;
using Xanadu.KorabliChsMod.DI;
using Xanadu.KorabliChsMod.ViewModels;
using Xanadu.KorabliChsMod.Views;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod
{
    /// <summary>
    /// 
    /// </summary>
    public class Bootstrapper : PrismBootstrapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册日志服务
            var loggerFactory = LoggerFactory.Create(builder =>
            {
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
            // 注册游戏探查服务
            containerRegistry.RegisterScoped<GameDetectorService>();
            // 注册Lesta Game Center探查服务
            containerRegistry.RegisterSingleton<ILgcIntegrator, LgcIntegrator>();
            // 注册网络引擎服务
            containerRegistry.RegisterScoped<NetworkEngine>();
            // 注册元数据获取服务
            containerRegistry.RegisterScoped<MetadataService>();
            // 注册缓存池服务
            containerRegistry.RegisterSingleton<FileCachePool>();
            // 注册更新助理服务
            containerRegistry.RegisterScoped<UpdateService>();
            // 注册Mod安装服务
            containerRegistry.RegisterScoped<ChsModService>();
            // 注册主窗口日志服务
            containerRegistry.RegisterSingleton<ILogger<MainWindowViewModel>, Logger<MainWindowViewModel>>();
            containerRegistry.RegisterForNavigation<MainWindowViewModel>();
        }
    }
}
