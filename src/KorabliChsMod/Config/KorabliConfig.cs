using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class KorabliConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public const string LogFile = "Korabli.log";

        /// <summary>
        /// 
        /// </summary>
        public static string BackupFolder => Path.Combine(Environment.CurrentDirectory, "backup");

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
        public void Read()
        {
            var config = File.Exists("config.json") ? JsonConvert.DeserializeObject<KorabliConfig>(File.ReadAllText("config.json", Encoding.UTF8)) : new KorabliConfig();
            Proxy = config.Proxy;
            GameFolder = config.GameFolder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsync()
        {
            var json = JsonConvert.SerializeObject(this);
            await File.WriteAllTextAsync("config.json", json, Encoding.UTF8);
        }
    }
}
