using System;
using System.IO;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.KorabliChsMod.Core.Services;

namespace Xanadu.Test.KorabliChsMod.Core
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class GameDetectorServiceTest
    {
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void VersionDetectTest()
        {
            var gameDetector = new GameDetectorService();
            var gameDetectModel = new GameDetectModel();
            gameDetector.Load(Path.Combine(Environment.CurrentDirectory, "assets"));
            Assert.AreEqual("13.6.0.0.8601080", gameDetectModel.ClientVersion);
        }
    }
}