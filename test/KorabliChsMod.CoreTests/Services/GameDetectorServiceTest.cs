using System;
using System.IO;
using Xanadu.KorabliChsMod.Core.Models;
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

        [TestMethod]
        [DataRow("8824884_0", "25.10.0.0.8824884", false)]
        [DataRow("8824884_1", "25.10.0.0.8824884", true)]
        [DataRow("8824884_2", "25.10.0.0.8824884", false)]
        [DataRow("8824884_3", "25.10.0.0.8824884", true)]
        public void PathXmlCheck(string subFolder, string version, bool expected)
        {
            var gameDetectModel = new GameDetectModel
            {
                Folder = Path.Combine(Environment.CurrentDirectory, "assets", subFolder),
                ClientVersion = version,
                ServerVersion = version
            };

            Assert.AreEqual(expected, GameDetectorService.PathXmlCheck(gameDetectModel));
        }

        [TestMethod]
        [DataRow("8824884_0", "25.10.0.0.8824884", true)]
        [DataRow("8824884_1", "25.10.0.0.8824884", false)]
        [DataRow("8824884_2", "25.10.0.0.8824884", false)]
        [DataRow("8824884_3", "25.10.0.0.8824884", true)]
        public void ChsModPackCheck(string subFolder, string version, bool expected)
        {
            var gameDetectModel = new GameDetectModel
            {
                Folder = Path.Combine(Environment.CurrentDirectory, "assets", subFolder),
                ClientVersion = version,
                ServerVersion = version
            };

            Assert.AreEqual(expected, GameDetectorService.ChsModPackCheck(gameDetectModel));
        }
    }
}