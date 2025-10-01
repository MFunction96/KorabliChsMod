using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Extensions;

namespace Xanadu.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 网络引擎，Transient 生命周期
    /// </summary>
    public sealed class NetworkEngine(HttpClient httpClient)
    {
        /// <summary>
        /// 
        /// </summary>
        public static event EventHandler<ServiceEventArg>? ServiceEvent;

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
                NetworkEngine.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = $"开始单次请求：{request.RequestUri?.Host}"
                });

                var res = await httpClient.SendAsync(request, cancellationToken);

                NetworkEngine.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = $"{res.StatusCode} -> {request.RequestUri?.Host}"
                });

                NetworkEngine.ServiceEvent?.Invoke(this, new ServiceEventArg
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
                    NetworkEngine.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"开始请求：{req.RequestUri?.Host}"
                    });

                    response = await httpClient.SendAsync(req, cancellationToken);
                    _ = response.EnsureSuccessStatusCode();
                    NetworkEngine.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"{response.StatusCode} -> {req.RequestUri?.Host}"
                    });

                    NetworkEngine.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"结束请求：{response.StatusCode} {req.RequestUri?.Host}"
                    });

                    return response;
                }
                catch (Exception e)
                {
                    NetworkEngine.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"请求失败，剩余尝试 {retry} 次数。{response?.StatusCode} {request.RequestUri?.Host}",
                        Exception = e
                    });
                }

                await Task.Delay(1000, cancellationToken);
            }

            NetworkEngine.ServiceEvent?.Invoke(this, new ServiceEventArg
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
                await response.Content.CopyToAsync(fsb, cancellationToken);
            }
            catch (Exception e)
            {
                NetworkEngine.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = $"请求失败，剩余尝试 {retry} 次数。{response?.StatusCode} {request.RequestUri?.Host}",
                    Exception = e
                });

                throw;
            }

        }
    }
}
