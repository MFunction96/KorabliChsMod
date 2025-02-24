using Newtonsoft.Json;
using System.IO;
using Xanadu.KorabliChsMod.Core;

namespace Xanadu.Test.KorabliChsMod.Core
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class GameDetectorIssueTest
    {
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void GameDetectorTest()
        {
            var issueFolders = Directory.GetDirectories("GameDetector");
            foreach (var folder in issueFolders)
            {
                var expected = File.ReadAllText(Path.Combine(folder, "Expected.json"));
                var gameDetector = new GameDetector();
                var result = gameDetector.Load(folder);
                var actual = JsonConvert.SerializeObject(new { Result = result, Detector = gameDetector }, Formatting.Indented);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
