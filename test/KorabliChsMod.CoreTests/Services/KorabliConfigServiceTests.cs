using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
            if (File.Exists(KorabliConfigServiceTests.TestConfigPath))
            {
                File.Delete(KorabliConfigServiceTests.TestConfigPath);
            }

            KorabliConfigModel.SetTestFolder(KorabliConfigServiceTests.TestFolder);
        }

        [TestMethod]
        public void Load_WhenConfigDoesNotExist_ShouldUseDefaultConfig()
        {
            var configService = new KorabliConfigService();
            var config = configService.Load();

            Assert.IsNotNull(config);
            Assert.AreEqual(KorabliConfigService.CurrentVersion, config.Version);
            Assert.IsTrue(File.Exists(KorabliConfigServiceTests.TestConfigPath));
        }

        [TestMethod]
        public async Task SaveAsync_ShouldWriteFile()
        {
            var configService = new KorabliConfigService
            {
                CurrentConfig =
                {
                    GameFolder = "TestGame"
                }
            };

            var result = await configService.SaveAsync();

            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(KorabliConfigServiceTests.TestConfigPath));

            var json = File.ReadAllText(KorabliConfigServiceTests.TestConfigPath);
            Assert.IsTrue(json.Contains("TestGame"));
        }

        [TestMethod]
        public async Task ErrorProxy_ShouldDisableProxy()
        {
            var configService = new KorabliConfigService
            {
                CurrentConfig =
                {
                    GameFolder = "TestGame",
                    Proxy = new()
                    {
                        Enabled = true,
                        Address = "invalid:proxy:address", // Invalid proxy address to trigger error
                    }
                }
            };

            var networkEngine = new NetworkEngine(configService);
            networkEngine.Dry();
            Assert.IsTrue(File.Exists(KorabliConfigServiceTests.TestConfigPath));

            var model = JsonConvert.DeserializeObject<KorabliConfigModel>(await File.ReadAllTextAsync(KorabliConfigServiceTests.TestConfigPath, System.Text.Encoding.UTF8))!;
            Assert.IsFalse(model.Proxy.Enabled, "Proxy should be disabled due to error.");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(KorabliConfigServiceTests.TestConfigPath))
            {
                File.Delete(KorabliConfigServiceTests.TestConfigPath);
            }
        }
    }

}
