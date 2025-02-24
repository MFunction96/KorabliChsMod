using System.Threading;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 汉化模组安装器
    /// </summary>
    public interface IChsModInstaller : IServiceEvent
    {
        /// <summary>
        /// 安装汉化模组
        /// </summary>
        /// <param name="mirror">镜像</param>
        /// <param name="cancellationToken">中止消息</param>
        /// <returns>成功返回true，失败返回false</returns>

        public Task<bool> Install(MirrorList mirror, CancellationToken cancellationToken = default);
    }
}
