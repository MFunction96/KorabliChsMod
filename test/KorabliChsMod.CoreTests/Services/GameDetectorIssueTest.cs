using Newtonsoft.Json;
using System.IO;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.KorabliChsMod.Core.Services;

namespace Xanadu.Test.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 针对实例的游戏探查服务进行测试
    /// </summary>
    [TestClass]
    public class GameDetectorIssueTest
    {
        /// <summary>
        /// 针对实例的游戏探查服务进行测试
        /// </summary>
        [TestMethod]
        public void GameDetectorTest()
        {
            var issueFolders = Directory.GetDirectories(Path.Combine("Services", "GameDetector"));
            foreach (var folder in issueFolders)
            {
                var expected = JsonConvert.DeserializeObject<GameDetectModel>(File.ReadAllText(Path.Combine(folder, "Expected.json")))!;
                var gameDetector = new GameDetectorService();
                var actual = gameDetector.Load(folder);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
