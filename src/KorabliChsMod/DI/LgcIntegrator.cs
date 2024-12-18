using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Xml;

namespace Xanadu.KorabliChsMod.DI
{
    public class LgcIntegrator(ILogger<LgcIntegrator> logger) : ILgcIntegrator
    {
        public string? Folder { get; private set; }

        public string? PreferencesXmlPath => string.IsNullOrEmpty(this.Folder) ? null : Path.Combine(this.Folder, ILgcIntegrator.PreferencesXmlFileName);

        public ICollection<string> GameFolders { get; } = new List<string>();

        public void Load()
        {
            try
            {
                var openSubKey = Registry.CurrentUser.OpenSubKey(ILgcIntegrator.RegistrySubKey, RegistryRights.QueryValues);
                if (openSubKey is null)
                {
                    logger.LogWarning("Lesta Game Center not found in registry");
                    return;
                }

                var value = openSubKey.GetValue("DisplayIcon") as string;
                openSubKey.Close();
                if (value is null)
                {
                    logger.LogWarning("Lesta Game Center not found in registry");
                    return;
                }

                this.Folder = Path.GetDirectoryName(value[..value.IndexOf(',')].Trim('\"').Trim());
                if (string.IsNullOrEmpty(this.PreferencesXmlPath))
                {
                    logger.LogWarning("Lesta Game Center folder not found");
                    return;
                }

                var preferencesXml = new XmlDocument();
                preferencesXml.Load(this.PreferencesXmlPath);
                var games = preferencesXml.SelectNodes("/protocol/application/games_manager/games/game");
                if (games is null)
                {
                    throw new KeyNotFoundException("找不到游戏路径");
                }

                foreach (XmlNode game in games)
                {
                    var gameFolder = game["working_dir"]?.InnerText;
                    if (string.IsNullOrEmpty(gameFolder))
                    {
                        continue;
                    }

                    this.GameFolders.Add(gameFolder);
                }

                logger.LogInformation("Lesta Game Center folder found!");
            }
            catch (Exception e)
            {
                logger.LogError(e, string.Empty);
            }

        }
    }
}
