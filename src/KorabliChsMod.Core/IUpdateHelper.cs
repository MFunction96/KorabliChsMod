using System;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    public interface IUpdateHelper
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
        public Task<Version> Check();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task Update();
    }
}
