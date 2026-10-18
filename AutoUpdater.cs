using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace DynamicSchematicOptimizer;

internal static class AutoUpdater
{
    private const string ReleasesUrl = "https://github.com/michcio15/DynamicSchematicOptimizer/releases";

    private const string AssetName = "DynamicSchematicOptimizer.dll";

    private static readonly Regex VersionPattern = new(@"\d+(?:\.\d+){1,3}", RegexOptions.Compiled);

    public static async Task CheckForUpdateAsync()
    {
        try
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("DynamicSchematicOptimizer-AutoUpdater");

            // releases/latest redirects to releases/tag/<tag>, so the final request uri carries the tag
            using HttpResponseMessage latest = await client.GetAsync($"{ReleasesUrl}/latest", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            latest.EnsureSuccessStatusCode();

            string tagName = latest.RequestMessage.RequestUri.Segments.Last();
            if (!TryParseVersion(tagName, out Version? latestVersion))
            {
                Log.Warn("Auto updater couldn't read the latest release version from GitHub");
                return;
            }

            Version currentVersion = DynamicSchematicOptimizerPlugin.Instance.Version;
            if (latestVersion <= currentVersion)
            {
                Log.Info($"DSO is up to date ({currentVersion})");
                return;
            }

            string pluginPath = DynamicSchematicOptimizerPlugin.Instance.FilePath;
            if (string.IsNullOrEmpty(pluginPath))
            {
                Log.Warn("Auto updater couldn't locate the plugin's own file on disk");
                return;
            }

            using HttpResponseMessage asset = await client.GetAsync($"{ReleasesUrl}/download/{tagName}/{AssetName}").ConfigureAwait(false);
            if (asset.StatusCode == HttpStatusCode.NotFound)
            {
                Log.Warn($"Auto updater found version {latestVersion} but no {AssetName} was attached to the release");
                return;
            }

            asset.EnsureSuccessStatusCode();
            byte[] bytes = await asset.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

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
