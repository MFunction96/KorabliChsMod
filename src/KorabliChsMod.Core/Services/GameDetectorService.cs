using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.Skidbladnir.Core.Extension;

namespace Xanadu.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 游戏探查服务，Transient 生命周期
    /// </summary>
    public sealed class GameDetectorService : IServiceEvent
    {
        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <summary>
        /// 加载游戏信息
        /// </summary>
        /// <param name="gameFolder">游戏文件夹</param>
        /// <returns>游戏探查模型</returns>
        public GameDetectModel? Load(string gameFolder)
        {
            try
            {
                var gameDetectModel = new GameDetectModel
                {
                    Folder = gameFolder
                };

                if (!File.Exists(gameDetectModel.GameInfoXmlPath))
                {
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Exception = new FileNotFoundException("WOWS游戏信息文件不存在，请核对所选文件夹")
                    });

                    return null;
                }

                var gameInfoXml = XDocument.Load(gameDetectModel.GameInfoXmlPath);
                var gameId = gameInfoXml.Root?.Element("game")?.Element("id")?.Value;
                gameDetectModel.IsWarship = string.Compare(gameId, "MK.RU.PRODUCTION", StringComparison.OrdinalIgnoreCase) == 0
                                            || string.Compare(gameId, "MK.RPT.PRODUCTION", StringComparison.OrdinalIgnoreCase) == 0;
                gameDetectModel.Server = gameInfoXml.Root?.Element("game")?.Element("localization")?.Value ?? string.Empty;
                gameDetectModel.ClientVersion = gameInfoXml.Root?.Element("game")?.Element("part_versions")?.Element("version")?.Attribute("installed")?.Value ?? string.Empty;
                gameDetectModel.PreInstalled = gameInfoXml.Root?.Element("game")?.Element("accepted_preinstalls") is not null;
                gameDetectModel.IsTest = string.Compare(gameId, "MK.RU.PRODUCTION", StringComparison.OrdinalIgnoreCase) != 0;
                if (File.Exists(gameDetectModel.PreferencesXmlPath))
                {
                    var preferenceLines = File.ReadLines(gameDetectModel.PreferencesXmlPath, StaticVault.Utf8WithoutBomEncoding);
                    var serverVersion = preferenceLines.FirstOrDefault(q => q.Contains("last_server_version", StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(serverVersion))
                    {
                        gameDetectModel.ServerVersion = serverVersion.Replace("<last_server_version>", string.Empty).Replace("</last_server_version>", string.Empty).Trim('\t').Trim().Replace(",", ".");
                    }
                }

                gameDetectModel.ChsMod = GameDetectorService.PathXmlCheck(gameDetectModel) && GameDetectorService.ChsModPackCheck(gameDetectModel);

                if (!Directory.Exists(gameDetectModel.ModFolder))
                {
                    Directory.CreateDirectory(gameDetectModel.ModFolder);
                }

                return gameDetectModel;
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Exception = e
                });

                return null;
            }
        }

        /// <summary>
        /// 检查paths.xml中是否已经添加mods路径
        /// </summary>
        /// <param name="gameDetectModel">游戏检测模块</param>
        /// <returns>paths.xml中的mods路径状态</returns>
        public static bool PathXmlCheck(GameDetectModel gameDetectModel)
        {
            if (!gameDetectModel.IsTest)
            {
                return true;
            }

            if (!File.Exists(gameDetectModel.PathXmlPath))
            {
                return false;
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(gameDetectModel.PathXmlPath);
            var paths = xmlDocument["root"]?["Paths"]?.ChildNodes;
            return paths is not null && string.Compare(paths.Cast<XmlNode>().FirstOrDefault()?.Attributes?["type"]?.Value, "mods", StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// 检查汉化模组是否安装
        /// </summary>
        /// <param name="gameDetectModel">游戏检测模块</param>
        /// <returns>汉化模组安装状态</returns>
        internal static bool ChsModPackCheck(GameDetectModel gameDetectModel)
        {
            return File.Exists(gameDetectModel.ChsModFilePath);
        }
    }
}
