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
        /// 获取最新的元信息
        /// </summary>
        /// <param name="mod">true为mod，false为程序自身</param>
        /// <param name="preRelease">预发布</param>
        /// <returns>最新的元信息，如果获取失败则返回null</returns>
        public async Task<JToken?> GetLatestJToken(bool mod, bool preRelease = false)
        {
            try
            {
                var mirror = korabliConfigService.CurrentConfig.Mirror;
                var link = mod
                    ? KorabliConfigModel.Links[mirror].ModMetadata
                    : KorabliConfigModel.Links[mirror].UpdateMetadata;
                using var response = await networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get, link), 5);
                _ = response!.EnsureSuccessStatusCode();
                var releases = await response.Content.ReadAsStringAsync();
                var jArray = JsonConvert.DeserializeObject<JArray>(releases) ?? [];
                return preRelease ? jArray.First() : jArray.First(q => !q["prerelease"]!.Value<bool>());
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = "获取版本信息失败！",
                    Exception = e,
                    AppendException = false
                });

                return null;
            }
        }
    }
}
