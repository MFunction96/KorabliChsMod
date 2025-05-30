using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xanadu.KorabliChsMod.Core.Models;

namespace Xanadu.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 考拉比配置服务
    /// </summary>
    public class KorabliConfigService : IServiceEvent
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
        private static KorabliConfigModel DefaultKorabliConfigModel => new()
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
        public KorabliConfigModel CurrentConfig { get; private set; } = KorabliConfigService.DefaultKorabliConfigModel;

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <returns>加载的配置文件</returns>
        public KorabliConfigModel Load()
        {
            try
            {
                if (!File.Exists(KorabliConfigModel.ConfigFilePath))
                {
                    this.SaveAsync().ConfigureAwait(false);
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

                if (updateConfig)
                {
                    this.SaveAsync().ConfigureAwait(false);
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
                var json = JsonConvert.SerializeObject(this.CurrentConfig, Formatting.Indented);
                if (!Directory.Exists(KorabliConfigModel.BaseFolder))
                {
                    Directory.CreateDirectory(KorabliConfigModel.BaseFolder);
                }

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

    }
}
