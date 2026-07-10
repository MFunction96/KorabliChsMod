using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.Skidbladnir.IO.File;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 汉化安装实现，Transient 生命周期
    /// </summary>
    /// <param name="networkEngine">网络引擎</param>
    /// <param name="fileCachePool">文件缓存</param>
    /// <param name="metadataService">元信息获取</param>
    /// <param name="korabliConfigService">考拉比配置中心</param>
    public partial class ChsModService(NetworkEngine networkEngine, FileCachePool fileCachePool, MetadataService metadataService, KorabliConfigService korabliConfigService) : IServiceEvent
    {
        [GeneratedRegex(@"(?<Version>\d+\.\d+)", RegexOptions.ExplicitCapture)]
        private static partial Regex GameVersionRegex();

        public static Regex VersionRegex => ChsModService.GameVersionRegex();

        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <summary>
        /// 安装汉化模组
        /// </summary>
        /// <param name="gameDetectModel">镜像</param>
        /// <param name="cancellationToken">中止消息</param>
        /// <returns>成功返回true，失败返回false</returns>
        public async Task<bool> Install(GameDetectModel gameDetectModel, CancellationToken cancellationToken = default)
        {
            return await this.PathXmlAddon(gameDetectModel, cancellationToken) && await this.CorePackInstall(gameDetectModel, cancellationToken)&& await this.ImeConfigInstall(gameDetectModel, cancellationToken)
;
        }

        /// <summary>
        /// 卸载汉化模组，仅移除汉化模组文件，不移除paths.xml中的mods路径
        /// </summary>
        /// <param name="gameDetectModel">游戏检测模块</param>
        /// <param name="cancellationToken">取消句柄</param>
        /// <returns>是否成功卸载</returns>
        public bool Uninstall(GameDetectModel gameDetectModel, CancellationToken cancellationToken = default)
        {
            try
            {
                IOExtension.DeleteFile(gameDetectModel.ChsModFilePath);
                IOExtension.DeleteFile(Path.Combine(gameDetectModel.ModFolder, "gui", "flash", "ime_config.xml"));
                return true;
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Exception = e
                });
                return false;
            }
        }

        /// <summary>
        /// 添加paths.xml中的mods路径
        /// </summary>
        /// <param name="gameDetectModel">游戏检测模型</param>
        /// <param name="cancellationToken">取消句柄</param>
        /// <returns>paths.xml安装状态</returns>
        internal Task<bool> PathXmlAddon(GameDetectModel gameDetectModel, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (GameDetectorService.PathXmlCheck(gameDetectModel))
                    {
                        return true;
                    }

                    XDocument doc;
                    XElement? pathsElement;

                    if (!File.Exists(gameDetectModel.PathXmlPath))
                    {
                        doc = new XDocument(new XElement("root", new XElement("Paths")));
                        pathsElement = doc.Element("root")!.Element("Paths")!;
                        var directory = Path.GetDirectoryName(gameDetectModel.PathXmlPath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                    }
                    else
                    {
                        try
                        {
                            doc = XDocument.Load(gameDetectModel.PathXmlPath);
                            var root = doc.Element("root");
                            if (root is null)
                            {
                                root = new XElement("root");
                                doc.Add(root);
                            }

                            pathsElement = root.Element("Paths");
                            if (pathsElement is null)
                            {
                                pathsElement = new XElement("Paths");
                                root.Add(pathsElement);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException($"无法加载或解析XML文件：{gameDetectModel.PathXmlPath}", ex);
                        }
                    }

                    var existingModsPaths = pathsElement.Elements("Path")
                        .Where(p => p.Attribute("type")?.ToString() == "mods")
                        .ToArray();

                    foreach (var pathNode in existingModsPaths)
                    {
                        pathNode.Remove();
                    }

                    var newModsPath = new XElement("Path",
                        new XAttribute("type", "mods"),
                        @"..\mods"
                    );

                    pathsElement.AddFirst(newModsPath);
                    using var writer = XmlWriter.Create(gameDetectModel.PathXmlPath, new XmlWriterSettings
                    {
                        Indent = true,
                        Encoding = new UTF8Encoding(false),
                        OmitXmlDeclaration = true
                    });

                    doc.Save(writer);
                    return true;
                }
                catch (Exception e)
                {
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Exception = e
                    });
                    return false;
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 核心汉化包安装
        /// </summary>
        /// <param name="gameDetectModel">游戏检测模块</param>
        /// <param name="cancellationToken">取消句柄</param>
        /// <returns></returns>
        internal async Task<bool> CorePackInstall(GameDetectModel gameDetectModel, CancellationToken cancellationToken = default)
        {
            using var modFileCache = fileCachePool.Register(GameDetectModel.ChsModFileName, "download");
            try
            {
                var gameVersion = ChsModService.VersionRegex.Match(gameDetectModel.GameVersion).Value;
                var latest = await metadataService.GetModRelease(Version.Parse(gameVersion),
                    gameDetectModel.IsTest);
                var downloadFile = latest.Assets.First(q => q.Name == GameDetectModel.ChsModFileName).BrowserDownloadUrl;
                using var request = new HttpRequestMessage(HttpMethod.Get, downloadFile);
                if (korabliConfigService.CurrentConfig.Mirror == MirrorList.Kodo)
                {
                    request.Headers.Referrer = new Uri("http://korablichsmod-kodo.mfbrain.xyz/");
                }
                await networkEngine.DownloadAsync(request, modFileCache.FullPath, 5, cancellationToken);
                if (!Directory.Exists(gameDetectModel.ModFolder))
                {
                    Directory.CreateDirectory(gameDetectModel.ModFolder);
                }

                File.Copy(modFileCache.FullPath, gameDetectModel.ChsModFilePath, true);
                return true;
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Exception = e
                });

                return false;
            }
        }


        internal Task<bool> ImeConfigInstall(GameDetectModel gameDetectModel, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                try
                {
                    // 源文件：程序目录下的 Assets/ime_config.xml
                    var source = Path.Combine(AppContext.BaseDirectory, "Assets", "ime_config.xml");
                    if (!File.Exists(source))
                    {
                        // 你可以选择：当作失败 or 直接跳过。
                        // “打包内置”的目标是必有，所以我建议：缺失就当失败，方便发现打包问题
                        throw new FileNotFoundException("未找到内置 ime_config.xml，请检查安装包是否包含 Assets/ime_config.xml", source);
                    }

                    // 目标文件：.../res_mods/<ver>/gui/flash/ime_config.xml
                    var target = Path.Combine(gameDetectModel.ModFolder, "gui", "flash", "ime_config.xml");
                    var targetDir = Path.GetDirectoryName(target)!;
                    Directory.CreateDirectory(targetDir);

                    File.Copy(source, target, overwrite: true);
                    return true;
                }
                catch (Exception e)
                {
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg { Exception = e });
                    return false;
                }
            }, cancellationToken);
        }
    }

}
