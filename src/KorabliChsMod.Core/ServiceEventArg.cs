using System;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 服务事件参数
    /// </summary>
    public class ServiceEventArg : EventArgs
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 异常
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// 消息是否追加异常信息
        /// </summary>
        public bool AppendException { get; set; } = true;
    }
}
