using System.IO;

namespace Xanadu.KorabliChsMod.Core.Models
{
    /// <summary>
    /// 游戏检测模型
    /// </summary>
    public class GameDetectModel
    {
        /// <summary>
        /// game_info.xml
        /// </summary>
        public const string GameInfoXmlFileName = "game_info.xml";

        /// <summary>
        /// locale_config.xml
        /// </summary>
        public const string LocaleInfoXmlFileName = "locale_config.xml";

        /// <summary>
        /// metadata.xml
        /// </summary>
        public const string MetaDataXmlFileName = "metadata.xml";

        /// <summary>
        /// preferences.xml
        /// </summary>
        public const string PreferencesXmlFileName = "preferences.xml";

        /// <summary>
        /// 是否为战舰世界
        /// </summary>
        public bool IsWarship { get; set; } = false;

        /// <summary>
        /// 游戏文件夹路径
        /// </summary>
        public string Folder { get; set; } = string.Empty;

        /// <summary>
        /// Mod文件夹路径
        /// </summary>
        public string ModFolder
            => string.IsNullOrEmpty(this.Folder)
                ? string.Empty
                : this.IsTest
                    ? Path.Combine(this.Folder, "bin", this.BuildNumber, "res")
                    : Path.Combine(this.Folder, "bin", this.BuildNumber, "res_mods");

        /// <summary>
        /// game_info.xml路径
        /// </summary>
        public string GameInfoXmlPath => string.IsNullOrEmpty(this.Folder) ? string.Empty : Path.Combine(this.Folder, GameDetectModel.GameInfoXmlFileName);

        /// <summary>
        /// metadata.xml路径
        /// </summary>
        public string MetaDataXmlPath => string.IsNullOrEmpty(this.Folder) ? string.Empty : Path.Combine(this.Folder, "game_metadata", GameDetectModel.MetaDataXmlFileName);

        /// <summary>
        /// preferences.xml路径
        /// </summary>
        public string PreferencesXmlPath => string.IsNullOrEmpty(this.Folder) ? string.Empty : Path.Combine(this.Folder, GameDetectModel.PreferencesXmlFileName);

        /// <summary>
        /// locale_config.xml路径
        /// </summary>
        public string LocaleInfoXmlPath => string.IsNullOrEmpty(this.Folder) ? string.Empty : Path.Combine(this.ModFolder, GameDetectModel.LocaleInfoXmlFileName);

        /// <summary>
        /// 服务器
        /// </summary>
        public string Server { get; set; } = string.Empty;

        /// <summary>
        /// 服务器版本
        /// </summary>
        public string ServerVersion { get; set; } = string.Empty;

        /// <summary>
        /// 客户端版本
        /// </summary>
        public string ClientVersion { get; set; } = string.Empty;

        /// <summary>
        /// 编译版本号
        /// </summary>
        public string BuildNumber
            => string.IsNullOrEmpty(this.Folder)
                ? string.Empty
                : this.PreInstalled && !string.IsNullOrEmpty(this.ServerVersion)
                    ? this.ServerVersion[(this.ServerVersion.LastIndexOf('.') + 1)..]
                    : this.ClientVersion[(this.ClientVersion.LastIndexOf('.') + 1)..];

        /// <summary>
        /// 预安装状态
        /// </summary>
        public bool PreInstalled { get; set; } = false;

        /// <summary>
        /// 是否为测试版本
        /// </summary>
        public bool IsTest { get; set; } = false;

        /// <summary>
        /// 本地化语言
        /// </summary>
        public string Locale { get; set; } = string.Empty;

        /// <summary>
        /// 汉化模组状态
        /// </summary>
        public bool ChsMod { get; set; } = false;
    }
}
