using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xanadu.KorabliChsMod.Core.Config;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod.Core
{
    public class UpdateHelper(ILogger<UpdateHelper> logger, IKorabliFileHub korabliFileHub, INetworkEngine networkEngine, ICachePool cachePool) : IUpdateHelper
    {
        private JToken? _latestJToken;
        public MirrorList Mirror { get; set; } = MirrorList.Github;
        public Version? LatestVersion { get; private set; }
        public async Task<Version> Check()
        {
            try
            {
                this._latestJToken = await this.GetLatestJToken();
                var name = this._latestJToken["name"]!.Value<string>()!;
                var version = name[name.IndexOf(" ", StringComparison.OrdinalIgnoreCase)..].Trim();
                this.LatestVersion = Version.Parse(version);
                return this.LatestVersion;
            }
            catch (Exception e)
            {
                logger.LogError(e, string.Empty);
                throw;
            }
        }

        public async Task Update()
        {
            var downloadFolder = Path.Combine(cachePool.BasePath, "download");
            if (!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
            }

            var exeFile = cachePool.Register("KorabliChsModInstaller.exe", "download");
            try
            {
                var latest = this._latestJToken ?? await this.GetLatestJToken();
                var assets = latest["assets"]! as JArray;
                var downloadFile = assets!.First(q =>
                    string.Compare(q["name"]!.Value<string>(), "KorabliChsModInstaller.exe",
                        StringComparison.OrdinalIgnoreCase) == 0)["browser_download_url"]!.Value<string>();
                await networkEngine.DownloadAsync(new HttpRequestMessage(HttpMethod.Get, downloadFile),
                    exeFile.FullPath, 5);

                var processInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                    Arguments =
                        $"-ExecutionPolicy Unrestricted -Command \"Stop-Process -Id {Environment.ProcessId} -Force ; $p = Start-Process -FilePath \'{exeFile.FullPath}\' -ArgumentList \'/S /D={Environment.CurrentDirectory}\' -PassThru ; $p.WaitForExit() ; Start-Process -FilePath \'{Environment.CurrentDirectory}\\KorabliChsMod.exe\'\"",
                    WorkingDirectory = Environment.CurrentDirectory,
                    CreateNoWindow = true
                };

                logger.LogInformation($"\"{processInfo.FileName}\" {processInfo.Arguments}");
                Process.Start(processInfo);
            }
            catch (Exception e)
            {
                logger.LogError(e, string.Empty);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<JToken> GetLatestJToken()
        {
            var response = await networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                "https://api.github.com/repos/MFunction96/KorabliChsMod/releases"), 5);
            _ = response!.EnsureSuccessStatusCode();
            var releases = await response.Content.ReadAsStringAsync();
            var jArray = JsonConvert.DeserializeObject<JArray>(releases) ?? [];
            return korabliFileHub.AllowPreRelease ? jArray.First() : jArray.First(q => !q["prerelease"]!.Value<bool>());
        }
    }
}
