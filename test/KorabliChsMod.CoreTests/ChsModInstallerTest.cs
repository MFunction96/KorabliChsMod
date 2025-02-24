using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core;
using Xanadu.Skidbladnir.IO.File;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.Test.KorabliChsMod.Core
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class ChsModInstallerTest
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Mock<ILogger<FileCachePool>> _mockFileCachePoolLogger = new();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GithubInstallTest()
        {
            var mirror = MirrorList.Github;
            var tempPath = Path.GetFullPath(Path.GetRandomFileName());
            Directory.CreateDirectory(tempPath);
            IOExtension.CopyDirectory(Path.Combine(Environment.CurrentDirectory, "assets"), tempPath);
            var gameDetector = new GameDetector();
            gameDetector.Load(tempPath);
            var networkEngine = new NetworkEngine();
            networkEngine.Init(mirror);
            var fileCachePool = new FileCachePool(this._mockFileCachePoolLogger.Object);
            var modInstaller = new ChsModInstaller(networkEngine, fileCachePool, gameDetector);
            await modInstaller.Install(mirror);
            Assert.IsTrue(Directory.Exists(Path.Combine(gameDetector.ModFolder, "texts")));
            Assert.IsTrue(File.Exists(Path.Combine(gameDetector.ModFolder, "locale_config.xml")));
            Assert.IsTrue(File.Exists(Path.Combine(gameDetector.ModFolder, "LICENSE")));
            await IOExtension.DeleteDirectory(tempPath, force: true);
        }
    }
}
