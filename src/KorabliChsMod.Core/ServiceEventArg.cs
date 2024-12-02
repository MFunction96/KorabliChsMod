using System;

namespace Xanadu.KorabliChsMod.Core
{
    public class ServiceEventArg : EventArgs
    {
        public string Message { get; set; } = string.Empty;

        public Exception? Exception { get; set; }
    }
}
