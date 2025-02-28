using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Config;
using Xanadu.Skidbladnir.Core.Extension;
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
        /// <summary>
        /// 文件校验
        /// </summary>
        private readonly Checksum _checksum = new(SHAAlgorithm.SHA256);

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
                var installedFiles = await ChsModInstaller.LoadInstalledFile();
                using var zip = ZipFile.OpenRead(zipFile.FullPath);
                var extractFolder = Path.Combine(zipTempFolder, zip.Entries[0].FullName.Split('/')[0]);
                ZipFile.ExtractToDirectory(zipFile.FullPath, zipTempFolder, Encoding.UTF8, true);
                foreach (var item in IChsModInstaller.RelativeFilePath)
                {
                    if (item.IsDirectory)
                    {
                        var textFolder = Path.Combine(gameDetector.ModFolder, item.FilePath);
                        IOExtension.CopyDirectory(Path.Combine(extractFolder, item.FilePath), textFolder);
                        var textFolderFiles = Directory.GetFiles(textFolder, "*.*", SearchOption.AllDirectories);
                        foreach (var textFolderFile in textFolderFiles)
                        {
                            installedFiles[textFolderFile] = await this._checksum.GetFileHashAsync(textFolderFile, BinaryFormatting.Base64, cancellationToken);
                        }
                    }
                    else
                    {
                        var file = Path.Combine(gameDetector.ModFolder, item.FilePath);
                        File.Copy(Path.Combine(extractFolder, item.FilePath), file, true);
                        installedFiles[file] = await this._checksum.GetFileHashAsync(file, BinaryFormatting.Base64, cancellationToken);
                    }
                }

                await File.WriteAllTextAsync(IKorabliFileHub.InstalledFilePath, JsonConvert.SerializeObject(installedFiles, Formatting.Indented), cancellationToken);
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

        /// <summary>
        /// 加载已安装文件
        /// </summary>
        /// <returns>已安装文件目录</returns>
        public static async Task<Dictionary<string, string>> LoadInstalledFile()
        {
            if (File.Exists(IKorabliFileHub.InstalledFilePath))
            {
                var json = await File.ReadAllTextAsync(IKorabliFileHub.InstalledFilePath);
                var files = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                return files;
            }
            else
            {
                return new Dictionary<string, string>();
            }

        }
    }

}
