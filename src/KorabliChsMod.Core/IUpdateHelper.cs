using System;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 升级助手
    /// </summary>
    public interface IUpdateHelper : IServiceEvent
    {

        /// <summary>
        /// 最新版本
        /// </summary>
        public Version? LatestVersion { get; }

        /// <summary>
        /// 检查更新是否可用
        /// </summary>
        /// <returns>有更新返回true，已是最新返回false</returns>
        public Task<bool> Check(MirrorList mirrorList, Version appVersion);

        /// <summary>
        /// 执行更新
        /// </summary>
        /// <returns>完成预更新返回true，未完成返回false</returns>
        public Task<bool> Update(MirrorList mirrorList);
    }
}
