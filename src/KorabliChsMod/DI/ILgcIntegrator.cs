using System.Collections.Generic;
using Xanadu.KorabliChsMod.Core;

namespace Xanadu.KorabliChsMod.DI
{
    /// <summary>
    /// Lesta Game Center探查器
    /// </summary>
    public interface ILgcIntegrator : IServiceEvent
    {
        /// <summary>
        /// 注册表子健
        /// </summary>
        public const string RegistrySubKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Lesta Game Center";

        /// <summary>
        /// LGC配置文件名
        /// </summary>
        public const string PreferencesXmlFileName = "preferences.xml";

        /// <summary>
        /// LGC所在文件夹
        /// </summary>
        public string? Folder { get; }

        /// <summary>
        /// LGC配置文件路径
        /// </summary>
        public string? PreferencesXmlPath { get; }

        /// <summary>
        /// 游戏路径
        /// </summary>
        public ICollection<string> GameFolders { get; }

        /// <summary>
        /// 加载LGC配置
        /// </summary>
        /// <param name="path">指定LGC配置文件路径</param>
        /// <returns>true为加载成功，false为加载失败</returns>
        public bool Load(string path = "");
    }
}
