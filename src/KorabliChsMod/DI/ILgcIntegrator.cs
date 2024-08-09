using System.Collections.Generic;
using System.IO;

namespace Xanadu.KorabliChsMod.DI
{
    public interface ILgcIntegrator
    {
        public const string RegistrySubKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Lesta Game Center";

        public const string PreferencesXmlFileName = "preferences.xml";

        public string? Folder { get; }

        public string? PreferencesXmlPath { get; }

        public ICollection<string> GameFolders { get; }

        public void Load();
    }
}
