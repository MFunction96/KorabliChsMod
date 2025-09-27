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
        [TestMethod]
        [DataRow("8601080_0", "13.6.0.0.8601080")]
        [DataRow("8796161_0", "13.11.0.8796161")]
        [DataRow("8824884_0", "25.10.0.8824884")]
        [DataRow("8824884_1", "25.10.0.8824884")]
        [DataRow("8824884_2", "25.10.0.8824884")]
        [DataRow("8824884_3", "25.10.0.8824884")]
        [DataRow("8824884_4", "25.9.0.8821884")]
        public void VersionDetectTest(string subFolder, string expected)
        {
            var gameDetector = new GameDetectorService();
            var model = gameDetector.Load(Path.Combine("assets", "GameDetectorService", subFolder));
            Assert.IsNotNull(model);
            Assert.AreEqual(expected, model.GameVersion);
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
                Folder = Path.Combine("assets", "GameDetectorService", subFolder),
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
                Folder = Path.Combine("assets", "GameDetectorService", subFolder),
                ClientVersion = version,
                ServerVersion = version
            };

            Assert.AreEqual(expected, GameDetectorService.ChsModPackCheck(gameDetectModel));
        }
    }
}