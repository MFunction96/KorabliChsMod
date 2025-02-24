using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File;

namespace Xanadu.KorabliChsMod.Core.Config
{
    /// <summary>
    /// 考拉比汉社厂配置中心
    /// </summary>
    public interface IKorabliFileHub : IServiceEvent
    {
        /// <summary>
        /// 链接字典
        /// </summary>
        public static Dictionary<MirrorList, ResourceLink> Links =
            new()
            {
                { MirrorList.Github , new ResourceLink
                {
                    Mirror = MirrorList.Github,
                    ModMetadata = "https://api.github.com/repos/DDFantasyV/Korabli_localization_chs/releases",
                    UpdateMetadata = "https://api.github.com/repos/MFunction96/KorabliChsMod/releases"
                }},
                { MirrorList.Cloudflare , new ResourceLink
                {
                    Mirror = MirrorList.Cloudflare,
                    ModMetadata = "https://warshipmod.mfbrain.xyz/mods/chs/metadata.json",
                    UpdateMetadata = "https://warshipmod.mfbrain.xyz/korablichsmod/metadata.json"
                }}
            };

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
        /// 日志文件路径
        /// </summary>
        [JsonIgnore]
        public static string LogFilePath => Path.Combine(IOExtension.AppDataFolder, IKorabliFileHub.LogFileName);

        /// <summary>
        /// 配置文件名
        /// </summary>
        [JsonIgnore]
        public const string ConfigFileName = "config.json";

        /// <summary>
        /// 配置文件路径
        /// </summary>
        [JsonIgnore]
        public static string ConfigFilePath => Path.Combine(IOExtension.AppDataFolder, IKorabliFileHub.ConfigFileName);

        /// <summary>
        /// 代理设置
        /// </summary>
        public ProxyConfig Proxy { get; set; }

        /// <summary>
        /// 已记录的游戏路径
        /// </summary>
        public string GameFolder { get; set; }

        /// <summary>
        /// 镜像站
        /// </summary>
        public MirrorList Mirror { get; set; }

        /// <summary>
        /// 允许预发布
        /// </summary>
        public bool AllowPreRelease { get; set; }

        /// <summary>
        /// 自动更新
        /// </summary>
        public bool AutoUpdate { get; set; }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void Load();

        /// <summary>
        /// 更新网络组件代理
        /// </summary>
        /// <param name="dry">干执行而非生效</param>
        /// <returns>true为更新成功，false为失败</returns>
        public bool UpdateEngineProxy(bool dry = false);

        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <returns>true为加载成功，false为加载失败</returns>
        public Task<bool> SaveAsync();
    }
}
