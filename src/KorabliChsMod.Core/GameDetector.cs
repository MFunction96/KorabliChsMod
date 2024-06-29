using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Xanadu.KorabliChsMod.Core
{
    /// <inheritdoc />
    public sealed class GameDetector : IGameDetector
    {
        /// <inheritdoc />
        public string Folder { get; private set; } = string.Empty;

        /// <inheritdoc />
        public string GameInfoXmlPath => Path.Combine(this.Folder, IGameDetector.GameInfoXmlFileName);

        /// <inheritdoc />
        public string ModFolder => this.IsTest ? throw new NotImplementedException(): Path.Combine(this.Folder, "bin", this.BuildNumber, "res_mods");

        /// <inheritdoc />
        public string LocaleInfoXmlPath => Path.Combine(this.ModFolder, IGameDetector.LocaleInfoXmlFileName);

        /// <inheritdoc />
        public string Server { get; private set; } = string.Empty;

        /// <inheritdoc />
        public string Version { get; private set; } = string.Empty;

        /// <inheritdoc />
        public string BuildNumber => this.Version[(this.Version.LastIndexOf('.') + 1)..];

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
                this.IsTest = false;
                this.Folder = gameFolder;
                var gameInfoXml = new XmlDocument();
                gameInfoXml.Load(this.GameInfoXmlPath);
                this.Server = gameInfoXml["protocol"]?["game"]?["localization"]?.InnerText ?? string.Empty;
                this.Version = gameInfoXml["protocol"]?["game"]?["part_versions"]?["version"]?.Attributes["installed"]?.Value ?? string.Empty;
                var localeXml = new XmlDocument();
                localeXml.Load(this.LocaleInfoXmlPath);
                this.Locale = localeXml["locale_config"]?["lang_mapping"]?["lang"]?.Attributes["full"]?.Value ?? string.Empty;
                this.ChsMod = string.Compare(this.Locale, "schinese", StringComparison.OrdinalIgnoreCase) == 0;
            }, cancellationToken);
        }
    }
}
