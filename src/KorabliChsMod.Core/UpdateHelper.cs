using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Config;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod.Core
{
    public class UpdateHelper(IKorabliFileHub korabliFileHub, INetworkEngine networkEngine, IFileCachePool fileCachePool) : IUpdateHelper
    {
        private JToken? _latestJToken;
        public MirrorList Mirror { get; set; } = MirrorList.Github;
        public Version? LatestVersion { get; private set; }

        public event EventHandler<ServiceEventArg>? ServiceEvent;

        public async Task<bool> UpdateAvailable(Version appVersion)
        {
            try
            {
                this._latestJToken = await this.GetLatestJToken();
                if (this._latestJToken is null)
                {
                    return false;
                }

                var name = this._latestJToken["name"]!.Value<string>()!;
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

        public async Task<bool> Update()
        {
            var downloadFolder = Path.Combine(fileCachePool.BasePath, "download");
            if (!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
            }

            var exeFile = Path.Combine(fileCachePool.BasePath, "download", "KorabliChsModInstaller.exe");
            try
            {
                var latest = this._latestJToken ?? await this.GetLatestJToken();
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<JToken?> GetLatestJToken()
        {
            try
            {
                var response = await networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                    "https://api.github.com/repos/MFunction96/KorabliChsMod/releases"), 5);
                _ = response!.EnsureSuccessStatusCode();
                var releases = await response.Content.ReadAsStringAsync();
                var jArray = JsonConvert.DeserializeObject<JArray>(releases) ?? [];
                return korabliFileHub.AllowPreRelease ? jArray.First() : jArray.First(q => !q["prerelease"]!.Value<bool>());
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = "获取版本信息失败！",
                    Exception = e,
                    AppendException = false
                });

                return null;
            }

        }
    }
}
