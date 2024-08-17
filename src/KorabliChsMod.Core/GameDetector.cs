using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Xanadu.KorabliChsMod.Core
{
    /// <inheritdoc />
    public sealed class GameDetector(ILogger<GameDetector> logger) : IGameDetector
    {
        /// <inheritdoc />
        public string Folder { get; private set; } = string.Empty;

        /// <inheritdoc />
        public string GameInfoXmlPath => Path.Combine(this.Folder, IGameDetector.GameInfoXmlFileName);

        /// <inheritdoc />
        public string MetaDataXmlPath => Path.Combine(this.Folder, "game_metadata", IGameDetector.MetaDataXmlFileName);

        /// <inheritdoc />
        public string PreferencesXmlPath => Path.Combine(this.Folder, IGameDetector.PreferencesXmlFileName);

        /// <inheritdoc />
        public string ModFolder => this.IsTest ? Path.Combine(this.Folder, "bin", this.BuildNumber, "res") : Path.Combine(this.Folder, "bin", this.BuildNumber, "res_mods");

        /// <inheritdoc />
        public string LocaleInfoXmlPath => Path.Combine(this.ModFolder, IGameDetector.LocaleInfoXmlFileName);

        /// <inheritdoc />
        public string Server { get; private set; } = string.Empty;

        /// <inheritdoc />
        public string ServerVersion { get; private set; } = string.Empty;

        /// <inheritdoc />
        public string ClientVersion { get; private set; } = string.Empty;

        /// <inheritdoc />
        public bool PreInstalled { get; private set; }

        /// <inheritdoc />
        public string BuildNumber => this.PreInstalled && !string.IsNullOrEmpty(this.ServerVersion) ? this.ServerVersion[(this.ServerVersion.LastIndexOf('.') + 1)..] : this.ClientVersion[(this.ClientVersion.LastIndexOf('.') + 1)..];

        /// <inheritdoc />
        public bool IsTest { get; private set; }

        /// <inheritdoc />
        public string Locale { get; private set; } = string.Empty;

        /// <inheritdoc />
        public bool ChsMod { get; private set; }

        /// <inheritdoc />
        public Task Load(string gameFolder, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                try
                {
                    this.IsTest = false;
                    this.Folder = gameFolder;
                    var gameInfoXml = new XmlDocument();
                    gameInfoXml.Load(this.GameInfoXmlPath);
                    this.Server = gameInfoXml["protocol"]?["game"]?["localization"]?.InnerText ?? string.Empty;
                    this.ClientVersion = gameInfoXml["protocol"]?["game"]?["part_versions"]?["version"]?.Attributes["installed"]?.Value ?? string.Empty;
                    this.PreInstalled = !(gameInfoXml["protocol"]?["game"]?["accepted_preinstalls"]?.IsEmpty ?? true);
                    if (File.Exists(this.PreferencesXmlPath))
                    {
                        var preferenceLines = File.ReadLines(this.PreferencesXmlPath, Encoding.UTF8);
                        var serverVersion = preferenceLines.FirstOrDefault(q => q.Contains("last_server_version"));
                        if (!string.IsNullOrEmpty(serverVersion))
                        {
                            this.ServerVersion = serverVersion.Replace("<last_server_version>", string.Empty).Replace("</last_server_version>", string.Empty).Trim('\t').Trim().Replace(",", ".");
                        }
                    }

                    var metadataXml = new XmlDocument();
                    metadataXml.Load(this.MetaDataXmlPath);
                    this.IsTest = string.Compare(metadataXml["protocol"]?["predefined_section"]?["app_id"]?.InnerText, "WOWS.RU.PRODUCTION", StringComparison.OrdinalIgnoreCase) != 0;
                    if (!File.Exists(this.LocaleInfoXmlPath))
                    {
                        this.Locale = "RU";
                        this.ChsMod = false;
                    }
                    else
                    {
                        var localeXml = new XmlDocument();
                        localeXml.Load(this.LocaleInfoXmlPath);
                        this.Locale = localeXml["locale_config"]?["lang_mapping"]?["lang"]?.Attributes["full"]?.Value ?? string.Empty;
                        this.ChsMod = string.Compare(this.Locale, "schinese", StringComparison.OrdinalIgnoreCase) == 0;
                    }

                }
                catch (Exception e)
                {
                    logger.LogError(e, string.Empty);
                    throw;
                }

            }, cancellationToken);
        }

        public Task Backup(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Restore(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
