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
        private const string ModFileName = "MK_L10N_CHS.mkmod";

        [GeneratedRegex(@"(?<Version>\d+\.\d+)", RegexOptions.ExplicitCapture)]
        private static partial Regex GameVersionRegex();

        public static Regex VersionRegex => ChsModService.GameVersionRegex();

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
            using var modFileCache = fileCachePool.Register(ChsModService.ModFileName, "download");
            try
            {
                var gameVersion = ChsModService.VersionRegex.Match(gameDetectModel.GameVersion).Value;
                var latest = await metadataService.GetModRelease(Version.Parse(gameVersion),
                    gameDetectModel.IsTest);
                var downloadFile = latest.Assets.First(q => q.Name == ChsModService.ModFileName).BrowserDownloadUrl;
                await networkEngine.DownloadAsync(new HttpRequestMessage(HttpMethod.Get, downloadFile), modFileCache.FullPath, 5, cancellationToken);
                if (!Directory.Exists(gameDetectModel.ModFolder))
                {
                    Directory.CreateDirectory(gameDetectModel.ModFolder);
                }

                File.Copy(modFileCache.FullPath, Path.Combine(gameDetectModel.ModFolder, ChsModService.ModFileName), true);
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
        }
    }

}
