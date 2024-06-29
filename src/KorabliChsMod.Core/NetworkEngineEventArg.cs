using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    public class NetworkEngineEventArg : EventArgs
    {
        public string Message { get; set; } = string.Empty;

        public Exception? Exception { get; set; }
    }
}
