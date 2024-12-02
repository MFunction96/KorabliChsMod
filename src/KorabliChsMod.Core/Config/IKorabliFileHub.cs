using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File;

namespace Xanadu.KorabliChsMod.Core.Config
{
    public interface IKorabliFileHub
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public const string LogFileName = "Korabli.log";

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public static string LogFilePath => Path.Combine(IOExtension.AppDataFolder, IKorabliFileHub.LogFileName);

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public const string ConfigFileName = "config.json";

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public static string ConfigFilePath => Path.Combine(IOExtension.AppDataFolder, IKorabliFileHub.ConfigFileName);

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public static string BackupFolder => Path.Combine(IOExtension.AppDataFolder, "backup");

        /// <summary>
        /// 
        /// </summary>
        public int Reserve { get; }

        /// <summary>
        /// 
        /// </summary>
        public ProxyConfig Proxy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GameFolder { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MirrorList Mirror { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool AllowPreRelease { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool AutoUpdate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reserve"></param>
        public void Load(int reserve = 2);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ReloadBackupInstance(CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task TrimBackInstance(bool reload = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trim"></param>
        /// <returns></returns>
        public Task<string> EnqueueBackup(bool trim = false);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string PeekLatestBackup();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task SaveAsync();
    }
}
