using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Models;

namespace Xanadu.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 元信息获取实现，Transient 生命周期
    /// </summary>
    /// <param name="korabliConfigService"></param>
    /// <param name="networkEngine"></param>
    public class MetadataService(KorabliConfigService korabliConfigService, NetworkEngine networkEngine) : IServiceEvent
    {
        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <summary>
        /// 模组元信息获取服务
        /// </summary>
        /// <param name="gameVersion">当前版本号</param>
        /// <param name="preRelease">预发布</param>
        /// <returns>模组源</returns>
        public async Task<JToken> GetModJToken(Version gameVersion, bool preRelease = false)
        {
            try
            {
                var jArray = await this.GetMetadata(true);
                foreach (var jToken in jArray)
                {
                    var pre = jToken["prerelease"]!.Value<bool>();
                    if (pre ^ preRelease)
                    {
                        continue;
                    }
                    var tagName = jToken["tag_name"]!.Value<string>()!;
                    var tagVersion = tagName[(tagName.IndexOf(".", StringComparison.OrdinalIgnoreCase) + 1)..].Trim();
                    if (tagVersion.Contains('-'))
                    {
                        tagVersion = tagVersion[..tagVersion.IndexOf('-', StringComparison.OrdinalIgnoreCase)];
                    }

                    if (tagVersion.Count(q => q == '.') > 1)
                    {
                        tagVersion = tagVersion[..tagVersion.LastIndexOf('.')].Trim();
                    }

                    var version = Version.Parse(tagVersion);
                    if (version > gameVersion)
                    {
                        continue;
                    }

                    return jToken;
                }

                throw new NullReferenceException("未找到符合条件的汉化包");
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Exception = e,
                    Message = "获取最新的汉化包元信息失败，请检查网络连接或配置是否正确。"
                });

                throw;
            }
            
        }

        /// <summary>
        /// 应用元信息获取
        /// </summary>
        /// <returns></returns>
        public async Task<JToken?> GetAppJToken()
        {
            var jArray = await this.GetMetadata(false);
            return jArray.First();
        }

        /// <summary>
        /// 获取最新的元信息
        /// </summary>
        /// <param name="mod">true为mod，false为程序自身</param>
        /// <returns>最新的元信息，如果获取失败则返回null</returns>
        internal async Task<JArray> GetMetadata(bool mod)
        {
            var mirror = korabliConfigService.CurrentConfig.Mirror;
            var link = mod
                ? KorabliConfigModel.Links[mirror].ModMetadata
                : KorabliConfigModel.Links[mirror].UpdateMetadata;
            using var response = await networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get, link), 5);
            _ = response!.EnsureSuccessStatusCode();
            var releases = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JArray>(releases) ?? [];
        }
    }
}
