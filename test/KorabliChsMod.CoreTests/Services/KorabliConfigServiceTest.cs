using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.KorabliChsMod.Core.Services;
using Xanadu.Skidbladnir.Net.DevOps.Service;

namespace Xanadu.Test.KorabliChsMod.Core.Services
{
    [TestClass]
    public class KorabliConfigServiceTest
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly string TestFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        /// <summary>
        /// 
        /// </summary>
        private static readonly string TestConfigPath = Path.Combine(KorabliConfigServiceTest.TestFolder, "config.json");

        [TestInitialize]
        public void Setup()
        {
            if (File.Exists(KorabliConfigServiceTest.TestConfigPath))
            {
                File.Delete(KorabliConfigServiceTest.TestConfigPath);
            }

            KorabliConfigModel.SetTestFolder(KorabliConfigServiceTest.TestFolder);
        }

        [TestMethod]
        public void Load_WhenConfigDoesNotExist_ShouldUseDefaultConfig()
        {
            var configService = new KorabliConfigService();
            var config = configService.Load();

            Assert.IsNotNull(config);
            Assert.AreEqual(KorabliConfigService.CurrentVersion, config.Version);
            Assert.IsTrue(File.Exists(KorabliConfigServiceTest.TestConfigPath));
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
            Assert.IsTrue(File.Exists(KorabliConfigServiceTest.TestConfigPath));

            var json = await File.ReadAllTextAsync(KorabliConfigServiceTest.TestConfigPath);
            Assert.Contains("TestGame", json);
        }

        [TestMethod]
        public async Task ErrorProxy_ShouldDisableProxy()
        {
            var configService = new KorabliConfigService
            {
                CurrentConfig =
                {
                    GameFolder = "TestGame",
                    Proxy = new ProxyConfigModel
                    {
                        Enabled = true,
                        Address = "invalid:proxy:address", // Invalid proxy address to trigger error
                    }
                }
            };

            var services = new ServiceCollection();
            services.AddSingleton(configService);
            services.AddHttpClient<NetworkEngine>(httpClient =>
            {
                httpClient.DefaultRequestVersion = HttpVersion.Version30;
                httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                foreach (var gitHubDefaultHeader in GitHubRestApiClient.GitHubDefaultHeaders)
                {
                    httpClient.DefaultRequestHeaders.Add(gitHubDefaultHeader.Key, gitHubDefaultHeader.Value);
                }

                httpClient.Timeout = TimeSpan.FromSeconds(30);
            }).ConfigurePrimaryHttpMessageHandler(provider =>
            {
                var korabliConfigService = provider.GetRequiredService<KorabliConfigService>();
                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.All
                };

                try
                {
                    if (korabliConfigService.CurrentConfig.Proxy.Enabled &&
                        !string.IsNullOrEmpty(korabliConfigService.CurrentConfig.Proxy.Address))
                    {
                        handler.Proxy = new WebProxy(korabliConfigService.CurrentConfig.Proxy.Address,
                            true, null,
                            new NetworkCredential(korabliConfigService.CurrentConfig.Proxy.Username,
                                korabliConfigService.CurrentConfig.Proxy.Password));
                    }
                }
                catch (Exception)
                {
                    lock (korabliConfigService.CurrentConfig.Proxy)
                    {
                        korabliConfigService.CurrentConfig.Proxy.Enabled = false;
                        korabliConfigService.SaveAsync().ConfigureAwait(false);
                    }
                }

                return handler;
            });

            var provider = services.BuildServiceProvider();
            var networkEngine = provider.GetRequiredService<NetworkEngine>();
            Assert.IsTrue(File.Exists(KorabliConfigServiceTest.TestConfigPath));

            var model = JsonSerializer.Deserialize<KorabliConfigModel>(await File.ReadAllTextAsync(KorabliConfigServiceTest.TestConfigPath, Encoding.UTF8))!;
            Assert.IsFalse(model.Proxy.Enabled, "Proxy should be disabled due to error.");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(KorabliConfigServiceTest.TestConfigPath))
            {
                File.Delete(KorabliConfigServiceTest.TestConfigPath);
            }
        }
    }

}
