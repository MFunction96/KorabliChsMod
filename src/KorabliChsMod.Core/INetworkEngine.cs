﻿using System;
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
        /// <param name="address"></param>
        public bool SetProxy(string address = "");

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
    }
}
