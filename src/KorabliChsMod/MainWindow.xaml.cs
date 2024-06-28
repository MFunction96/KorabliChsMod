using Microsoft.Win32;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
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
        private KorabliConfig Config { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="gameDetector"></param>
        public MainWindow(ILogger logger, IGameDetector gameDetector)
        {
            this._logger = logger;
            this._gameDetector = gameDetector;
            this.Config = new KorabliConfig();
            this.Config.Read();
            InitializeComponent();
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

            this.TbStatus.Text += $"考拉比汉社厂 v{fullVersion}";
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
    }
}