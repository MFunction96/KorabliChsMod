using System.IO;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core;

namespace Xanadu.Test.KorabliChsMod.Core
{
    [TestClass]
    public class IssueTest
    {
        [TestMethod]
        public void GameDetectorTest()
        {
            var issueFolders = Directory.GetDirectories("Issue");
            foreach (var folder in issueFolders)
            {
                var gameDetector = new GameDetector();
                gameDetector.Load(folder);
            }
        }
    }
}
