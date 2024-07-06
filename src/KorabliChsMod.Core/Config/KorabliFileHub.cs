﻿using Microsoft.Extensions.Logging;
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
    public class KorabliFileHub(ILogger<KorabliFileHub> logger) : IKorabliFileHub
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        protected ConcurrentQueue<ulong> BackupInstances { get; } = new();

        /// <inheritdoc />
        public ProxyConfig Proxy { get; set; } = new();

        /// <inheritdoc />
        public int Reserve { get; protected set; } = 2;

        /// <inheritdoc />
        public string GameFolder { get; set; } = string.Empty;

        /// <inheritdoc />
        public async void Load(int reserve = 2)
        {
            try
            {
                if (!Directory.Exists(IKorabliFileHub.BackupFolder))
                {
                    Directory.CreateDirectory(IKorabliFileHub.BackupFolder);
                }

                this.Reserve = reserve;
                await this.ReloadBackupInstance();
                await this.TrimBackInstance();

                if (!File.Exists(IKorabliFileHub.ConfigFilePath))
                {
                    await this.SaveAsync();
                    return;
                }

                var config =
                    JsonConvert.DeserializeObject<KorabliFileHub>(
                        await File.ReadAllTextAsync(IKorabliFileHub.ConfigFilePath, Encoding.UTF8))!;
                this.Proxy = config.Proxy;
                this.GameFolder = config.GameFolder;
            }
            catch (Exception e)
            {
                logger.LogError(e, string.Empty);
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
        public async Task SaveAsync()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            await File.WriteAllTextAsync(IKorabliFileHub.ConfigFilePath, json, Encoding.UTF8);
        }
    }
}
