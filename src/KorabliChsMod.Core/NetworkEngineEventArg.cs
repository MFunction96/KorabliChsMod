using System;

namespace Xanadu.KorabliChsMod.Core
{
    public class NetworkEngineEventArg : EventArgs
    {
        public string Message { get; set; } = string.Empty;

        public Exception? Exception { get; set; }
    }
}
