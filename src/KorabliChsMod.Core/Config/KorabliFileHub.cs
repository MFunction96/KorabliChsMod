using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File;

namespace Xanadu.KorabliChsMod.Core.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class KorabliFileHub(INetworkEngine networkEngine) : IKorabliFileHub
    {

        /// <inheritdoc />
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        protected ConcurrentQueue<ulong> BackupInstances { get; } = new();

        /// <inheritdoc />
        public ProxyConfig Proxy { get; set; } = new();

        /// <inheritdoc />
        public MirrorList Mirror { get; set; } = MirrorList.Github;

        /// <inheritdoc />
        public bool AllowPreRelease { get; set; } = false;

        /// <inheritdoc />
        public bool AutoUpdate { get; set; } = true;

        /// <inheritdoc />
        public int Reserve { get; protected set; } = 2;

        /// <inheritdoc />
        public string GameFolder { get; set; } = string.Empty;

        /// <inheritdoc />
        public void Load(int reserve = 2)
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

                if (!Directory.Exists(IKorabliFileHub.BackupFolder))
                {
                    Directory.CreateDirectory(IKorabliFileHub.BackupFolder);
                }

                this.Reserve = reserve;
                //await this.ReloadBackupInstance();
                //await this.TrimBackInstance();

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
                this.UpdateEngineProxy();
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
        public Task ReloadBackupInstance(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                this.BackupInstances.Clear();
                var backups = Directory.GetDirectories(IKorabliFileHub.BackupFolder).Select(q => ulong.Parse(Path.GetFileName(q)));
                foreach (var backup in backups)
                {
                    this.BackupInstances.Enqueue(backup);
                }

            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task TrimBackInstance(bool reload = true, CancellationToken cancellationToken = default)
        {
            while (this.BackupInstances.Count > this.Reserve)
            {
                _ = this.BackupInstances.TryDequeue(out var backup);
                await IOExtension.DeleteDirectory(Path.Combine(IKorabliFileHub.BackupFolder, backup.ToString()), true, true);
            }

            if (reload)
            {
                await this.ReloadBackupInstance(cancellationToken);
            }

        }

        /// <inheritdoc />
        public async Task<string> EnqueueBackup(bool trim = false)
        {
            var now = (ulong)DateTime.Now.ToBinary();
            var folder = Path.Combine(IKorabliFileHub.BackupFolder, now.ToString());
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            this.BackupInstances.Enqueue(now);
            if (trim)
            {
                await this.TrimBackInstance();
            }

            return folder;
        }

        /// <inheritdoc />
        public string PeekLatestBackup()
        {
            return Path.Combine(IKorabliFileHub.BackupFolder, this.BackupInstances.Last().ToString());
        }

        /// <inheritdoc />
        public async Task<bool> SaveAsync()
        {
            try
            {
                if (this.Proxy.Enabled)
                {
                    this.UpdateEngineProxy();
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

        public bool UpdateEngineProxy(bool dry = false)
        {
            try
            {
                return networkEngine.SetProxy(new Uri(this.Proxy.Address), this.Proxy.Username, this.Proxy.Password);
            }
            catch (Exception e)
            {
                this.ServiceEvent?.Invoke(this, new ServiceEventArg { Message = "代理设置错误，请检查配置", Exception = e });
                return false;
            }

        }

    }
}
