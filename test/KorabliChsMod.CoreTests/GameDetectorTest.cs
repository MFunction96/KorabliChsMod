using System;
using System.IO;
using Xanadu.KorabliChsMod.Core;

namespace Xanadu.Test.KorabliChsMod.Core
{
    [TestClass]
    public class GameDetectorTest
    {
        [TestMethod]
        public void VersionDetectTest()
        {
            var gameDetector = new GameDetector();
            gameDetector.Load(Path.Combine(Environment.CurrentDirectory, "assets"));
            Assert.AreEqual("13.6.0.0.8601080", gameDetector.ClientVersion);
        }
    }
}