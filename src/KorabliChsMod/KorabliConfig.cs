using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace Xanadu.KorabliChsMod
{
    public class KorabliConfig
    {
        public ProxyConfig Proxy { get; set; }

        public string GameFolder { get; set; }

        public void Read()
        {
            var config = File.Exists("config.json") ? JsonConvert.DeserializeObject<KorabliConfig>(File.ReadAllText("config.json", Encoding.UTF8)) : new KorabliConfig();
            this.Proxy = config.Proxy;
            this.GameFolder = config.GameFolder;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText("config.json", json, Encoding.UTF8);
        }
    }
}
