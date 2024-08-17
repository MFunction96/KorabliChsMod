using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using Xanadu.KorabliChsMod.Core;

namespace Xanadu.Test.KorabliChsMod.Core
{
    [TestClass]
    public class IssueTest
    {
        private readonly Mock<ILogger<GameDetector>> _mockLogger = new();

        [TestMethod]
        public async Task GameDetectorTest()
        {
            var issueFolders = Directory.GetDirectories("Issue");
            foreach (var folder in issueFolders)
            {
                var gameDetector = new GameDetector(this._mockLogger.Object);
                await gameDetector.Load(folder);
            }
        }
    }
}
