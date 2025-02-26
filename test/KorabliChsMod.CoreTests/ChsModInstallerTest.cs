using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
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

        private static readonly Mutex TestMutex = new();

        private static void RunWithMutex(Action action)
        {
            ChsModInstallerTest.TestMutex.WaitOne();
            try
            {
                action();
            }
            finally
            {
                ChsModInstallerTest.TestMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public void GithubInstallTest()
        {
            ChsModInstallerTest.RunWithMutex(() =>
            {
                const MirrorList mirror = MirrorList.Github;
                var tempPath = Path.GetFullPath(Path.GetRandomFileName());
                Directory.CreateDirectory(tempPath);
                IOExtension.CopyDirectory(Path.Combine(Environment.CurrentDirectory, "assets"), tempPath);
                var gameDetector = new GameDetector();
                gameDetector.Load(tempPath);
                var networkEngine = new NetworkEngine();
                networkEngine.Init();
                networkEngine.SetProxy(new Uri("http://192.168.144.254:23333"));
                var metadataFetcher = new MetadataFetcher(networkEngine);
                var fileCachePool = new FileCachePool(this._mockFileCachePoolLogger.Object);
                var modInstaller = new ChsModInstaller(networkEngine, fileCachePool, gameDetector, metadataFetcher);
                var installTask = modInstaller.Install(mirror);
                installTask.Wait();
                foreach (var filePath in IChsModInstaller.RelativeFilePath)
                {
                    Assert.IsTrue(filePath.IsDirectory
                        ? Directory.Exists(Path.Combine(gameDetector.ModFolder, filePath.FilePath))
                        : File.Exists(Path.Combine(gameDetector.ModFolder, filePath.FilePath)));
                }
                IOExtension.DeleteDirectory(tempPath, force: true);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public void CdnInstallTest()
        {
            ChsModInstallerTest.RunWithMutex(() =>
            {
                const MirrorList mirror = MirrorList.Cloudflare;
                var tempPath = Path.GetFullPath(Path.GetRandomFileName());
                Directory.CreateDirectory(tempPath);
                IOExtension.CopyDirectory(Path.Combine(Environment.CurrentDirectory, "assets"), tempPath);
                var gameDetector = new GameDetector();
                gameDetector.Load(tempPath);
                var networkEngine = new NetworkEngine();
                networkEngine.Init();
                var metadataFetcher = new MetadataFetcher(networkEngine);
                var fileCachePool = new FileCachePool(this._mockFileCachePoolLogger.Object);
                var modInstaller = new ChsModInstaller(networkEngine, fileCachePool, gameDetector, metadataFetcher);
                var installTask = modInstaller.Install(mirror);
                installTask.Wait();
                foreach (var filePath in IChsModInstaller.RelativeFilePath)
                {
                    Assert.IsTrue(filePath.IsDirectory
                        ? Directory.Exists(Path.Combine(gameDetector.ModFolder, filePath.FilePath))
                        : File.Exists(Path.Combine(gameDetector.ModFolder, filePath.FilePath)));
                }
                IOExtension.DeleteDirectory(tempPath, force: true);
            });
        }
    }
}
