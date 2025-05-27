using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Extensions;

namespace Xanadu.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 网络引擎
    /// </summary>
    public sealed class NetworkEngine : IServiceEvent, IDisposable
    {
        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <summary>
        /// 是否析构标记
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// HTTP客户端
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 网络引擎构造函数
        /// </summary>
        /// <param name="korabliConfigService">考拉比配置服务</param>
        public NetworkEngine(KorabliConfigService korabliConfigService)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.All
            };

            try
            {
                if (korabliConfigService.CurrentConfig.Proxy.Enabled &&
                    !string.IsNullOrEmpty(korabliConfigService.CurrentConfig.Proxy.Address))
                {
                    handler.Proxy = new WebProxy(korabliConfigService.CurrentConfig.Proxy.Address,
                        true, null,
                        new NetworkCredential(korabliConfigService.CurrentConfig.Proxy.Username,
                            korabliConfigService.CurrentConfig.Proxy.Password));
                }
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Exception = e,
                    Message = "代理设置错误，已禁用代理。"
                });

                throw;
            }

            this._httpClient = new HttpClient(handler, true)
            {
                DefaultRequestVersion = Version.Parse("2.0"),
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
                DefaultRequestHeaders =
                {
                    { "Accept", "application/vnd.github+json" },
                    { "X-GitHub-Api-Version", "2022-11-28" },
                    { "User-Agent", "C#/.NET 8.0 KorabliChsMod" }
                },
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// 发送HTTP请求
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <param name="retry">重试次数</param>
        /// <param name="cancellationToken">取消句柄</param>
        /// <returns>HTTP响应</returns>
        public async Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, int retry = 0, CancellationToken cancellationToken = default)
        {
            if (retry == 0)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = $"开始单次请求：{request.RequestUri?.Host}"
                });

                var res = await this._httpClient.SendAsync(request, cancellationToken);

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
                    var req = await request.CloneAsync();
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"开始请求：{req.RequestUri?.Host}"
                    });

                    response = await this._httpClient.SendAsync(req, cancellationToken);
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

                await Task.Delay(1000, cancellationToken);
            }

            this.ServiceEvent?.Invoke(this, new ServiceEventArg
            {
                Exception = new HttpRequestException($"请求失败 {response?.StatusCode} {request.RequestUri?.Host}"),
            });

            return null;
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="request">HTTP请求</param>
        /// <param name="filePath">下载路径</param>
        /// <param name="retry">重试次数</param>
        /// <param name="cancellationToken">取消句柄</param>
        /// <returns>下载任务</returns>
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
                this._httpClient.Dispose();

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
