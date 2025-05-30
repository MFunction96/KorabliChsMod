using System;
using System.IO;
using Xanadu.KorabliChsMod.Core.Services;

namespace Xanadu.Test.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class GameDetectorServiceTest
    {
        /// <summary>
        /// 版本检查测试
        /// </summary>
        [TestMethod]
        public void VersionDetectTest()
        {
            var gameDetector = new GameDetectorService();
            var model = gameDetector.Load(Path.Combine(Environment.CurrentDirectory, "assets"));
            Assert.AreEqual("13.6.0.0.8601080", model!.ClientVersion);
        }
    }
}