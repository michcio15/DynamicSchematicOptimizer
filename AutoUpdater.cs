using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

using YamlDotNet.Serialization;

namespace DynamicSchematicOptimizer;

internal static class AutoUpdater
{
    private const string LatestReleaseUrl = "https://api.github.com/repos/michcio15/DynamicSchematicOptimizer/releases/latest";

    private static readonly Regex VersionPattern = new(@"\d+(?:\.\d+){1,3}", RegexOptions.Compiled);

    public static async Task CheckForUpdateAsync()
    {
        try
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("DynamicSchematicOptimizer-AutoUpdater");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

            string json = await client.GetStringAsync(LatestReleaseUrl).ConfigureAwait(false);

            IDeserializer deserializer = new DeserializerBuilder().Build();
            Dictionary<object, object> release = deserializer.Deserialize<Dictionary<object, object>>(json);

            if (!release.TryGetValue("tag_name", out object? tagObj) || tagObj is not string tagName ||
                !TryParseVersion(tagName, out Version? latestVersion))
            {
                Log.Warn("Auto updater couldn't read the latest release version from GitHub");
                return;
            }

            Version currentVersion = DynamicSchematicOptimizerPlugin.Instance.Version;
            if (latestVersion <= currentVersion)
            {
                Log.Debug($"DSO is up to date ({currentVersion})");
                return;
            }

            if (!release.TryGetValue("assets", out object? assetsObj) || assetsObj is not List<object> assets)
            {
                Log.Warn($"Auto updater found version {latestVersion} but the release has no assets");
                return;
            }

            string? downloadUrl = FindDllAssetUrl(assets);
            if (downloadUrl == null)
            {
                Log.Warn($"Auto updater found version {latestVersion} but no .dll asset was attached to the release");
                return;
            }

            string pluginPath = DynamicSchematicOptimizerPlugin.Instance.FilePath;
            if (string.IsNullOrEmpty(pluginPath))
            {
                Log.Warn("Auto updater couldn't locate the plugin's own file on disk");
                return;
            }

            byte[] bytes = await client.GetByteArrayAsync(downloadUrl).ConfigureAwait(false);

            string tempPath = pluginPath + ".update";
            await File.WriteAllBytesAsync(tempPath, bytes);
            File.Copy(tempPath, pluginPath, true);
            File.Delete(tempPath);

            Log.Info($"Downloaded Dynamic Schematic Optimizer {latestVersion} (running {currentVersion}). Restart the server to apply the update.");
        }
        catch (Exception e)
        {
            Log.Error($"Auto updater failed: {e}");
        }
    }

    private static string? FindDllAssetUrl(List<object> assets)
    {
        foreach (object assetObj in assets)
        {
            if (assetObj is not Dictionary<object, object> asset)
            {
                continue;
            }

            if (asset.TryGetValue("name", out object? nameObj) && nameObj is string name &&
                name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) &&
                asset.TryGetValue("browser_download_url", out object? urlObj) && urlObj is string url)
            {
                return url;
            }
        }

        return null;
    }

    private static bool TryParseVersion(string tag, [NotNullWhen(true)] out Version? version)
    {
        Match match = VersionPattern.Match(tag);
        if (match.Success)
        {
            return Version.TryParse(match.Value, out version);
        }

        version = null;
        return false;
    }
}