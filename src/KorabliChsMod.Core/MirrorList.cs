using System;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 镜像列表
    /// </summary>
    public enum MirrorList
    {
        /// <summary>
        /// Github源
        /// </summary>
        GitHub = 0,
        /// <summary>
        /// Cloudflare源
        /// </summary>
        [Obsolete("Cloudflare源已不可用，请使用阿里云源")]
        Cloudflare = 1,
        /// <summary>
        /// 阿里云
        /// </summary>
        AliYun = 2
    }
}
