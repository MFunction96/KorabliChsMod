using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core;

namespace Xanadu.KorabliChsModTests
{
    [TestClass]
    public class GameDetectorTest
    {
        private readonly Mock<ILogger<GameDetector>> _mockLogger = new();

        [TestMethod]
        public async Task VersionDetectTest()
        {
            var gameDetector = new GameDetector(this._mockLogger.Object);
            await gameDetector.Load(Environment.CurrentDirectory);

            Assert.AreEqual("13.6.0.0.8601080", gameDetector.Version);
        }
    }
}