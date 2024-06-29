using Microsoft.Win32;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xanadu.KorabliChsMod.Config;
using Xanadu.KorabliChsMod.Core;

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
        private readonly ILogger _logger;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="gameDetector"></param>
        /// <param name="networkEngine"></param>
        public MainWindow(ILogger logger, IGameDetector gameDetector, INetworkEngine networkEngine)
        {
            this._logger = logger;
            this._gameDetector = gameDetector;
            this._networkEngine = networkEngine;
            this._networkEngine.NetworkEngineEvent += this.SyncNetworkEngineMessage;
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
            var dialog = new OpenFolderDialog()
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
            }
            catch (Exception exception)
            {
                this.TbStatus.Text += exception.Message + "\r\n";
                this._logger.Error(exception, string.Empty);
            }

        }

        private void BtnInstall_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnUninstall_Click(object sender, RoutedEventArgs e)
        {

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
                }
                catch (Exception exception)
                {
                    this.TbStatus.Text += exception.Message + "\r\n";
                    this._logger.Error(exception, string.Empty);
                }

            }

            this.TbStatus.Text += $"考拉比汉社厂 v{fullVersion}\r\n";
            this._logger.Information($"考拉比汉社厂 v{fullVersion}");
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
            var response = await this._networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                "https://api.github.com/repos/MFunction96/KorabliChsMod/releases"), 5);
            if (response is null || !response.IsSuccessStatusCode)
            {
                return;
            }

            var releases = await response!.Content.ReadAsStringAsync();
            var jArray = JsonConvert.DeserializeObject<JArray>(releases) ?? [];
            var latest = jArray[0];
            var downloadFile = @"";
            var processInfo = new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                Arguments =
                    $"-ExecutionPolicy Unrestricted -File {Environment.CurrentDirectory}\\Update.ps1 -Id {Environment.ProcessId} -ZipPath {downloadFile} -InstallPath {Environment.CurrentDirectory}",
                WorkingDirectory = Environment.CurrentDirectory,
                CreateNoWindow = true
            };

            Process.Start(processInfo);
        }

        private void SyncNetworkEngineMessage(object? sender, NetworkEngineEventArg e)
        {
            this.TbStatus.Text += e.Message + "\r\n";
            this.SvStatus.ScrollToBottom();
        }
    }
}