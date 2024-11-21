using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xanadu.KorabliChsMod.Core;
using Xanadu.Skidbladnir.IO.File;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.Test.KorabliChsMod.Core
{
    [TestClass]
    public class ModInstallerTest
    {
        private readonly Mock<ILogger<GameDetector>> _mockGameDetectorLogger = new();
        private readonly Mock<ILogger<NetworkEngine>> _mockNetworkEngineLogger = new();
        private readonly Mock<ILogger<FileCachePool>> _mockFileCachePoolLogger = new();

        [TestMethod]
        public async Task InstallTest()
        {
            var tempPath = Path.GetFullPath(Path.GetRandomFileName());
            Directory.CreateDirectory(tempPath);
            IOExtension.CopyDirectory(Path.Combine(Environment.CurrentDirectory, "assets"), tempPath);
            var gameDetector = new GameDetector(this._mockGameDetectorLogger.Object);
            await gameDetector.Load(tempPath);
            var networkEngine = new NetworkEngine(this._mockNetworkEngineLogger.Object);
            var fileCachePool = new FileCachePool(this._mockFileCachePoolLogger.Object);
            var modInstaller = new ModInstaller(networkEngine, fileCachePool, gameDetector);
            await modInstaller.Install();
            await IOExtension.DeleteDirectory(tempPath, force: true);
        }
    }
}
