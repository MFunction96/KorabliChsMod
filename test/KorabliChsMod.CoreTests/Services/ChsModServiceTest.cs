using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.KorabliChsMod.Core.Services;
using Xanadu.Skidbladnir.IO.File.Cache;
using Xanadu.Skidbladnir.Net.DevOps;

namespace Xanadu.Test.KorabliChsMod.Core.Services
{
    [TestClass]
    public class ChsModServiceTest
    {
        private IServiceProvider _serviceProvider = null!;

        private readonly string _testBasePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        public TestContext TestContext { get; set; } = null!;

        [TestInitialize]
        public void Setup()
        {
            Directory.CreateDirectory(this._testBasePath);
            KorabliConfigModel.SetTestFolder(this._testBasePath);
            var services = new ServiceCollection();
            services.AddSingleton<KorabliConfigService>();
            services.AddGitHubRestApiClient();
            services.AddSingleton<FileCachePool>();
            services.AddHttpClient<NetworkEngine>(RestApiClient.DefaultHttpClientAction)
                .ConfigurePrimaryHttpMessageHandler(() => RestApiClient.DefaultHttpClientHandler());
            services.AddTransient<MetadataService>();
            services.AddTransient<ChsModService>();
            this._serviceProvider = services.BuildServiceProvider();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(this._testBasePath))
            {
                Directory.Delete(this._testBasePath, true);
            }
        }

        [TestMethod]
        [DataRow("25.1.0.0.8798761")]
        [DataRow("13.6.0.0.8601080")]
        [DataRow("25.1.0.8798761")]
        [DataRow("25.1.0.0.0.8798761")]
        public void VersionRegex(string gameVersion)
        {
            this.TestContext.WriteLine(ChsModService.VersionRegex.Match(gameVersion).Groups["Version"].Value);
            Assert.IsTrue(ChsModService.VersionRegex.IsMatch(gameVersion));
        }

        [TestMethod]
        [DataRow(true)]
        public async Task Install_AliYun(bool prerelease)
        {
            // Arrange
            var now = DateTimeOffset.Now;
            var version = prerelease ? $"{now:yy}.{now.Month + 1}.0.0.8601080" : $"{now:yy}.{now.Month}.0.0.8601080";
            var model = new GameDetectModel
            {
                Folder = Path.Combine(_testBasePath, "game"),
                ServerVersion = version,
                ClientVersion = version,
                PreInstalled = true,
                IsTest = prerelease
            };
            Directory.CreateDirectory(model.Folder);

            // Act
            using var scope = _serviceProvider.CreateScope();
            var korabliConfigService = scope.ServiceProvider.GetRequiredService<KorabliConfigService>();
            korabliConfigService.CurrentConfig.Mirror = MirrorList.AliYun;
            var chsModService = scope.ServiceProvider.GetRequiredService<ChsModService>();
            var result = await chsModService.Install(model, this.TestContext.CancellationTokenSource.Token);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(Directory.Exists(model.ModFolder));
            Assert.IsTrue(File.Exists(Path.Combine(model.ModFolder, "MK_L10N_CHS.mkmod")));
        }

        [TestMethod]
        [DataRow(true)]
        public async Task Install_Github(bool prerelease)
        {
            // Arrange
            var now = DateTimeOffset.Now;
            var version = prerelease ? $"{now:yy}.{now.Month + 1}" : $"{now:yy}.{now.Month}";
            var model = new GameDetectModel
            {
                Folder = Path.Combine(_testBasePath, "game"),
                ServerVersion = version,
                ClientVersion = version,
                PreInstalled = true,
                IsTest = prerelease
            };
            Directory.CreateDirectory(model.Folder);

            // Act
            using var scope = _serviceProvider.CreateScope();
            var korabliConfigService = scope.ServiceProvider.GetRequiredService<KorabliConfigService>();
            korabliConfigService.CurrentConfig.Mirror = MirrorList.GitHub;
            var chsModService = scope.ServiceProvider.GetRequiredService<ChsModService>();
            var result = await chsModService.Install(model, this.TestContext.CancellationTokenSource.Token);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(Directory.Exists(model.ModFolder)); 
            Assert.IsTrue(File.Exists(Path.Combine(model.ModFolder, "MK_L10N_CHS.mkmod")));
        }

    }

}
