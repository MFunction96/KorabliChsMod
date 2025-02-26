using System;

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

        /// <summary>
        /// 是否相等
        /// </summary>
        /// <param name="other">另一实例</param>
        /// <returns>true为相同，false为不同</returns>
        public bool Equals(ProxyConfig other)
        {
            return this.Enabled == other.Enabled
                && string.Compare(this.Address, other.Address, StringComparison.OrdinalIgnoreCase) == 0
                && string.Compare(this.Username, other.Username, StringComparison.OrdinalIgnoreCase) == 0
                && string.Compare(this.Password, other.Password, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
