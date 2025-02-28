using System;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 服务事件
    /// </summary>
    public interface IServiceEvent
    {
        /// <summary>
        /// 服务事件
        /// </summary>
        public event EventHandler<ServiceEventArg>? ServiceEvent;
    }
}
