using System.IO;
using System.Net.Http;

namespace Xanadu.KorabliChsMod.Core
{
    public static class HttpMessageExtensions
    {
        public static HttpRequestMessage Clone(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content.Clone(),
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

        public static HttpContent? Clone(this HttpContent? content)
        {
            if (content == null) return null;

            var ms = new MemoryStream();
            content.CopyToAsync(ms).Wait();
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
