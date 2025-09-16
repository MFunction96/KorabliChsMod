using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
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
    }
}
