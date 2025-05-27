namespace Xanadu.KorabliChsMod.Core.Models
{
    /// <summary>
    /// 资源链接
    /// </summary>
    public class ResourceLinkModel
    {
        /// <summary>
        /// 镜像
        /// </summary>
        public required MirrorList Mirror { get; set; }

        /// <summary>
        /// 模组Metadata位置
        /// </summary>
        public required string ModMetadata { get; set; }

        /// <summary>
        /// 自更新Metadata位置
        /// </summary>
        public required string UpdateMetadata { get; set; }
    }
}
