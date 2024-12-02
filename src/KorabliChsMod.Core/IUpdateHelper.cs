using System;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    public interface IUpdateHelper : IServiceEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public MirrorList Mirror { get; }

        /// <summary>
        /// 
        /// </summary>
        public Version? LatestVersion { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<bool> UpdateAvailable(Version appVersion);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task Update();
    }
}
