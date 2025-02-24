using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 元信息获取器
    /// </summary>
    public interface IMetadataFetcher : IServiceEvent
    {
        /// <summary>
        /// 获取最新的元信息
        /// </summary>
        /// <param name="mirrorList">镜像</param>
        /// <param name="preRelease">预发布</param>
        /// <param name="forcePre">强制应用预发布配置</param>
        /// <returns>最新的元信息，如果获取失败则返回null</returns>
        public Task<JToken?> GetLatestJToken(MirrorList mirrorList, bool preRelease = false, bool forcePre = false);
    }
}
