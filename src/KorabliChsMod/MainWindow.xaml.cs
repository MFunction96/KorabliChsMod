using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Xanadu.KorabliChsMod.Config;
using Xanadu.KorabliChsMod.Core;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly ILogger<MainWindow> _logger;

        /// <summary>
        /// 
        /// </summary>
        private readonly IGameDetector _gameDetector;

        /// <summary>
        /// 
        /// </summary>
        private readonly INetworkEngine _networkEngine;

        /// <summary>
        /// 
        /// </summary>
        private readonly BackgroundWorker _worker;

        /// <summary>
        /// 
        /// </summary>
        private KorabliConfig Config { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KorabliChsMod");

        private readonly CachePool _cachePool;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="gameDetector"></param>
        /// <param name="networkEngine"></param>
        public MainWindow(ILogger<MainWindow> logger, IGameDetector gameDetector, INetworkEngine networkEngine)
        {
            this._logger = logger;
            this._gameDetector = gameDetector;
            this._networkEngine = networkEngine;
            this._networkEngine.NetworkEngineEvent += this.SyncNetworkEngineMessage;
            this._cachePool = new CachePool(MainWindow.AppDataPath, true, logger);
            // TODO: Github与Gitee切换
            _ = this._networkEngine.Headers.TryAdd("Accept", "application/vnd.github+json");
            _ = this._networkEngine.Headers.TryAdd("X-GitHub-Api-Version", "2022-11-28");
            _ = this._networkEngine.Headers.TryAdd("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0");

            this.Config = new KorabliConfig();
            this.Config.Read();
            InitializeComponent();
            this._worker = new BackgroundWorker();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnGameFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Multiselect = false,
                Title = "选择窝窝屎本体位置"
            };

            var result = dialog.ShowDialog();

            if (!(result ?? false))
            {
                return;
            }

            var gameFolder = dialog.FolderName;
            this.Config.GameFolder = gameFolder;
            try
            {
                await this._gameDetector.Load(this.Config.GameFolder);
                this.TbGameFolder.Text = this._gameDetector.Folder;
                this.LbGameServerDetail.Content = this._gameDetector.Server;
                this.LbGameVersionDetail.Content = this._gameDetector.Version;
                this.LbGameChsVersionDetail.Content = this._gameDetector.ChsMod ? "已安装" : "未安装";
                await this.Config.SaveAsync();
                this.BtnInstall.IsEnabled = true;
                this.BtnUninstall.IsEnabled = true;
            }
            catch (Exception exception)
            {
                this.TbStatus.Text += exception.Message + "\r\n";
                this._logger.LogError(exception, string.Empty);
            }

        }

        private async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            this.BtnInstall.IsEnabled = false;
            this.BtnUninstall.IsEnabled = false;

            var zipFile = this._cachePool.Register("Korabli_localization_chs.zip", "download");
            var downloadFolder = Path.Combine(this._cachePool.BasePath, "download");
            if (!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
            }

            try
            {
                if (!Directory.Exists(KorabliConfig.BackupFolder))
                {
                    Directory.CreateDirectory(KorabliConfig.BackupFolder);
                }

                var queue = new Queue<string>(Directory.GetDirectories(KorabliConfig.BackupFolder)
                    .OrderBy(q => ulong.Parse(Path.GetFileName(q))));

                while (queue.Count > 2)
                {
                    Directory.Delete(queue.Dequeue(), true);
                }

                var now = (ulong)DateTime.Now.ToBinary();
                var backupFolder = Path.Combine(KorabliConfig.BackupFolder, now.ToString());
                Directory.CreateDirectory(backupFolder);
                File.Copy(this._gameDetector.LocaleInfoXmlPath, Path.Combine(backupFolder, IGameDetector.LocaleInfoXmlFileName), true);

                var response = await this._networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                    "https://api.github.com/repos/DDFantasyV/Korabli_localization_chs/releases"), 5);
                if (response is null || !response.IsSuccessStatusCode)
                {
                    return;
                }

                var releases = await response.Content.ReadAsStringAsync();
                var jArray = JsonConvert.DeserializeObject<JArray>(releases) ?? [];
                var latest = jArray.First(q => !q["prerelease"]!.Value<bool>());
                var modVersion = latest["tag_name"]!.Value<string>();
                var downloadFile = latest["zipball_url"]!.Value<string>();
                var fileResponse =
                    await this._networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get, downloadFile), 5);
                if (fileResponse is null || !fileResponse.IsSuccessStatusCode)
                {
                    return;
                }

                await using var fs = new FileStream(zipFile, FileMode.Create, FileAccess.Write);
                await using var fsb = new BufferedStream(fs);
                await using var nsb = new BufferedStream(await fileResponse.Content.ReadAsStreamAsync());
                await nsb.CopyToAsync(fsb);
                fsb.Close();
                fs.Close();
                using var zip = ZipFile.OpenRead(zipFile);
                var entry = zip.Entries[0].FullName;
                ZipFile.ExtractToDirectory(zipFile, downloadFolder, Encoding.UTF8, true);
                MainWindow.CopyDirectory(Path.Combine(downloadFolder, entry), this._gameDetector.ModFolder, true);
                await File.WriteAllTextAsync(Path.Combine(this._gameDetector.ModFolder, "Korabli_localization_chs.sig"),
                    modVersion, Encoding.UTF8);
                
                await this._gameDetector.Load(this.Config.GameFolder);
                this.TbGameFolder.Text = this._gameDetector.Folder;
                this.LbGameServerDetail.Content = this._gameDetector.Server;
                this.LbGameVersionDetail.Content = this._gameDetector.Version;
                this.LbGameChsVersionDetail.Content = this._gameDetector.ChsMod ? "已安装" : "未安装";
                await this.Config.SaveAsync();
                this.BtnInstall.IsEnabled = true;
                this.BtnUninstall.IsEnabled = true;

                MessageBox.Show("汉化完成！");
            }
            catch (Exception exception)
            {
                this.TbStatus.Text += exception.Message + "\r\n";
                this._logger.LogError(exception, string.Empty);
            }
            this.BtnInstall.IsEnabled = true;
            this.BtnUninstall.IsEnabled = true;
        }

        private async void BtnUninstall_Click(object sender, RoutedEventArgs e)
        {
            this.BtnInstall.IsEnabled = false;
            this.BtnUninstall.IsEnabled = false;
            try
            {
                var queue = new Queue<string>(Directory.GetDirectories(KorabliConfig.BackupFolder)
                    .OrderByDescending(q => ulong.Parse(Path.GetFileName(q))));

                var backupFile = Path.Combine(KorabliConfig.BackupFolder, queue.Peek(),
                    IGameDetector.LocaleInfoXmlFileName);
                File.Copy(backupFile, this._gameDetector.LocaleInfoXmlPath, true);

                await this._gameDetector.Load(this.Config.GameFolder);
                this.TbGameFolder.Text = this._gameDetector.Folder;
                this.LbGameServerDetail.Content = this._gameDetector.Server;
                this.LbGameVersionDetail.Content = this._gameDetector.Version;
                this.LbGameChsVersionDetail.Content = this._gameDetector.ChsMod ? "已安装" : "未安装";
                await this.Config.SaveAsync();
                this.BtnInstall.IsEnabled = true;
                this.BtnUninstall.IsEnabled = true;

                MessageBox.Show("还原完成！");
            }
            catch (Exception exception)
            {
                this.TbStatus.Text += exception.Message + "\r\n";
                this._logger.LogError(exception, string.Empty);
            }
            this.BtnInstall.IsEnabled = true;
            this.BtnUninstall.IsEnabled = true;
        }

        private void HlDdf_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(hyperlink!.NavigateUri.AbsoluteUri) { UseShellExecute = true });
        }

        private void HlMf_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(hyperlink!.NavigateUri.AbsoluteUri) { UseShellExecute = true });
        }

        private void HlNg_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(hyperlink!.NavigateUri.AbsoluteUri) { UseShellExecute = true });
        }

        private void HlWalks_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(hyperlink!.NavigateUri.AbsoluteUri) { UseShellExecute = true });
        }

        private void HlMod_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(hyperlink!.NavigateUri.AbsoluteUri) { UseShellExecute = true });
        }

        private void HlProject_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(hyperlink!.NavigateUri.AbsoluteUri) { UseShellExecute = true });
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            var fullVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion!;
            this.LbVersion.Content = fullVersion.Split('+')[0];
            if (!Directory.Exists(MainWindow.AppDataPath))
            {
                Directory.CreateDirectory(MainWindow.AppDataPath);
            }

            if (Directory.Exists(this.Config.GameFolder))
            {
                try
                {
                    await this._gameDetector.Load(this.Config.GameFolder);
                    this.TbGameFolder.Text = this._gameDetector.Folder;
                    this.LbGameServerDetail.Content = this._gameDetector.Server;
                    this.LbGameVersionDetail.Content = this._gameDetector.Version;
                    this.LbGameChsVersionDetail.Content = this._gameDetector.ChsMod ? "已安装" : "未安装";
                    this.BtnInstall.IsEnabled = true;
                    this.BtnUninstall.IsEnabled = true;
                }
                catch (Exception exception)
                {
                    this.TbStatus.Text += exception.Message + "\r\n";
                    this._logger.LogError(exception, string.Empty);
                }

            }

            this.TbStatus.Text += $"考拉比汉社厂 v{fullVersion}\r\n";
            this._logger.LogInformation($"考拉比汉社厂 v{fullVersion}");
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            this.Config.Proxy = new ProxyConfig
            {
                Address = this.TbProxyAddress.Text,
                Username = this.TbProxyUsername.Text,
                Password = this.TbProxyPassword.Text
            };

            await this.Config.SaveAsync();
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            this.BtnUpdate.IsEnabled = false;
            var downloadFolder = Path.Combine(this._cachePool.BasePath, "download");
            if (!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
            }

            var zipFile = this._cachePool.Register("KorabliChsMod.zip", "download");
            try
            {
                var response = await this._networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                    "https://api.github.com/repos/MFunction96/KorabliChsMod/releases"), 5);
                if (response is null || !response.IsSuccessStatusCode)
                {
                    return;
                }

                var releases = await response.Content.ReadAsStringAsync();
                var jArray = JsonConvert.DeserializeObject<JArray>(releases) ?? [];
                var latest = jArray.First(q => !q["prerelease"]!.Value<bool>());
                var assets = latest["assets"]! as JArray;
                var downloadFile = assets!.First(q =>
                    string.Compare(q["content_type"]!.Value<string>(), "application/zip", StringComparison.OrdinalIgnoreCase) == 0)["browser_download_url"]!.Value<string>();
                var fileResponse =
                    await this._networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get, downloadFile), 5);
                if (fileResponse is null || !fileResponse.IsSuccessStatusCode)
                {
                    return;
                }

                await using var fs = new FileStream(zipFile, FileMode.Create, FileAccess.Write);
                await using var fsb = new BufferedStream(fs);
                await using var nsb = new BufferedStream(await fileResponse.Content.ReadAsStreamAsync());
                await nsb.CopyToAsync(fsb);

                var processInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                    Arguments =
                        $"-ExecutionPolicy Unrestricted -File {Environment.CurrentDirectory}\\Update.ps1 -Id {Environment.ProcessId} -ZipPath {zipFile} -InstallPath {Environment.CurrentDirectory}",
                    WorkingDirectory = Environment.CurrentDirectory,
                    CreateNoWindow = true
                };

                Process.Start(processInfo);
            }
            catch (Exception exception)
            {
                this.TbStatus.Text += exception.Message + "\r\n";
                this.SvStatus.ScrollToBottom();
                this._cachePool.UnRegister(new CacheFile(this._cachePool, zipFile, "download"));
            }

            this.BtnUpdate.IsEnabled = true;
        }

        private void SyncNetworkEngineMessage(object? sender, NetworkEngineEventArg e)
        {
            this.TbStatus.Text += e.Message + "\r\n";
            this.SvStatus.ScrollToBottom();
        }

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}