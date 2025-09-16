using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.Skidbladnir.Core.Extension;
using Xanadu.Skidbladnir.IO.File;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 汉化安装实现，Transient 生命周期
    /// </summary>
    /// <param name="networkEngine">网络引擎</param>
    /// <param name="fileCachePool">文件缓存</param>
    /// <param name="metadataService">元信息获取</param>
    public partial class ChsModService(NetworkEngine networkEngine, FileCachePool fileCachePool, MetadataService metadataService) : IServiceEvent
    {
        /// <summary>
        /// 文件相对路径，bool值表示是否为文件夹
        /// </summary>
        private static readonly IEnumerable<(string FilePath, bool IsDirectory)> RelativeFilePath =
        [
            ("texts", true),
            ("locale_config.xml", false),
            ("LICENSE", false),
            ("thanks.md", false),
            ("change.log",false)
        ];
        [GeneratedRegex(@"(?<Version>\d+\.\d+)", RegexOptions.ExplicitCapture)]
        private static partial Regex GameVersionRegex();

        public static Regex VersionRegex => ChsModService.GameVersionRegex();

        /// <summary>
        /// 文件校验
        /// </summary>
        private readonly Checksum _checksum = new(SHAAlgorithm.SHA256);

        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <summary>
        /// 安装汉化模组
        /// </summary>
        /// <param name="gameDetectModel">镜像</param>
        /// <param name="cancellationToken">中止消息</param>
        /// <returns>成功返回true，失败返回false</returns>
        public async Task<bool> Install(GameDetectModel gameDetectModel, CancellationToken cancellationToken = default)
        {
            using var zipFile = fileCachePool.Register("Korabli_localization_chs.zip", "download");
            var extractFolder = Path.Combine(zipFile.Pool.BasePath, Path.GetRandomFileName());
            try
            {
                var gameVersion = ChsModService.VersionRegex.Match(gameDetectModel.GameVersion).Value;
                var latest = await metadataService.GetModRelease(Version.Parse(gameVersion),
                    gameDetectModel.IsTest);
                var downloadFile = latest.Assets.First(q => q.Name == "Korabli_localization_chs.zip").BrowserDownloadUrl;
                await networkEngine.DownloadAsync(new HttpRequestMessage(HttpMethod.Get, downloadFile),
                    zipFile.FullPath, 5, cancellationToken);
                var installedFiles = await LoadInstalledFile();
                using var zip = ZipFile.OpenRead(zipFile.FullPath);
                ZipFile.ExtractToDirectory(zipFile.FullPath, extractFolder, Encoding.UTF8, true);
                foreach (var item in ChsModService.RelativeFilePath)
                {
                    if (item.IsDirectory)
                    {
                        var textFolder = Path.Combine(gameDetectModel.ModFolder, item.FilePath);
                        IOExtension.CopyDirectory(Path.Combine(extractFolder, item.FilePath), textFolder);
                        var textFolderFiles = Directory.GetFiles(textFolder, "*.*", SearchOption.AllDirectories);
                        foreach (var textFolderFile in textFolderFiles)
                        {
                            installedFiles[textFolderFile] = await this._checksum.GetFileHashAsync(textFolderFile, BinaryFormatting.BASE64, cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        var file = Path.Combine(gameDetectModel.ModFolder, item.FilePath);
                        File.Copy(Path.Combine(extractFolder, item.FilePath), file, true);
                        installedFiles[file] = await this._checksum.GetFileHashAsync(file, BinaryFormatting.BASE64, cancellationToken: cancellationToken);
                    }
                }

                await File.WriteAllTextAsync(KorabliConfigModel.InstalledFilePath, JsonSerializer.Serialize(installedFiles), cancellationToken);
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
                IOExtension.DeleteDirectory(extractFolder);
            }
        }

        /// <summary>
        /// 加载已安装文件
        /// </summary>
        /// <returns>已安装文件目录</returns>
        private static async Task<Dictionary<string, string>> LoadInstalledFile()
        {
            if (!File.Exists(KorabliConfigModel.InstalledFilePath))
            {
                return new Dictionary<string, string>();
            }

            var json = await File.ReadAllTextAsync(KorabliConfigModel.InstalledFilePath);
            var files = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            return files;

        }
    }

}
