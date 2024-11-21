using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface INetworkEngine : IDisposable
    {
        public ConcurrentDictionary<string, string> Headers { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        public bool SetProxy(Uri? uri);

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<NetworkEngineEventArg>? NetworkEngineEvent;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="retry"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, int retry = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filePath"></param>
        /// <param name="retry"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task DownloadAsync(HttpRequestMessage request, string filePath, int retry = 0,
            CancellationToken cancellationToken = default);
    }
}
