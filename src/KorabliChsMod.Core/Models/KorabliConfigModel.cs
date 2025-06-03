using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Xanadu.Skidbladnir.IO.File;
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

namespace Xanadu.KorabliChsMod.Core.Models
{
    /// <summary>
    /// 考拉比配置中心
    /// </summary>
    public class KorabliConfigModel
    {
        /// <summary>
        /// 
        /// </summary>
        private static string? _testFolder;

        /// <summary>
        /// 
        /// </summary>
        public static string BaseFolder => string.IsNullOrEmpty(KorabliConfigModel._testFolder) ? IOExtension.AppDataFolder : KorabliConfigModel._testFolder;

        /// <summary>
        /// 功能未开放提示
        /// </summary>
        [JsonIgnore]
        public const string DeprecatedHint = "该功能暂未开放";

        /// <summary>
        /// 日志文件名
        /// </summary>
        [JsonIgnore]
        public const string LogFileName = "Korabli.log";

        /// <summary>
        /// 配置文件名
        /// </summary>
        [JsonIgnore]
        public const string ConfigFileName = "config.json";

        /// <summary>
        /// 已安装文件名
        /// </summary>
        [JsonIgnore]
        public const string InstalledFileName = "installed.json";

        /// <summary>
        /// 配置文件路径
        /// </summary>
        [JsonIgnore]
        public static string ConfigFilePath => Path.Combine(KorabliConfigModel.BaseFolder, KorabliConfigModel.ConfigFileName);

        /// <summary>
        /// 日志文件路径
        /// </summary>
        [JsonIgnore]
        public static string LogFilePath => Path.Combine(KorabliConfigModel.BaseFolder, KorabliConfigModel.LogFileName);

        /// <summary>
        /// 已安装文件路径
        /// </summary>
        [JsonIgnore]
        public static string InstalledFilePath => Path.Combine(KorabliConfigModel.BaseFolder, KorabliConfigModel.InstalledFileName);

        /// <summary>
        /// 链接字典
        /// </summary>
        public static readonly IDictionary<MirrorList, ResourceLinkModel> Links =
            new Dictionary<MirrorList, ResourceLinkModel>
            {
                { MirrorList.Github , new ResourceLinkModel
                {
                    Mirror = MirrorList.Github,
                    ModMetadata = "https://api.github.com/repos/DDFantasyV/Korabli_localization_chs/releases",
                    UpdateMetadata = "https://api.github.com/repos/MFunction96/KorabliChsMod/releases"
                }},
                { MirrorList.Cloudflare , new ResourceLinkModel
                {
                    Mirror = MirrorList.Cloudflare,
                    ModMetadata = "https://warshipmod.mfbrain.xyz/mods/chs/metadata.json",
                    UpdateMetadata = "https://warshipmod.mfbrain.xyz/korablichsmod/metadata.json"
                }}
            };

        /// <summary>
        /// 配置文件版本
        /// </summary>
        public Version Version { get; set; } = new(0, 0, 0);

        /// <summary>
        /// 代理设置
        /// </summary>
        public ProxyConfigModel Proxy { get; set; } = new();

        /// <summary>
        /// 已记录的游戏路径
        /// </summary>
        public string GameFolder { get; set; } = string.Empty;

        /// <summary>
        /// 镜像站
        /// </summary>
        public MirrorList Mirror { get; set; } = MirrorList.Cloudflare;

        /// <summary>
        /// 自动更新
        /// </summary>
        public bool AutoUpdate { get; set; }

        /// <inheritdoc />>
        public override bool Equals(object? other)
        {
            if (other is not KorabliConfigModel otherModel)
            {
                return false;
            }

            return this.Mirror == otherModel.Mirror &&
                   this.Proxy.Equals(otherModel.Proxy) &&
                   this.AutoUpdate == otherModel.AutoUpdate &&
                   string.Equals(this.GameFolder, otherModel.GameFolder, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 测试文件夹设置
        /// </summary>
        /// <param name="testFolder">测试文件夹</param>
        internal static void SetTestFolder(string testFolder)
        {
            KorabliConfigModel._testFolder = testFolder;
        }
    }
}
