using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Xml;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Services;
using Xanadu.KorabliChsMod.Models;

namespace Xanadu.KorabliChsMod.Services
{
    /// <summary>
    /// Lesta Game Center探查器实现
    /// </summary>
    /// <param name="serviceProvider"></param>
    public class LgcIntegratorService(IServiceProvider serviceProvider) : IServiceEvent
    {
        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <summary>
        /// 注册表子健
        /// </summary>
        private const string RegistrySubKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Lesta Game Center";

        /// <summary>
        /// 加载LGC配置
        /// </summary>
        /// <param name="path">指定LGC配置文件路径</param>
        /// <returns>true为加载成功，false为加载失败</returns>
        public LgcIntegratorModel? Load(string path = "")
        {
            try
            {
                LgcIntegratorModel lgcIntegratorModel;
                if (string.IsNullOrEmpty(path))
                {
                    var openSubKey = Registry.CurrentUser.OpenSubKey(LgcIntegratorService.RegistrySubKey, RegistryRights.QueryValues);
                    if (openSubKey is null)
                    {
                        this.ServiceEvent?.Invoke(this, new ServiceEventArg
                        {
                            Exception = new KeyNotFoundException("未检测到Lesta Game Center")
                        });

                        return null;
                    }

                    if (openSubKey.GetValue("DisplayIcon") is not string value)
                    {
                        this.ServiceEvent?.Invoke(this, new ServiceEventArg
                        {
                            Exception = new KeyNotFoundException("检测到Lesta Game Center，但未能读取Lesta Game Center安装路径")
                        });

                        return null;
                    }

                    openSubKey.Close();

                    lgcIntegratorModel = new LgcIntegratorModel
                    {
                        Folder = Path.GetDirectoryName(value)!
                    };
                }
                else
                {
                    lgcIntegratorModel = new LgcIntegratorModel
                    {
                        Folder = path
                    };
                }

                if (string.IsNullOrEmpty(lgcIntegratorModel.PreferencesXmlPath))
                {
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Exception = new KeyNotFoundException("检测到Lesta Game Center，但无法访问Lesta Game Center配置文件")
                    });

                    return null;
                }

                var preferencesXml = new XmlDocument();
                preferencesXml.Load(lgcIntegratorModel.PreferencesXmlPath);
                var games = preferencesXml.SelectNodes("/protocol/application/games_manager/games/game");
                if (games is null)
                {
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Exception = new KeyNotFoundException("检测到Lesta Game Center，但未检测到已安装游戏")
                    });

                    return lgcIntegratorModel;
                }

                using var scope = serviceProvider.CreateScope();
                var gameDetectorService = scope.ServiceProvider.GetRequiredService<GameDetectorService>();
                foreach (XmlNode game in games)
                {
                    var gameFolder = game["working_dir"]?.InnerText;
                    if (string.IsNullOrEmpty(gameFolder))
                    {
                        continue;
                    }

                    var gameDetectModel = gameDetectorService.Load(gameFolder);
                    if (gameDetectModel is not null)
                    {
                        lgcIntegratorModel.GameDetectModels.Add(gameDetectModel);
                    }
                    
                }

                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Message = "已成功读取游戏安装目录"
                });

                return lgcIntegratorModel;
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

    }
}
