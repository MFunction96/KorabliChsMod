namespace Xanadu.KorabliChsMod.Core.Config
{
    /// <summary>
    /// 代理配置
    /// </summary>
    public class ProxyConfig
    {
        /// <summary>
        /// 启用
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// 地址，支持http/https
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
