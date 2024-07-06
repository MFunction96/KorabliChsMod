using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Config;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<App>();
                    services.AddSingleton<MainWindow>();
                    // 注册日志组件
                    services.AddLogging(configuration => configuration.AddConsole().AddSerilog());
                    // 注册日志文件输出
                    services.AddSerilog(configuration => configuration
                        .Enrich.FromLogContext()
                        .WriteTo.File(IKorabliFileHub.LogFilePath));
                    // 注册配置文件
                    services.AddSingleton<IKorabliFileHub, KorabliFileHub>();
                    // 注册游戏探查服务
                    services.AddSingleton<IGameDetector, GameDetector>();
                    // 注册网络引擎服务
                    services.AddSingleton<INetworkEngine, NetworkEngine>();
                    // 注册缓存池服务
                    services.AddSingleton<ICachePool, CachePool>();
                })
                .Build();

            var app = host.Services.GetService<App>()!;
            app.Run();
        }
    }
}
