// ReSharper disable NonReadonlyMemberInGetHashCode
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

namespace Xanadu.KorabliChsMod.Core.Models
{
    /// <summary>
    /// 代理配置
    /// </summary>
    public class ProxyConfigModel
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

        /// <inheritdoc />
        public override bool Equals(object? other)
        {
            if (other is not ProxyConfigModel otherModel)
            {
                return false;
            }

            return this.Enabled == otherModel.Enabled &&
                   this.Address == otherModel.Address &&
                   this.Username == otherModel.Username &&
                   this.Password == otherModel.Password;
        }

    }

}
