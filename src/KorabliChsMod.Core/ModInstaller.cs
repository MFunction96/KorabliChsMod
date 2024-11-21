using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xanadu.Skidbladnir.IO.File;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace Xanadu.KorabliChsMod.Core
{
    public class ModInstaller(INetworkEngine networkEngine, IFileCachePool fileCachePool, IGameDetector gameDetector) : IModInstaller
    {
        public async Task Install(CancellationToken cancellationToken = default)
        {
            using var zipFile = fileCachePool.Register("Korabli_localization_chs.zip", "download");
            var zipTempFolder = Path.Combine(zipFile.Pool.BasePath, Path.GetRandomFileName());
            var response = await networkEngine.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                "https://api.github.com/repos/DDFantasyV/Korabli_localization_chs/releases"), 5, cancellationToken);
            if (response is null || !response.IsSuccessStatusCode)
            {
                return;
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
            var zipFolder = Path.Combine(zipTempFolder, entry);
            ZipFile.ExtractToDirectory(zipFile.FullPath, zipTempFolder, Encoding.UTF8, true);
            IOExtension.CopyDirectory(zipFolder, gameDetector.ModFolder);
            await File.WriteAllTextAsync(Path.Combine(gameDetector.ModFolder, "Korabli_localization_chs.ver"),
                modVersion, Encoding.UTF8, cancellationToken);
        }
    }
}
