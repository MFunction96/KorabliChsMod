using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 网络引擎
    /// </summary>
    public sealed class NetworkEngine : INetworkEngine
    {
        /// <summary>
        /// 是否析构标记
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// HTTP客户端
        /// </summary>
        private HttpClient Client { get; set; } = new();

        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <inheritdoc />
        public ConcurrentDictionary<string, string> Headers { get; } = new();

        /// <inheritdoc />
        public bool Init()
        {
            try
            {
                this.Headers.Clear();
                this.Headers.TryAdd("Accept", "application/vnd.github+json");
                this.Headers.TryAdd("X-GitHub-Api-Version", "2022-11-28");
                this.Headers.TryAdd("User-Agent", "C#/.NET 8.0");
                return true;

            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg { Exception = e });
                return false;
            }
        }

        /// <inheritdoc />
        public bool SetProxy(Uri? uri, string username = "", string password = "", bool dry = false)
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
                        handler.Proxy = new WebProxy(uri, true, null, new NetworkCredential(username, password));
                    }

                    var client = new HttpClient(handler, true)
                    {
                        DefaultRequestVersion = Version.Parse("2.0"),
                        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                    };

                    if (!dry)
                    {
                        this.Client = client;
                    }

                    return true;
                }

            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg { Exception = e });
                return false;
            }

        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, int retry = 0, CancellationToken cancellationToken = default)
        {
            foreach (var header in this.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            if (retry == 0)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = $"开始单次请求：{request.RequestUri?.Host}"
                });

                var res = await this.Client.SendAsync(request, cancellationToken);

                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = $"{res.StatusCode} -> {request.RequestUri?.Host}"
                });

                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = $"结束单次请求：{res.StatusCode} {request.RequestUri?.Host}"
                });

                return res;
            }

            HttpResponseMessage? response = null;
            while (retry-- > 0)
            {
                try
                {
                    var req = request.Clone();
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"开始请求：{req.RequestUri?.Host}"
                    });

                    response = await this.Client.SendAsync(req, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"{response.StatusCode} -> {req.RequestUri?.Host}"
                    });

                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"结束请求：{response.StatusCode} {req.RequestUri?.Host}"
                    });

                    return response;
                }
                catch (Exception e)
                {
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"请求失败，剩余尝试 {retry} 次数。{response?.StatusCode} {request.RequestUri?.Host}",
                        Exception = e
                    });
                }

                Thread.Sleep(1000);
            }

            this.ServiceEvent?.Invoke(this, new ServiceEventArg
            {
                Exception = new HttpRequestException($"请求失败 {response?.StatusCode} {request.RequestUri?.Host}"),
            });

            return null;
        }

        /// <inheritdoc />
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
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = $"请求失败，剩余尝试 {retry} 次数。{response?.StatusCode} {request.RequestUri?.Host}",
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
            // UpdateAvailable to see if Dispose has already been called.
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
