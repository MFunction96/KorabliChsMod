using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.Skidbladnir.Net.DevOps.Model.GitHub.Basic;
using Xanadu.Skidbladnir.Net.DevOps.Model.GitHub.Release;
using Xanadu.Skidbladnir.Net.DevOps.Service;

namespace Xanadu.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 元信息获取实现，Transient 生命周期
    /// </summary>
    /// <param name="korabliConfigService"></param>
    /// <param name="networkEngine"></param>
    public partial class MetadataService(KorabliConfigService korabliConfigService, NetworkEngine networkEngine, GitHubRestApiClient gitHubRestApiClient) : IServiceEvent
    {
        [GeneratedRegex(@"([Vv]\d+\.(?<Version>\d+\.\d+))", RegexOptions.ExplicitCapture)]
        private static partial Regex VersionTagRegex();

        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <summary>
        /// 元数据版本号正则表达式
        /// </summary>
        public static Regex VersionRegex => MetadataService.VersionTagRegex();

        /// <summary>
        /// 模组元信息获取服务
        /// </summary>
        /// <param name="gameVersion">当前版本号</param>
        /// <param name="preRelease">预发布</param>
        /// <returns>模组源</returns>
        public async Task<ReleaseModel> GetModRelease(Version gameVersion, bool preRelease = false)
        {
            try
            {
                var releaseModels = await this.GetMetadata(true);
                if (releaseModels is null || releaseModels.Length == 0)
                {
                    throw new NullReferenceException("元数据获取失败！");
                }

                var releaseModel = releaseModels.FirstOrDefault(q =>
                    q.Prerelease == preRelease &&
                    (gameVersion.Major < 26 || Version.Parse(MetadataService.VersionRegex.Match(q.TagName).Groups["Version"].Value) <= gameVersion));
                return releaseModel ?? throw new NullReferenceException("未找到符合条件的汉化包");
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Exception = e,
                    Message = $"获取最新的汉化包元信息失败，请检查网络连接或配置是否正确。内部错误：{e.Message}"
                });

                throw;
            }

        }

        /// <summary>
        /// 应用元信息获取
        /// </summary>
        /// <returns></returns>
        public async Task<ReleaseModel?> GetAppRelease()
        {
            var releaseModels = await this.GetMetadata(false);
            return releaseModels?.FirstOrDefault();
        }

        /// <summary>
        /// 获取最新的元信息
        /// </summary>
        /// <param name="mod">true为mod，false为程序自身</param>
        /// <returns>最新的元信息，如果获取失败则返回null</returns>
        internal async Task<ReleaseModel[]?> GetMetadata(bool mod)
        {
            var mirror = korabliConfigService.CurrentConfig.Mirror;
            if (mirror == MirrorList.GitHub)
            {
                var gitHubRepositoryInfoModel = mod
                    ? new GitHubRepositoryInfoModel
                    {
                        Owner = "DDFantasyV",
                        Repository = "Korabli_localization_chs"
                    }
                    : new GitHubRepositoryInfoModel
                    {
                        Owner = "MFunction96",
                        Repository = "KorabliChsMod"
                    };
                return await gitHubRestApiClient.ListReleases(gitHubRepositoryInfoModel);
            }

            var link = mod
                ? KorabliConfigModel.Links[mirror].ModMetadata
                : KorabliConfigModel.Links[mirror].UpdateMetadata;
            using var request = new HttpRequestMessage(HttpMethod.Get, link);
            if (mirror == MirrorList.Kodo)
            {
                request.Headers.Referrer = new Uri("https://korablichsmod-kodo.mfbrain.xyz/");
            }

            using var response = await networkEngine.SendAsync(request, 5);
            _ = response!.EnsureSuccessStatusCode();
            try
            {
                return await response.Content.ReadFromJsonAsync<ReleaseModel[]>();
            }
            catch (Exception e)
            {
                var rawContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"获取元信息失败，响应内容：{rawContent}", e);
            }
        }


    }
}
