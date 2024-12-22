﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    public sealed class NetworkEngine : INetworkEngine
    {
        private bool _disposed;

        private bool _init;

        private HttpClient Client { get; set; } = new();

        public event EventHandler<ServiceEventArg>? ServiceEvent;

        public ConcurrentDictionary<string, string> Headers { get; } = new();

        public void Init()
        {
            if (this._init)
            {
                return;
            }

            this.Headers.TryAdd("Accept", "application/vnd.github+json");
            this.Headers.TryAdd("X-GitHub-Api-Version", "2022-11-28");
            this.Headers.TryAdd("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0");
            this._init = true;
        }

        public bool SetProxy(Uri? uri, string username = "", string password = "")
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
                this.ServiceEvent?.Invoke(this, new ServiceEventArg { Exception = e });
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
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"开始请求：{request.RequestUri?.Host}"
                    });

                    response = await this.Client.SendAsync(request, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"{response.StatusCode} -> {request.RequestUri?.Host}"
                    });

                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"结束请求：{response.StatusCode} {request.RequestUri?.Host}"
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
