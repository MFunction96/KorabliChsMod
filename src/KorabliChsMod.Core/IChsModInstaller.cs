﻿using System.Collections.Generic;
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
        /// 文件相对路径，bool值表示是否为文件夹
        /// </summary>
        public static IEnumerable<(string FilePath, bool IsDirectory)> RelativeFilePath =>
        [
            ("texts", true), ("locale_config.xml", false), ("LICENSE", false)
        ];

        /// <summary>
        /// 安装汉化模组
        /// </summary>
        /// <param name="mirror">镜像</param>
        /// <param name="cancellationToken">中止消息</param>
        /// <returns>成功返回true，失败返回false</returns>

        public Task<bool> Install(MirrorList mirror, CancellationToken cancellationToken = default);
    }
}
