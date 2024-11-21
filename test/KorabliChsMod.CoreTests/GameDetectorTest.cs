using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core;

namespace Xanadu.Test.KorabliChsMod.Core
{
    [TestClass]
    public class GameDetectorTest
    {
        private readonly Mock<ILogger<GameDetector>> _mockLogger = new();

        [TestMethod]
        public async Task VersionDetectTest()
        {
            var gameDetector = new GameDetector(this._mockLogger.Object);
            await gameDetector.Load(Path.Combine(Environment.CurrentDirectory, "assets"));
            Assert.AreEqual("13.6.0.0.8601080", gameDetector.ClientVersion);
        }
    }
}