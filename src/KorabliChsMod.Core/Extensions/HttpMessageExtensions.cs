using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core.Extensions
{
    /// <summary>
    /// HTTP消息扩展
    /// </summary>
    public static class HttpMessageExtensions
    {
        /// <summary>
        /// 克隆HTTP请求
        /// </summary>
        /// <param name="request">HTTP请求</param>
        /// <returns>HTTP请求新实例</returns>
        public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = await request.Content.CloneAsync(),
                Version = request.Version
            };
            foreach (var prop in request.Options)
            {
                clone.Options.Set(new HttpRequestOptionsKey<object?>(prop.Key), prop.Value);
            }
            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        /// <summary>
        /// HTTP内容克隆
        /// </summary>
        /// <param name="content">HTTP内容</param>
        /// <returns>HTTP内容新实例</returns>
        public static async Task<HttpContent?> CloneAsync(this HttpContent? content)
        {
            if (content is null)
            {
                return null;
            }

            var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            ms.Position = 0;
            var clone = new StreamContent(ms);
            foreach (var header in content.Headers)
            {
                clone.Headers.Add(header.Key, header.Value);
            }

            return clone;
        }
    }
}
