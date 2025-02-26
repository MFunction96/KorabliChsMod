using System.IO;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Config;

namespace Xanadu.Test.KorabliChsMod.Core
{
    [TestClass]
    public class KorabliFileHubTest
    {
        [TestMethod]
        public void Load()
        {
            IKorabliFileHub.ConfigFilePath = Path.Combine("assets", "config_test.json");
            var networkEngine = new NetworkEngine();
            var expected = new KorabliFileHub(networkEngine)
            {
                Mirror = MirrorList.Cloudflare,
                AllowPreRelease = true,
                AutoUpdate = true,
                GameFolder = "C:\\Test",
                Proxy = new ProxyConfig
                {
                    Enabled = true,
                    Address = "http://192.168.144.254:23333",
                    Username = "User",
                    Password = "Password"
                }
            };

            var actual = new KorabliFileHub(networkEngine);
            actual.Load();
            Assert.IsTrue(actual.ConfigEquals(expected));
        }

        public void UpdateEngineProxy(bool dry = false)
        {

        }


        public async Task SaveAsync()
        {
            //IKorabliFileHub.ConfigFilePath = Path.Combine("assets", "config_test.json");
            //var networkEngine = new NetworkEngine();
            //var expected = new KorabliFileHub(networkEngine)
            //{
            //    Mirror = MirrorList.Cloudflare,
            //    AllowPreRelease = true,
            //    AutoUpdate = true,
            //    GameFolder = "C:\\Test",
            //    Proxy = new ProxyConfig
            //    {
            //        Enabled = true,
            //        Address = "http://192.168.144.254:23333",
            //        Username = "User",
            //        Password = "Password"
            //    }
            //};

            //var actual = new KorabliFileHub(networkEngine);
            //actual.Load();

            //await actual.SaveAsync();
            //Assert.IsTrue(actual.ConfigEquals(expected));
        }
    }
}
