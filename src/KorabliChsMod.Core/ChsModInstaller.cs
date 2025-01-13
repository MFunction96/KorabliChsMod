using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod.Core
{
    public class ChsModInstaller(INetworkEngine networkEngine, IFileCachePool fileCachePool, IGameDetector gameDetector) : IChsModInstaller
    {
        public event EventHandler<ServiceEventArg>? ServiceEvent;

        public async Task<bool> Install(MirrorList mirror, CancellationToken cancellationToken = default)
        {
            try
            {
                using var zipFile = fileCachePool.Register("Korabli_localization_chs.zip", "download");
                var zipTempFolder = Path.Combine(zipFile.Pool.BasePath, Path.GetRandomFileName());
                var response = await networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                    "https://api.github.com/repos/DDFantasyV/Korabli_localization_chs/releases"), 5, cancellationToken);
                if (response is null || !response.IsSuccessStatusCode)
                {
                    return false;
                }

                var releases = await response.Content.ReadAsStringAsync(cancellationToken);
                var jArray = JsonConvert.DeserializeObject<JArray>(releases) ?? [];
                var latest = jArray.First(q => q["prerelease"]!.Value<bool>() == gameDetector.IsTest);
                var modVersion = latest["tag_name"]!.Value<string>();
                var downloadFile = latest["zipball_url"]!.Value<string>();
                await networkEngine.DownloadAsync(new HttpRequestMessage(HttpMethod.Get, downloadFile),
                    zipFile.FullPath, 5, cancellationToken);

                using var zip = ZipFile.OpenRead(zipFile.FullPath);
                var entry = zip.Entries[0].FullName;
                var extractFolder = Path.Combine(zipTempFolder, entry);
                ZipFile.ExtractToDirectory(zipFile.FullPath, zipTempFolder, Encoding.UTF8, true);
                IOExtension.CopyDirectory(Path.Combine(extractFolder, "texts"), Path.Combine(gameDetector.ModFolder, "texts"));
                File.Copy(Path.Combine(extractFolder, "locale_config.xml"), Path.Combine(gameDetector.ModFolder, "locale_config.xml"), true);
                File.Copy(Path.Combine(extractFolder, "LICENSE"), Path.Combine(gameDetector.ModFolder, "LICENSE"), true);
                await File.WriteAllTextAsync(Path.Combine(gameDetector.ModFolder, "Korabli_localization_chs.ver"),
                    modVersion, Encoding.UTF8, cancellationToken);
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

    }
}
