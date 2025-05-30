using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.KorabliChsMod.Core.Services;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.Test.KorabliChsMod.Core.Services
{
    [TestClass]
    public class ChsModServiceTest
    {
        private IServiceProvider _serviceProvider = null!;
        private readonly string _testBasePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        [TestInitialize]
        public void Setup()
        {
            Directory.CreateDirectory(this._testBasePath);
            KorabliConfigModel.SetTestFolder(this._testBasePath);
            var services = new ServiceCollection();
            services.AddSingleton<KorabliConfigService>();
            services.AddSingleton<FileCachePool>();
            services.AddScoped<NetworkEngine>();
            services.AddScoped<MetadataService>();
            services.AddScoped<ChsModService>();
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
        public async Task Install_Cloudflare()
        {
            // Arrange
            var model = new GameDetectModel
            {
                Folder = Path.Combine(_testBasePath, "game"),
                ServerVersion = "1.0.0.12345",
                ClientVersion = "1.0.0.12345",
                PreInstalled = true,
                IsTest = false
            };
            Directory.CreateDirectory(model.Folder);

            // Act
            using var scope = _serviceProvider.CreateScope();
            var chsModService = scope.ServiceProvider.GetRequiredService<ChsModService>();
            var result = await chsModService.Install(model);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(Directory.Exists(model.ModFolder));
            Assert.IsTrue(File.Exists(Path.Combine(model.ModFolder, "locale_config.xml")));
            Assert.IsTrue(File.Exists(Path.Combine(model.ModFolder, "LICENSE")));
            Assert.IsTrue(File.Exists(Path.Combine(model.ModFolder, "thanks.md")));
            Assert.IsTrue(File.Exists(Path.Combine(model.ModFolder, "change.log")));
        }

        [TestMethod]
        public async Task Install_Github()
        {
            // Arrange
            var model = new GameDetectModel
            {
                Folder = Path.Combine(_testBasePath, "game"),
                ServerVersion = "1.0.0.12345",
                ClientVersion = "1.0.0.12345",
                PreInstalled = true,
                IsTest = false
            };
            Directory.CreateDirectory(model.Folder);

            // Act
            using var scope = _serviceProvider.CreateScope();
            var korabliConfigService = scope.ServiceProvider.GetRequiredService<KorabliConfigService>();
            korabliConfigService.CurrentConfig.Mirror = MirrorList.Github;
            var chsModService = scope.ServiceProvider.GetRequiredService<ChsModService>();
            var result = await chsModService.Install(model);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(Directory.Exists(model.ModFolder));
            Assert.IsTrue(File.Exists(Path.Combine(model.ModFolder, "locale_config.xml")));
            Assert.IsTrue(File.Exists(Path.Combine(model.ModFolder, "LICENSE")));
            Assert.IsTrue(File.Exists(Path.Combine(model.ModFolder, "thanks.md")));
            Assert.IsTrue(File.Exists(Path.Combine(model.ModFolder, "change.log")));
        }
    }

}
