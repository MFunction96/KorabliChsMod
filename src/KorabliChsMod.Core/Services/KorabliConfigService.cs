using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Models;

namespace Xanadu.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 考拉比配置服务
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    public class KorabliConfigService(IServiceProvider serviceProvider) : IServiceEvent
    {
        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <summary>
        /// 当前配置文件版本
        /// </summary>
        public static Version CurrentVersion => new(1, 0, 0);

        /// <summary>
        /// 默认配置
        /// </summary>
        public static KorabliConfigModel DefaultKorabliConfigModel => new()
        {
            Mirror = MirrorList.Cloudflare,
            Proxy = new ProxyConfigModel(),
            AutoUpdate = true,
            GameFolder = string.Empty,
            Version = KorabliConfigService.CurrentVersion
        };

        /// <summary>
        /// 当前考拉比配置模型
        /// </summary>
        public volatile KorabliConfigModel CurrentConfig = KorabliConfigService.DefaultKorabliConfigModel;

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <returns>加载的配置文件</returns>
        public KorabliConfigModel Load()
        {
            try
            {
                var saveThread = new Thread(async void () =>
                {
                    try
                    {
                        _ = await this.SaveAsync();
                    }
                    catch (Exception e)
                    {
                        this.ServiceEvent?.Invoke(this, new ServiceEventArg
                        {
                            Exception = e,
                            Message = "配置文件写入失败！"
                        });
                    }

                });

                if (!File.Exists(KorabliConfigModel.ConfigFilePath))
                {
                    saveThread.Start();
                    return this.CurrentConfig;
                }
                
                this.CurrentConfig = JsonConvert.DeserializeObject<KorabliConfigModel>(File.ReadAllText(KorabliConfigModel.ConfigFilePath, Encoding.UTF8))!;

                var updateConfig = false;
                if (this.CurrentConfig.Version < KorabliConfigService.CurrentVersion)
                {
                    this.ServiceEvent?.Invoke(this, new ServiceEventArg
                    {
                        Message = $"配置文件版本过低，已重置为默认配置。当前版本：{this.CurrentConfig.Version}，最新版本：{KorabliConfigService.CurrentVersion}"
                    });

                    this.CurrentConfig = KorabliConfigService.DefaultKorabliConfigModel;
                    updateConfig = true;
                }

                if (this.CurrentConfig.Proxy.Enabled)
                {
                    var updateProxy = this.UpdateEngineProxy();
                    if (!updateProxy)
                    {
                        updateConfig = true;
                    }

                }

                if (updateConfig)
                {
                    saveThread.Start();
                }

                return this.CurrentConfig;
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Exception = e,
                    Message = "配置文件加载失败！"
                });

                throw;
            }

        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <returns>保存成功为true，保存失败为false。</returns>
        public async Task<bool> SaveAsync()
        {
            try
            {
                if (this.CurrentConfig.Proxy.Enabled)
                {
                    _ = this.UpdateEngineProxy();
                }

                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                await File.WriteAllTextAsync(KorabliConfigModel.ConfigFilePath, json, Encoding.UTF8);
                return true;
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg
                {
                    Exception = e,
                    Message = $"配置文件保存失败！\r\n{e.Message}"
                });

                return false;
            }

        }

        /// <summary>
        /// 更新网络引擎的代理设置
        /// </summary>
        /// <returns>成功为true，失败为false。</returns>
        public bool UpdateEngineProxy()
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                using var networkEngine = scope.ServiceProvider.GetRequiredService<NetworkEngine>();
                return true;
            }
            catch (Exception)
            {
                this.CurrentConfig.Proxy.Enabled = false;
                return false;
            }

        }

    }
}
