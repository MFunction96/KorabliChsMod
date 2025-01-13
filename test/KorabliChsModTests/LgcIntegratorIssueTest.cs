using Newtonsoft.Json;
using System.IO;
using Xanadu.KorabliChsMod.DI;

namespace Xanadu.Test.KorabliChsMod
{
    [TestClass]
    public class LgcIntegratorIssueTest
    {
        [TestMethod]
        public void GameDetectorTest()
        {
            var issueFolders = Directory.GetDirectories("LgcIntegrator");
            foreach (var folder in issueFolders)
            {
                var expected = File.ReadAllText(Path.Combine(folder, "Expected.json"));
                var lgcIntegrator = new LgcIntegrator();
                var result = lgcIntegrator.Load(folder);
                var actual = JsonConvert.SerializeObject(new { Result = result, Integrator = lgcIntegrator }, Formatting.Indented);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
