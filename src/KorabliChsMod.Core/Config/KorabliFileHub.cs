using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xanadu.KorabliChsMod.Core.Config
{
    /// <summary>
    /// 考拉比配置中心
    /// </summary>
    /// <param name="networkEngine">网络引擎</param>
    public class KorabliFileHub(INetworkEngine networkEngine) : IKorabliFileHub
    {

        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <inheritdoc />
        public ProxyConfig Proxy { get; set; } = new();

        /// <inheritdoc />
        public MirrorList Mirror { get; set; } = MirrorList.Github;

        /// <inheritdoc />
        public bool AllowPreRelease { get; set; } = false;

        /// <inheritdoc />
        public bool AutoUpdate { get; set; } = true;

        /// <inheritdoc />
        public string GameFolder { get; set; } = string.Empty;

        /// <inheritdoc />
        public void Load()
        {
            try
            {
                var saveThread = new Thread(async void () =>
                {
                    try
                    {
                        await this.SaveAsync();
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

                if (!File.Exists(IKorabliFileHub.ConfigFilePath))
                {
                    saveThread.Start();
                    return;
                }

                var config =
                    JsonConvert.DeserializeObject<KorabliFileHub>(File.ReadAllText(IKorabliFileHub.ConfigFilePath,
                        Encoding.UTF8))!;

                var updateConfig = false;
                this.Proxy = config.Proxy;
                if (string.Compare(this.Proxy.Username, IKorabliFileHub.DeprecatedHint,
                        StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.Proxy.Username = string.Empty;
                    updateConfig = true;
                }

                if (string.Compare(this.Proxy.Password, IKorabliFileHub.DeprecatedHint,
                        StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.Proxy.Password = string.Empty;
                    updateConfig = true;
                }

                this.AutoUpdate = config.AutoUpdate;
                if (this.Proxy.Enabled)
                {
                    this.UpdateEngineProxy();
                }
                
                this.GameFolder = config.GameFolder;
                if (updateConfig)
                {
                    saveThread.Start();
                }

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

        /// <inheritdoc />
        public async Task<bool> SaveAsync()
        {
            try
            {
                if (this.Proxy.Enabled)
                {
                    _ = this.UpdateEngineProxy();
                }

                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                await File.WriteAllTextAsync(IKorabliFileHub.ConfigFilePath, json, Encoding.UTF8);
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

        /// <inheritdoc />
        public bool UpdateEngineProxy(bool dry = false)
        {
            try
            {
                return networkEngine.SetProxy(new Uri(this.Proxy.Address), this.Proxy.Username, this.Proxy.Password);
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg { Message = "代理设置错误，已关闭代理", Exception = e });
                this.Proxy.Enabled = false;
                return false;
            }

        }

    }
}
