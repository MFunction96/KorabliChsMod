using System;

namespace Xanadu.KorabliChsMod.Core
{
    public interface IServiceEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ServiceEventArg>? ServiceEvent;
    }
}
