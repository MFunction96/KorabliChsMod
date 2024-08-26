using System.Threading;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGameDetector
    {
        /// <summary>
        /// 
        /// </summary>
        public const string GameInfoXmlFileName = "game_info.xml";

        /// <summary>
        /// 
        /// </summary>
        public const string LocaleInfoXmlFileName = "locale_config.xml";

        /// <summary>
        /// 
        /// </summary>
        public const string MetaDataXmlFileName = "metadata.xml";

        /// <summary>
        /// 
        /// </summary>
        public const string PreferencesXmlFileName = "preferences.xml";

        /// <summary>
        /// 
        /// </summary>
        public bool IsWows { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Folder { get; }

        /// <summary>
        /// 
        /// </summary>
        public string MetaDataXmlPath { get; }

        /// <summary>
        /// 
        /// </summary>
        public string GameInfoXmlPath { get; }

        /// <summary>
        /// 
        /// </summary>
        public string PreferencesXmlPath { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ModFolder { get; }

        /// <summary>
        /// 
        /// </summary>
        public string LocaleInfoXmlPath { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public string Server { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ServerVersion { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ClientVersion { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public string BuildNumber { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool PreInstalled { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsTest { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public string Locale { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool ChsMod { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameInfoXmlPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Load(string gameInfoXmlPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        public void Clear();
    }
}
