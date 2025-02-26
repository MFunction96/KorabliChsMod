using System;
using System.IO;
using System.Threading;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Config;

namespace Xanadu.Test.KorabliChsMod.Core
{
    [TestClass]
    public class KorabliFileHubTest
    {
        private static readonly Mutex TestMutex = new();

        private static void RunWithMutex(Action action)
        {
            KorabliFileHubTest.TestMutex.WaitOne();
            try
            {
                action();
            }
            finally
            {
                KorabliFileHubTest.TestMutex.ReleaseMutex();
            }
        }

        [TestMethod]
        public void Load()
        {
            KorabliFileHubTest.RunWithMutex(() =>
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
            });

            KorabliFileHubTest.RunWithMutex(() =>
            {
                IKorabliFileHub.ConfigFilePath = Path.Combine("assets", "config_test_empty.json");
                var networkEngine = new NetworkEngine();
                var expected = new KorabliFileHub(networkEngine);
                var actual = new KorabliFileHub(networkEngine);
                actual.Load();
                Assert.IsTrue(actual.ConfigEquals(expected));
            });

        }

        [TestMethod]
        public void UpdateEngineProxy()
        {
            KorabliFileHubTest.RunWithMutex(() =>
            {
                IKorabliFileHub.ConfigFilePath = Path.Combine("assets", "config_test.json");
                var networkEngine = new NetworkEngine();
                var actual = new KorabliFileHub(networkEngine);
                actual.Load();
                Assert.IsTrue(actual.UpdateEngineProxy());
            });
        }

        [TestMethod]
        public void SaveAsync()
        {
            KorabliFileHubTest.RunWithMutex(() =>
            {
                IKorabliFileHub.ConfigFilePath = Path.Combine("assets", "config_test_save.json");
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

                var fileHub = new KorabliFileHub(networkEngine);
                fileHub.Load();
                fileHub.Mirror = expected.Mirror;
                fileHub.AllowPreRelease = expected.AllowPreRelease;
                fileHub.AutoUpdate = expected.AutoUpdate;
                fileHub.GameFolder = expected.GameFolder;
                fileHub.Proxy = expected.Proxy;
                var task = fileHub.SaveAsync();
                task.Wait();
                var actual = new KorabliFileHub(networkEngine);
                actual.Load();
                Assert.IsTrue(actual.ConfigEquals(expected));
            });
        }
    }
}
