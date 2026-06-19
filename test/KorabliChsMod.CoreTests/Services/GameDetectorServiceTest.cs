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
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class GameDetectorServiceTest
    {
        private IServiceProvider _serviceProvider = null!;

        private readonly string _testBasePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        public TestContext TestContext { get; set; } = null!;

        public void OnServiceEvent(object? sender, ServiceEventArg e)
        {
            if (e.Exception is not null)
            {
                this.TestContext.WriteLine(e.Message);
                this.TestContext.WriteLine(e.Exception.ToString());
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            Directory.CreateDirectory(this._testBasePath);
            KorabliConfigModel.SetTestFolder(this._testBasePath);
            var services = new ServiceCollection();
            services.AddSingleton<KorabliConfigService>();
            services.AddGitHubRestApiClient();
            services.AddSingleton<FileCachePool>();
            services.AddHttpClient<NetworkEngine>(RestApiClient.DefaultHttpClientAction)
                .ConfigurePrimaryHttpMessageHandler(() => RestApiClient.DefaultSocketsHttpHandler());
            services.AddTransient<GameDetectorService>();
            this._serviceProvider = services.BuildServiceProvider();
        }

        //[TestMethod]
        //[DataRow("8601080_0", "13.6.0.0.8601080")]
        //[DataRow("8796161_0", "13.11.0.8796161")]
        //[DataRow("8824884_0", "25.10.0.8824884")]
        //[DataRow("8824884_1", "25.10.0.8824884")]
        //[DataRow("8824884_2", "25.10.0.8824884")]
        //[DataRow("8824884_3", "25.10.0.8824884")]
        //[DataRow("8824884_4", "25.9.0.8821884")]
        //public void VersionDetectTest(string subFolder, string expected)
        //{
        //    using var scope = this._serviceProvider.CreateScope();
        //    var gameDetector = scope.ServiceProvider.GetRequiredService<GameDetectorService>();
        //    var model = gameDetector.Load(Path.Combine("assets", "GameDetectorService", subFolder));
        //    Assert.IsNotNull(model);
        //    Assert.AreEqual(expected, model.GameVersion);
        //}

        [TestMethod]
        [DataRow("8824884_0", "25.10.0.0.8824884", false)]
        [DataRow("8824884_1", "25.10.0.0.8824884", true)]
        [DataRow("8824884_2", "25.10.0.0.8824884", false)]
        [DataRow("8824884_3", "25.10.0.0.8824884", true)]
        public void PathXmlCheck(string subFolder, string version, bool expected)
        {
            var gameDetectModel = new GameDetectModel
            {
                Folder = Path.Combine("assets", "GameDetectorService", subFolder),
                ClientVersion = version,
                ServerVersion = version,
                IsTest = true
            };

            Assert.AreEqual(expected, GameDetectorService.PathXmlCheck(gameDetectModel));
        }

        [TestMethod]
        [DataRow("8824884_0", "25.10.0.0.8824884", true)]
        [DataRow("8824884_1", "25.10.0.0.8824884", false)]
        [DataRow("8824884_2", "25.10.0.0.8824884", false)]
        [DataRow("8824884_3", "25.10.0.0.8824884", true)]
        public void ChsModPackCheck(string subFolder, string version, bool expected)
        {
            var gameDetectModel = new GameDetectModel
            {
                Folder = Path.Combine("assets", "GameDetectorService", subFolder),
                ClientVersion = version,
                ServerVersion = version,
                IsTest = true
            };

            Assert.AreEqual(expected, GameDetectorService.ChsModPackCheck(gameDetectModel));
        }

        [TestMethod]
        [DataRow("Korabli", true, false)]
        [DataRow("Korabli_PT", true, true)]
        [DataRow("Tanki", false, true)]
        [DataRow("Tanks_Blitz", false, true)]
        public void WarshipCheck(string subFolder, bool isWarship, bool isTest)
        {
            using var scope = this._serviceProvider.CreateScope();
            var gameDetector = scope.ServiceProvider.GetRequiredService<GameDetectorService>();
            var model = gameDetector.Load(Path.Combine("assets", "GameDetectorService", subFolder));
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IsWarship, isWarship);
            Assert.AreEqual(model.IsTest, isTest);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task OnlineServerVersion(bool prerelease)
        {
            using var scope = this._serviceProvider.CreateScope();
            var gameDetector = scope.ServiceProvider.GetRequiredService<GameDetectorService>();
            gameDetector.ServiceEvent += this.OnServiceEvent;
            var version = await gameDetector.OnlineServerVersion(prerelease);
            this.TestContext.WriteLine(version);
        }
    }
}