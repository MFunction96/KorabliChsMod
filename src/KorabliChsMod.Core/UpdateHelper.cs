using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Config;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 更新助手实现
    /// </summary>
    /// <param name="korabliFileHub">考拉比配置中心</param>
    /// <param name="networkEngine">网络引擎</param>
    /// <param name="fileCachePool">缓存服务</param>
    /// <param name="metadataFetcher">元信息获取器</param>
    public class UpdateHelper(
        IKorabliFileHub korabliFileHub,
        INetworkEngine networkEngine,
        IFileCachePool fileCachePool,
        IMetadataFetcher metadataFetcher)
        : IUpdateHelper
    {
        /// <summary>
        /// 镜像元信息
        /// </summary>
        private readonly Dictionary<MirrorList, JToken> _latestJToken = [];

        /// <inheritdoc />
        public Version? LatestVersion { get; private set; }

        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <inheritdoc />
        public async Task<bool> Check(MirrorList mirrorList, Version appVersion)
        {
            try
            {
                var jToken = await metadataFetcher.GetLatestJToken(mirrorList, korabliFileHub.AllowPreRelease);
                if (jToken is null)
                {
                    return false;
                }

                this._latestJToken[mirrorList] = jToken;
                var name = this._latestJToken[mirrorList]["name"]!.Value<string>()!;
                var version = name[name.IndexOf(" ", StringComparison.OrdinalIgnoreCase)..].Trim();
                this.LatestVersion = Version.Parse(version);
                var result = this.LatestVersion > appVersion;
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = result ? $"发现新版本{this.LatestVersion}，请点击关于-更新按钮进行更新\r\n" : "已是最新版本\r\n"
                });

                return this.LatestVersion > appVersion;
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg { Exception = e });
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Update(MirrorList mirrorList)
        {
            var downloadFolder = Path.Combine(fileCachePool.BasePath, "download");
            if (!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
            }

            var exeFile = Path.Combine(fileCachePool.BasePath, "download", "KorabliChsModInstaller.exe");
            try
            {
                var latest = this._latestJToken.TryGetValue(mirrorList, out var jToken)
                    ? jToken
                    : await metadataFetcher.GetLatestJToken(mirrorList, korabliFileHub.AllowPreRelease);
                var assets = latest!["assets"]! as JArray;
                var downloadFile = assets!.First(q =>
                    string.Compare(q["name"]!.Value<string>(), "KorabliChsModInstaller.exe",
                        StringComparison.OrdinalIgnoreCase) == 0)["browser_download_url"]!.Value<string>();
                await networkEngine.DownloadAsync(new HttpRequestMessage(HttpMethod.Get, downloadFile),
                    exeFile, 5);

                var processInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                    Arguments =
                        $"-ExecutionPolicy Unrestricted -Command \"Stop-Process -Id {Environment.ProcessId} -Force ; $p = Start-Process -FilePath \'{exeFile}\' -ArgumentList \'/S /D={Environment.CurrentDirectory}\' -PassThru ; $p.WaitForExit() ; Start-Process -FilePath \'{Environment.CurrentDirectory}\\KorabliChsMod.exe\'\"",
                    WorkingDirectory = Environment.CurrentDirectory,
                    CreateNoWindow = true
                };

                Process.Start(processInfo);
                return true;
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg { Exception = e });
                return false;
            }
        }

    }
}
