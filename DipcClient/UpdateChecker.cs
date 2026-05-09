using System.Text.Json;
using System.Text.Json.Serialization;

namespace DipcClient;

public static class UpdateChecker
{
    private static string GitHubRepo => (Environment.GetEnvironmentVariable("DIPC_GITHUB_REPO") ?? "Dziondzio/dipc").Trim();
    private static Uri LatestReleaseApiUri => new($"https://api.github.com/repos/{GitHubRepo}/releases/latest");

    public static async Task<UpdateCheckResult?> CheckAsync(CancellationToken cancellationToken)
    {
        var currentInfo = GetCurrentVersionString();
        var releasesPage = $"https://github.com/{GitHubRepo}/releases/latest";

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd($"DIPC/{currentInfo} (Windows)");
            http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            http.DefaultRequestHeaders.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28");

            using var req = new HttpRequestMessage(HttpMethod.Get, LatestReleaseApiUri);
            using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
            {
                var msg = $"Nie udało się pobrać informacji o aktualizacji z GitHub.\n\nRepo: {GitHubRepo}\nHTTP: {(int)resp.StatusCode} {resp.ReasonPhrase}\n\nJeśli to limit GitHuba (rate limit), spróbuj ponownie za kilka minut.";
                return new UpdateCheckResult
                {
                    CurrentVersion = currentInfo,
                    LatestVersion = "",
                    ReleasePageUrl = releasesPage,
                    ErrorMessage = msg
                };
            }

            var rel = JsonSerializer.Deserialize<GitHubRelease>(body);
            if (rel is null || string.IsNullOrWhiteSpace(rel.TagName))
            {
                return new UpdateCheckResult
                {
                    CurrentVersion = currentInfo,
                    LatestVersion = "",
                    ReleasePageUrl = releasesPage,
                    ErrorMessage = $"GitHub zwrócił niepoprawną odpowiedź (brak tag_name).\n\nRepo: {GitHubRepo}"
                };
            }

            var latestRaw = rel.TagName.Trim().TrimStart('v', 'V');

            var portable = PickAsset(rel.Assets, isPortable: true);
            var installer = PickAsset(rel.Assets, isPortable: false);

            var portableShaUrl = PickShaAssetUrl(rel.Assets, portable?.Name);
            var installerShaUrl = PickShaAssetUrl(rel.Assets, installer?.Name);

            var portableSha = portableShaUrl is null ? null : await TryReadSha256Async(http, portableShaUrl, cancellationToken).ConfigureAwait(false);
            var installerSha = installerShaUrl is null ? null : await TryReadSha256Async(http, installerShaUrl, cancellationToken).ConfigureAwait(false);

            return new UpdateCheckResult
            {
                CurrentVersion = currentInfo,
                LatestVersion = latestRaw,
                PortableUrl = portable?.BrowserDownloadUrl,
                InstallerUrl = installer?.BrowserDownloadUrl,
                PortableSha256 = portableSha,
                InstallerSha256 = installerSha,
                Notes = rel.Body,
                ReleasePageUrl = string.IsNullOrWhiteSpace(rel.HtmlUrl) ? releasesPage : rel.HtmlUrl,
                IsUpdateAvailable = IsUpdateAvailable(currentInfo, latestRaw)
            };
        }
        catch (Exception ex)
        {
            return new UpdateCheckResult
            {
                CurrentVersion = currentInfo,
                LatestVersion = "",
                ReleasePageUrl = releasesPage,
                ErrorMessage = $"Błąd sprawdzania aktualizacji:\n{ex.Message}\n\nRepo: {GitHubRepo}"
            };
        }
    }

    private static string GetCurrentVersionString()
    {
        var asm = typeof(UpdateChecker).Assembly;
        var info = asm.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
            .FirstOrDefault()
            ?.InformationalVersion;

        return string.IsNullOrWhiteSpace(info)
            ? (asm.GetName().Version?.ToString() ?? "0.0.0")
            : info;
    }

    private static bool IsUpdateAvailable(string current, string latest)
    {
        var currentKey = ExtractBuildKey(current);
        var latestKey = ExtractBuildKey(latest);
        if (currentKey is not null && latestKey is not null)
        {
            return latestKey > currentKey;
        }

        var currentVersion = typeof(UpdateChecker).Assembly.GetName().Version ?? new Version(0, 0, 0, 0);
        return Version.TryParse(latest, out var latestVersion) && latestVersion > currentVersion;
    }

    private static long? ExtractBuildKey(string version)
    {
        var idx = version.LastIndexOf('-');
        if (idx >= 0 && idx + 1 < version.Length)
        {
            var suffix = version[(idx + 1)..].Trim();
            if (suffix.Length >= 10 && suffix.All(char.IsDigit) && long.TryParse(suffix, out var n))
            {
                return n;
            }
        }

        var digits = new string(version.Where(char.IsDigit).ToArray());
        if (digits.Length >= 10 && long.TryParse(digits, out var n2))
        {
            return n2;
        }

        return null;
    }

    private static GitHubAsset? PickAsset(List<GitHubAsset>? assets, bool isPortable)
    {
        if (assets is null || assets.Count == 0)
        {
            return null;
        }

        var exeAssets = assets
            .Where(a => !string.IsNullOrWhiteSpace(a.Name) && a.Name!.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (exeAssets.Count == 0)
        {
            return null;
        }

        if (isPortable)
        {
            var p = exeAssets.FirstOrDefault(a => a.Name!.Contains("portable", StringComparison.OrdinalIgnoreCase));
            return p ?? exeAssets.FirstOrDefault(a => a.Name!.Contains("dipc", StringComparison.OrdinalIgnoreCase)) ?? exeAssets[0];
        }

        var i = exeAssets.FirstOrDefault(a => a.Name!.Contains("installer", StringComparison.OrdinalIgnoreCase) || a.Name!.Contains("setup", StringComparison.OrdinalIgnoreCase));
        return i ?? exeAssets.FirstOrDefault(a => a.Name!.Contains("dipc", StringComparison.OrdinalIgnoreCase)) ?? exeAssets[0];
    }

    private static string? PickShaAssetUrl(List<GitHubAsset>? assets, string? forExeName)
    {
        if (assets is null || assets.Count == 0 || string.IsNullOrWhiteSpace(forExeName))
        {
            return null;
        }

        var shaName1 = forExeName + ".sha256";
        var shaName2 = forExeName + ".sha256.txt";

        var match = assets.FirstOrDefault(a =>
            string.Equals(a.Name, shaName1, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(a.Name, shaName2, StringComparison.OrdinalIgnoreCase));

        return match?.BrowserDownloadUrl;
    }

    private static async Task<string?> TryReadSha256Async(HttpClient http, string url, CancellationToken cancellationToken)
    {
        try
        {
            var text = (await http.GetStringAsync(url, cancellationToken).ConfigureAwait(false)).Trim();
            if (text.Length == 64 && text.All(Uri.IsHexDigit))
            {
                return text;
            }

            var token = text.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(token) && token.Length == 64 && token.All(Uri.IsHexDigit))
            {
                return token;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}

public sealed class UpdateCheckResult
{
    public string CurrentVersion { get; init; } = "";
    public string LatestVersion { get; init; } = "";
    public string? PortableUrl { get; init; }
    public string? InstallerUrl { get; init; }
    public string? PortableSha256 { get; init; }
    public string? InstallerSha256 { get; init; }
    public string? Notes { get; init; }
    public string? ReleasePageUrl { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsUpdateAvailable { get; init; }
}

public sealed class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string? TagName { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("body")]
    public string? Body { get; set; }
    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }
    [JsonPropertyName("assets")]
    public List<GitHubAsset>? Assets { get; set; }
}

public sealed class GitHubAsset
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("browser_download_url")]
    public string? BrowserDownloadUrl { get; set; }
}
