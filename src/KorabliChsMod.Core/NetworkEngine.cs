using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    public sealed class NetworkEngine(ILogger<NetworkEngine> logger) : INetworkEngine
    {
        private bool _disposed;

        private HttpClient Client { get; set; } = new();

        public event EventHandler<NetworkEngineEventArg>? NetworkEngineEvent;

        public ConcurrentDictionary<string, string> Headers { get; } = new();

        public bool SetProxy(Uri? uri)
        {
            try
            {
                lock (this.Client)
                {
                    var handler = new HttpClientHandler
                    {
                        AllowAutoRedirect = true,
                        AutomaticDecompression = DecompressionMethods.All
                    };

                    if (uri is not null)
                    {
                        handler.Proxy = new WebProxy(uri);
                    }

                    this.Client = new HttpClient(handler, true)
                    {
                        DefaultRequestVersion = Version.Parse("2.0"),
                        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                    };

                }
                
            }
            catch (Exception e)
            {
                logger.LogError(e, string.Empty);
                return false;
            }

            return true;
        }

        public async Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, int retry = 0, CancellationToken cancellationToken = default)
        {
            foreach (var header in this.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            if (retry == 0)
            {
                this.NetworkEngineEvent?.Invoke(this, new NetworkEngineEventArg
                {
                    Message = $"开始单次请求：{request.RequestUri?.AbsoluteUri}"
                });

                var res = await this.Client.SendAsync(request, cancellationToken);

                logger.LogInformation($"{res.StatusCode} -> {request.RequestUri?.AbsoluteUri}");
                this.NetworkEngineEvent?.Invoke(this, new NetworkEngineEventArg
                {
                    Message = $"结束单次请求：{res.StatusCode} {request.RequestUri?.AbsoluteUri}"
                });

                return res;
            }

            HttpResponseMessage? response = null;
            while (retry-- > 0)
            {
                try
                {
                    this.NetworkEngineEvent?.Invoke(this, new NetworkEngineEventArg
                    {
                        Message = $"开始请求：{request.RequestUri?.AbsoluteUri}"
                    });

                    response = await this.Client.SendAsync(request, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    logger.LogInformation($"{response.StatusCode} -> {request.RequestUri?.AbsoluteUri}");
                    this.NetworkEngineEvent?.Invoke(this, new NetworkEngineEventArg
                    {
                        Message = $"结束请求：{response.StatusCode} {request.RequestUri?.AbsoluteUri}"
                    });

                    return response;
                }
                catch (Exception e)
                {
                    logger.LogError(e, string.Empty);
                    this.NetworkEngineEvent?.Invoke(this, new NetworkEngineEventArg
                    {
                        Message = $"请求失败，剩余尝试 {retry} 次数。{response?.StatusCode} {request.RequestUri?.AbsoluteUri}",
                        Exception = e
                    });
                }

                Thread.Sleep(1000);
            }

            this.NetworkEngineEvent?.Invoke(this, new NetworkEngineEventArg
            {
                Message = $"请求失败 {response?.StatusCode} {request.RequestUri?.AbsoluteUri}"
            });

            return null;
        }

        public async Task DownloadAsync(HttpRequestMessage request, string filePath, int retry = 0,
            CancellationToken cancellationToken = default)
        {
            using var response = await this.SendAsync(request, 5, cancellationToken);
            try
            {
                if (response is null || !response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException();
                }

                await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await using var fsb = new BufferedStream(fs);
                await using var nsb = new BufferedStream(await response.Content.ReadAsStreamAsync(cancellationToken));
                await nsb.CopyToAsync(fsb, cancellationToken);
                nsb.Close();
                fsb.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                logger.LogError(e, string.Empty);
                this.NetworkEngineEvent?.Invoke(this, new NetworkEngineEventArg
                {
                    Message = $"请求失败，剩余尝试 {retry} 次数。{response?.StatusCode} {request.RequestUri?.AbsoluteUri}",
                    Exception = e
                });

                throw;
            }

        }

        #region Disposing

        /// <summary>
        /// Implement IDisposable. Do not make this method virtual. A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// </summary>
        /// <param name="disposing">If disposing equals true, the method has been called directly, or indirectly by a user's code. Managed and unmanaged resources can be disposed. If disposing equals false, the method has been called by the runtime from inside the finalizer and you should not reference other objects. Only unmanaged resources can be disposed.</param>
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (this._disposed)
            {
                return;
            }

            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                this.Client.Dispose();
                logger.LogInformation($"{this.GetType().Name} disposing");

            }
            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // If disposing is false,
            // only the following code is executed.

            this._disposed = true;

        }

        /// <summary>
        /// The finalize method.
        /// </summary>
        ~NetworkEngine()
        {
            this.Dispose(false);
        }

        #endregion
    }
}
