﻿using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Config;
using Xanadu.Skidbladnir.IO.File;
using Xanadu.Skidbladnir.IO.File.Cache;
// ReSharper disable RedundantExtendsListEntry

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
        private readonly ICachePool _cachePool;

        /// <summary>
        /// 
        /// </summary>
        private readonly IKorabliFileHub _korabliFileHub;

        /// <summary>
        /// 
        /// </summary>
        private static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KorabliChsMod");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="gameDetector"></param>
        /// <param name="networkEngine"></param>
        /// <param name="cachePool"></param>
        /// <param name="korabliFileHub"></param>
        public MainWindow(ILogger<MainWindow> logger, IGameDetector gameDetector, INetworkEngine networkEngine, ICachePool cachePool, IKorabliFileHub korabliFileHub)
        {
            this._logger = logger;
            this._gameDetector = gameDetector;
            this._networkEngine = networkEngine;
            this._networkEngine.NetworkEngineEvent += this.SyncNetworkEngineMessage;
            this._cachePool = cachePool;
            this._korabliFileHub = korabliFileHub;
            // TODO: Github与Gitee切换
            _ = this._networkEngine.Headers.TryAdd("Accept", "application/vnd.github+json");
            _ = this._networkEngine.Headers.TryAdd("X-GitHub-Api-Version", "2022-11-28");
            _ = this._networkEngine.Headers.TryAdd("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0");
            this._korabliFileHub.Load();
            InitializeComponent();
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            var fullVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion!;
            this.LbVersion.Content = fullVersion.Split('+')[0];
            if (!Directory.Exists(MainWindow.AppDataPath))
            {
                Directory.CreateDirectory(MainWindow.AppDataPath);
            }

            this.TbStatus.Text += $"考拉比汉社厂 v{fullVersion}\r\n";
            this._logger.LogInformation($"考拉比汉社厂 v{fullVersion}");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.TbProxyAddress.Text = this._korabliFileHub.Proxy.Address;
            this.ReloadFolder();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGameFolder_Click(object sender, RoutedEventArgs e)
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
            this._korabliFileHub.GameFolder = gameFolder;
            try
            {
                this.ReloadFolder();
            }
            catch (Exception exception)
            {
                this.WriteErrorToStatus(exception);
            }

        }

        private async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            this.BtnInstall.IsEnabled = false;
            this.BtnUninstall.IsEnabled = false;

            var zipFile = this._cachePool.Register("Korabli_localization_chs.zip", "download");
            try
            {

                var backupFolder = await this._korabliFileHub.EnqueueBackup(true);
                IOExtension.CopyDirectory(this._gameDetector.ModFolder, backupFolder);
                var response = await this._networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                    "https://api.github.com/repos/DDFantasyV/Korabli_localization_chs/releases"), 5);
                if (response is null || !response.IsSuccessStatusCode)
                {
                    return;
                }

                var releases = await response.Content.ReadAsStringAsync();
                var jArray = JsonConvert.DeserializeObject<JArray>(releases) ?? [];
                var latest = jArray.First(q => q["prerelease"]!.Value<bool>() == this._gameDetector.IsTest);
                var modVersion = latest["tag_name"]!.Value<string>();
                var downloadFile = latest["zipball_url"]!.Value<string>();
                await this._networkEngine.DownloadAsync(new HttpRequestMessage(HttpMethod.Get, downloadFile),
                    zipFile.FullPath, 5);

                using var zip = ZipFile.OpenRead(zipFile.FullPath);
                var entry = zip.Entries[0].FullName;
                var zipFolder = Path.Combine(zipFile.Pool.BasePath, entry);
                ZipFile.ExtractToDirectory(zipFile.FullPath, zipFile.Pool.BasePath, Encoding.UTF8, true);
                IOExtension.CopyDirectory(zipFolder, this._gameDetector.ModFolder);
                await File.WriteAllTextAsync(Path.Combine(this._gameDetector.ModFolder, "Korabli_localization_chs.ver"),
                    modVersion, Encoding.UTF8);

                this.ReloadFolder();

                MessageBox.Show("汉化完成！");
            }
            catch (Exception exception)
            {
                this.WriteErrorToStatus(exception);
            }

            this._cachePool.UnRegister(zipFile);
            this.BtnInstall.IsEnabled = true;
            this.BtnUninstall.IsEnabled = true;
        }

        private void BtnUninstall_Click(object sender, RoutedEventArgs e)
        {
            this.BtnInstall.IsEnabled = false;
            this.BtnUninstall.IsEnabled = false;
            try
            {
                var backupFolder = this._korabliFileHub.PeekLatestBackup();
                IOExtension.CopyDirectory(backupFolder, this._gameDetector.ModFolder);
                this.ReloadFolder();
                MessageBox.Show("还原完成！");
            }
            catch (Exception exception)
            {
                this.WriteErrorToStatus(exception);
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

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            this._korabliFileHub.Proxy = new ProxyConfig
            {
                Address = this.TbProxyAddress.Text,
                Username = this.TbProxyUsername.Text,
                Password = this.TbProxyPassword.Text
            };

            await this._korabliFileHub.SaveAsync();
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            this.BtnUpdate.IsEnabled = false;
            var downloadFolder = Path.Combine(this._cachePool.BasePath, "download");
            if (!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
            }

            var exeFile = this._cachePool.Register("KorabliChsModInstaller.exe", "download");
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
                    string.Compare(q["name"]!.Value<string>(), "KorabliChsModInstaller.exe", StringComparison.OrdinalIgnoreCase) == 0)["browser_download_url"]!.Value<string>();
                await this._networkEngine.DownloadAsync(new HttpRequestMessage(HttpMethod.Get, downloadFile),
                    exeFile.FullPath, 5);

                var processInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                    Arguments =
                        $"-ExecutionPolicy Unrestricted -Command \"Stop-Process -Id {Environment.ProcessId} -Force ; $p = Start-Process -FilePath \'{exeFile.FullPath}\' -ArgumentList \'/S /D={Path.GetDirectoryName(Environment.CurrentDirectory)}\' -PassThru ; $p.WaitForExit() ; Start-Process -FilePath \'{Environment.CurrentDirectory}\\KorabliChsMod.exe\'\"",
                    WorkingDirectory = Environment.CurrentDirectory,
                    CreateNoWindow = true
                };

                this._logger.LogInformation($"\"{processInfo.FileName}\" {processInfo.Arguments}");
                Process.Start(processInfo);
            }
            catch (Exception exception)
            {
                this.WriteErrorToStatus(exception);
            }

            this._cachePool.UnRegister(exeFile);
            this.BtnUpdate.IsEnabled = true;
        }

        private void SyncNetworkEngineMessage(object? sender, NetworkEngineEventArg e)
        {
            this.TbStatus.Text += e.Message + "\r\n";
            this.SvStatus.ScrollToBottom();
        }



        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void ReloadFolder()
        {
            this.BtnInstall.IsEnabled = false;
            this.BtnUninstall.IsEnabled = false;
            try
            {
                if (!string.IsNullOrEmpty(this._korabliFileHub.GameFolder))
                {
                    await this._gameDetector.Load(this._korabliFileHub.GameFolder);
                }

                await this._korabliFileHub.SaveAsync();

                if (string.IsNullOrEmpty(this._korabliFileHub.GameFolder))
                {
                    return;
                }

            }
            catch (Exception exception)
            {
                this.WriteErrorToStatus(exception);
                return;
            }

            this.TbGameFolder.Text = this._gameDetector.Folder;
            this.LbGameServerDetail.Content = this._gameDetector.Server;
            this.LbGameVersionDetail.Content = this._gameDetector.PreInstalled ? this._gameDetector.ServerVersion : this._gameDetector.ClientVersion;
            this.LbGameChsVersionDetail.Content = this._gameDetector.ChsMod ? "已安装" : "未安装";
            this.LbGameTestDetail.Content = this._gameDetector.IsTest ? "测试服" : "正式服";
            this.BtnInstall.IsEnabled = true;
            this.BtnUninstall.IsEnabled = true;
        }

        private void WriteErrorToStatus(Exception exception, bool autoScroll = true)
        {
            this._logger.LogError(exception, string.Empty);
            this.TbStatus.Text += exception.Message + "\r\n";
            if (autoScroll)
            {
                this.SvStatus.ScrollToBottom();
            }
        }
    }
}