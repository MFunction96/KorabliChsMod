using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Config;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 元信息获取实现
    /// </summary>
    /// <param name="networkEngine"></param>
    public class MetadataFetcher(INetworkEngine networkEngine) : IMetadataFetcher
    {
        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <inheritdoc />
        public async Task<JToken?> GetLatestJToken(MirrorList mirrorList, bool preRelease = false, bool forcePre = false)
        {
            try
            {
                var response = await networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                    IKorabliFileHub.Links[mirrorList].UpdateMetadata), 5);
                _ = response!.EnsureSuccessStatusCode();
                var releases = await response.Content.ReadAsStringAsync();
                var jArray = JsonConvert.DeserializeObject<JArray>(releases) ?? [];
                if (forcePre)
                {
                    return jArray.First(q => q["prerelease"]!.Value<bool>() == preRelease);
                }

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
