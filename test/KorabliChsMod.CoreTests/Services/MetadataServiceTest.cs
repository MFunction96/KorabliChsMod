using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Services;
using Xanadu.Skidbladnir.Net.DevOps;
using Xanadu.Skidbladnir.Net.DevOps.Model.GitHub.Basic;
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

        public TestContext TestContext { get; set; } = null!;

        [TestInitialize]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddSingleton<KorabliConfigService>();
            services.AddGitHubRestApiClient();
            services.AddHttpClient<NetworkEngine>(RestApiClient.DefaultHttpClientAction)
                .ConfigurePrimaryHttpMessageHandler(() => RestApiClient.DefaultHttpClientHandler());
            services.AddTransient<MetadataService>();
            this._provider = services.BuildServiceProvider();
            var korabliConfigService = this._provider.GetRequiredService<KorabliConfigService>();
            korabliConfigService.Load();
            korabliConfigService.CurrentConfig.Mirror = MirrorList.AliYun;
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public async Task VersionRegex()
        {
            var githubRestApiClient = this._provider.GetRequiredService<GitHubRestApiClient>();
            var chsModGitHubModel = new GitHubRepositoryInfoModel
            {
                Owner = "DDFantasyV",
                Repository = "Korabli_localization_chs"
            };

            var releases = await githubRestApiClient.ListReleases(chsModGitHubModel);
            Assert.IsNotNull(releases);
            foreach (var releaseModel in releases.Take(10))
            {
                this.TestContext.WriteLine(MetadataService.VersionRegex.Match(releaseModel.TagName).Groups["Version"].Value);
                Assert.IsTrue(MetadataService.VersionRegex.IsMatch(releaseModel.TagName));
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task GetModRelease(bool prerelease)
        {
            var metadataService = this._provider.GetRequiredService<MetadataService>();
            var now = DateTimeOffset.Now;
            var version = prerelease ? Version.Parse($"{now:yy}.{now.Month + 1}") : Version.Parse($"{now:yy}.{now.Month}");
            var releases = await metadataService.GetModRelease(version, prerelease);
            // Assert.IsNotNull(releases);
            this.TestContext.WriteLine(JsonSerializer.Serialize(releases, new JsonSerializerOptions { WriteIndented = true }));
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task GetAppRelease(bool prerelease)
        {
            var metadataService = this._provider.GetRequiredService<MetadataService>();
            var releases = await metadataService.GetAppRelease();
            Assert.IsNotNull(releases);
            this.TestContext.WriteLine(JsonSerializer.Serialize(releases, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
