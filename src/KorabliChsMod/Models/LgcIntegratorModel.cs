using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xanadu.KorabliChsMod.Core.Models;
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

namespace Xanadu.KorabliChsMod.Models
{
    public class LgcIntegratorModel
    {
        /// <summary>
        /// LGC配置文件名
        /// </summary>
        private const string PreferencesXmlFileName = "preferences.xml";

        /// <summary>
        /// LGC所在文件夹
        /// </summary>
        public required string Folder { get; init; }

        /// <summary>
        /// LGC配置文件路径
        /// </summary>
        public string PreferencesXmlPath => string.IsNullOrEmpty(this.Folder) ? string.Empty : Path.Combine(this.Folder, LgcIntegratorModel.PreferencesXmlFileName);

        /// <summary>
        /// 游戏路径
        /// </summary>
        public ICollection<GameDetectModel> GameDetectModels { get; } = new List<GameDetectModel>();

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is not LgcIntegratorModel other)
            {
                return false;
            }

            return this.Folder == other.Folder &&
                   this.GameDetectModels.SequenceEqual(other.GameDetectModels);
        }
    }
}
