using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 汉化安装实现
    /// </summary>
    /// <param name="networkEngine">网络引擎</param>
    /// <param name="fileCachePool">文件缓存</param>
    /// <param name="gameDetector">游戏探查</param>
    /// <param name="metadataFetcher">元信息获取</param>
    public class ChsModInstaller(INetworkEngine networkEngine, IFileCachePool fileCachePool, IGameDetector gameDetector, IMetadataFetcher metadataFetcher) : IChsModInstaller
    {
        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <inheritdoc />
        public async Task<bool> Install(MirrorList mirror, CancellationToken cancellationToken = default)
        {
            var zipFile = fileCachePool.Register("Korabli_localization_chs.zip", "download");
            var zipTempFolder = Path.Combine(zipFile.Pool.BasePath, Path.GetRandomFileName());
            try
            {
                var latest = await metadataFetcher.GetLatestJToken(mirror, true, gameDetector.IsTest, true) ?? throw new DataException("获取元信息失败！请检查网络连接。");
                var downloadFile = latest["zipball_url"]!.Value<string>();
                await networkEngine.DownloadAsync(new HttpRequestMessage(HttpMethod.Get, downloadFile),
                    zipFile.FullPath, 5, cancellationToken);

                using var zip = ZipFile.OpenRead(zipFile.FullPath);
                var extractFolder = Path.Combine(zipTempFolder, zip.Entries[0].FullName.Split('/')[0]);
                ZipFile.ExtractToDirectory(zipFile.FullPath, zipTempFolder, Encoding.UTF8, true);
                IOExtension.CopyDirectory(Path.Combine(extractFolder, "texts"),
                    Path.Combine(gameDetector.ModFolder, "texts"));
                File.Copy(Path.Combine(extractFolder, "locale_config.xml"),
                    Path.Combine(gameDetector.ModFolder, "locale_config.xml"), true);
                File.Copy(Path.Combine(extractFolder, "LICENSE"), Path.Combine(gameDetector.ModFolder, "LICENSE"),
                    true);
                return true;
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Exception = e
                });

                return false;
            }
            finally
            {
                zipFile.Dispose();
                IOExtension.DeleteDirectory(zipTempFolder);
            }
        }

    }
}
