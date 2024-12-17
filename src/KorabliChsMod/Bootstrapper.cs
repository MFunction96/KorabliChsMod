using Prism.DryIoc;
using Prism.Ioc;
using Serilog;
using System.Windows;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Config;
using Xanadu.KorabliChsMod.DI;
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
            containerRegistry.RegisterInstance(Log.Logger);
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
            containerRegistry.RegisterSingleton<IModInstaller, ModInstaller>();
        }
    }
}
