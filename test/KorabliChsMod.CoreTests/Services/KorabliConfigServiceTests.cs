using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.KorabliChsMod.Core.Services;

namespace Xanadu.Test.KorabliChsMod.Core.Services
{
    [TestClass]
    public class KorabliConfigServiceTests
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly string TestFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        /// <summary>
        /// 
        /// </summary>
        private static readonly string TestConfigPath = Path.Combine(KorabliConfigServiceTests.TestFolder, "config.json");

        [TestInitialize]
        public void Setup()
        {
            if (File.Exists(TestConfigPath))
                File.Delete(TestConfigPath);

            KorabliConfigModel.SetTestFolder(TestFolder);
        }

        [TestMethod]
        public void Load_WhenConfigDoesNotExist_ShouldUseDefaultConfig()
        {
            var services = new ServiceCollection();
            services.AddScoped<NetworkEngine>();
            services.AddSingleton<KorabliConfigService>();
            var provider = services.BuildServiceProvider();
            var configService = provider.GetRequiredService<KorabliConfigService>();

            var config = configService.Load();

            Assert.IsNotNull(config);
            Assert.AreEqual(KorabliConfigService.CurrentVersion, config.Version);
            Assert.IsTrue(File.Exists(TestConfigPath));
        }

        [TestMethod]
        public async Task SaveAsync_ShouldWriteFile()
        {
            var services = new ServiceCollection();
            services.AddScoped<NetworkEngine>();
            services.AddSingleton<KorabliConfigService>();
            var provider = services.BuildServiceProvider();
            var configService = provider.GetRequiredService<KorabliConfigService>();
            configService.CurrentConfig.GameFolder = "TestGame";

            var result = await configService.SaveAsync();

            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(TestConfigPath));

            var json = File.ReadAllText(TestConfigPath);
            Assert.IsTrue(json.Contains("TestGame"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(TestConfigPath))
                File.Delete(TestConfigPath);
        }
    }

}
