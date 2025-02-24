using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 网络引擎
    /// </summary>
    public interface INetworkEngine : IServiceEvent, IDisposable
    {
        /// <summary>
        /// HTTP消息头
        /// </summary>
        public ConcurrentDictionary<string, string> Headers { get; }

        /// <summary>
        /// 初始化网络引擎
        /// </summary>
        public bool Init();

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="uri">地址</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="dry">干运行</param>
        /// <returns>成功则返回true，失败则返回false</returns>
        public bool SetProxy(Uri? uri, string username = "", string password = "", bool dry = false);

        /// <summary>
        /// 发送HTTP请求
        /// </summary>
        /// <param name="request">HTTP请求</param>
        /// <param name="retry">重试次数</param>
        /// <param name="cancellationToken">中止信号</param>
        /// <returns>HTTP响应</returns>
        public Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, int retry = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="request">HTTP请求</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="retry">重试次数</param>
        /// <param name="cancellationToken">中止信号</param>
        public Task DownloadAsync(HttpRequestMessage request, string filePath, int retry = 0,
            CancellationToken cancellationToken = default);
    }
}
