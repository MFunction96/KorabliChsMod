using System.IO;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core;

namespace Xanadu.Test.KorabliChsMod.Core
{
    [TestClass]
    public class IssueTest
    {
        [TestMethod]
        public async Task GameDetectorTest()
        {
            var issueFolders = Directory.GetDirectories("Issue");
            foreach (var folder in issueFolders)
            {
                var gameDetector = new GameDetector();
                await gameDetector.Load(folder);
            }
        }
    }
}
