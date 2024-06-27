using System;
using System.Windows;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Path = System.IO.Path;

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
        /// <param name="logger"></param>
        public MainWindow(ILogger<MainWindow> logger)
        {
            this._logger = logger;
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGameLocation_Click(object sender, RoutedEventArgs e)
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
            var gameInfoFile = Path.Combine(gameFolder, "game_info.xml");
            try
            {
                var gameInfoXml = new XmlDocument();
                gameInfoXml.Load(gameInfoFile);
                var gameServer = gameInfoXml["protocol"]?["game"]?["localization"]?.InnerText ?? string.Empty;
                this.LbGameServerDetail.Content = gameServer;
                var gameVersion = gameInfoXml["protocol"]?["game"]?["part_versions"]?["version"]?.Attributes["installed"]?.Value ?? string.Empty;
                this.LbGameVersionDetail.Content = gameVersion;
                var buildNumber = gameVersion[(gameVersion.LastIndexOf('.') + 1)..];
                var modFolder = Path.Combine(gameFolder, "bin", buildNumber, "res_mods");
                var localeFile = Path.Combine(modFolder, "locale_config.xml");
                var localeXml = new XmlDocument();
                localeXml.Load(localeFile);
                var language = localeXml["locale_config"]?["lang_mapping"]?["lang"]?.Attributes["full"]?.Value ?? string.Empty;
                var chsModStatus = string.Compare(language, "schinese", StringComparison.OrdinalIgnoreCase) == 0;
                this.LbGameChsVersionDetail.Content = chsModStatus ? "已安装" : "未安装";
            }
            catch (Exception exception)
            {
                _ = MessageBox.Show("该文件夹路径不合法！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                this._logger.LogError(exception, "选择游戏路径异常");
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

        }

        private void HlMf_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HlNg_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HlWalks_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HlMod_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HlProject_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Initialized(object sender, EventArgs e)
        {

        }
    }
}