using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Xml;
using Xanadu.KorabliChsMod.Core;

namespace Xanadu.KorabliChsMod.DI
{
    public class LgcIntegrator : ILgcIntegrator
    {

        public event EventHandler<ServiceEventArg>? ServiceEvent;

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
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Exception = new KeyNotFoundException("未检测到Lesta Game Center")
                    });

                    return;
                }

                var value = openSubKey.GetValue("DisplayIcon") as string;
                openSubKey.Close();
                if (value is null)
                {
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Exception = new KeyNotFoundException("检测到Lesta Game Center，但未能读取Lesta Game Center安装路径")
                    });

                    return;
                }

                this.Folder = Path.GetDirectoryName(value[..value.IndexOf(',')].Trim('\"').Trim());
                if (string.IsNullOrEmpty(this.PreferencesXmlPath))
                {
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Exception = new KeyNotFoundException("检测到Lesta Game Center，但无法访问Lesta Game Center配置文件")
                    });

                    return;
                }

                var preferencesXml = new XmlDocument();
                preferencesXml.Load(this.PreferencesXmlPath);
                var games = preferencesXml.SelectNodes("/protocol/application/games_manager/games/game");
                if (games is null)
                {
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Exception = new KeyNotFoundException("检测到Lesta Game Center，但未检测到已安装游戏")
                    });

                    return;
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

                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = "已成功读取游戏安装目录"
                });
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Exception = e
                });

            }

        }

    }
}
