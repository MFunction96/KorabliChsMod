using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xanadu.KorabliChsMod.Core.Services;
using Xanadu.KorabliChsMod.Services;

namespace Xanadu.Test.KorabliChsMod.Services
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class LgcIntegratorServiceTest
    {
        public TestContext TestContext { get; set; } = null!;
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void LgcTest()
        {
            var service = new ServiceCollection();
            service.AddSingleton<LgcIntegratorService>();
            service.AddTransient<GameDetectorService>();
            var serviceProvider = service.BuildServiceProvider();
            var lgcIntegrator = serviceProvider.GetRequiredService<LgcIntegratorService>();
            var lgcIntegratorModel = lgcIntegrator.Load();
            if (lgcIntegratorModel is not null)
            {
                TestContext.WriteLine(JsonConvert.SerializeObject(lgcIntegratorModel, Formatting.Indented));
            }
        }
    }
}
