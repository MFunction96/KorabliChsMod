using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Services;
using Xanadu.Skidbladnir.Net.DevOps;
using Xanadu.Skidbladnir.Net.DevOps.Service;

namespace Xanadu.Test.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 元信息获取实现，Transient 生命周期
    /// </summary>
    [TestClass]
    public class MetadataServiceTest
    {
        private IServiceProvider _provider = null!;

        [TestInitialize]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new KorabliConfigService());
            services.AddGitHubRestApiClient();
            services.AddTransient<MetadataService>();
            this._provider = services.BuildServiceProvider();
        }

        [TestCleanup]
        public void Cleanup()
        {
            
        }
    }
}
