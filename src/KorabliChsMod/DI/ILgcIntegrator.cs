using System.Collections.Generic;
using Xanadu.KorabliChsMod.Core;

namespace Xanadu.KorabliChsMod.DI
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILgcIntegrator : IServiceEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public const string RegistrySubKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Lesta Game Center";

        /// <summary>
        /// 
        /// </summary>
        public const string PreferencesXmlFileName = "preferences.xml";

        /// <summary>
        /// 
        /// </summary>
        public string? Folder { get; }

        /// <summary>
        /// 
        /// </summary>
        public string? PreferencesXmlPath { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<string> GameFolders { get; }

        /// <summary>
        /// 
        /// </summary>
        public void Load();
    }
}
