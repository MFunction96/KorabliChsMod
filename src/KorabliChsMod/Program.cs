using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using Xanadu.KorabliChsMod.Config;
using Xanadu.KorabliChsMod.Core;

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
                    services.AddSingleton<IGameDetector, GameDetector>();
                    services.AddSingleton<INetworkEngine, NetworkEngine>();
                    services.AddSerilog(configuration => configuration
                        .Enrich.FromLogContext()
                        .WriteTo.File(KorabliConfig.LogFile));
                })
                .Build();

            var app = host.Services.GetService<App>()!;
            app.Run();
        }
    }
}
