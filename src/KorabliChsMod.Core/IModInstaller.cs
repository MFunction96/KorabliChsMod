using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    public interface IModInstaller : IServiceEvent
    {
        public Task<bool> Install(CancellationToken cancellationToken = default);
    }
}
