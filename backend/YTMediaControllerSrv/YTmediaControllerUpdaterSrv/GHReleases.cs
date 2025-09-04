using Newtonsoft.Json;
using Octokit;
using Semver;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YTMediaControllerUpdaterSrv
{
    internal class GHReleases
    {
        private const string Owner = "Echo-Peak";
        private const string Repo = "YT-media-controller";
        private Release latestRelese;
        private GitHubClient GitHubClient { get; set; }
        private HttpClient http = new HttpClient();
        static readonly Regex TagRx = new Regex(@"^[vV]?(?<core>\d+\.\d+\.\d+)-(?<chan>[A-Za-z]+)\.(?<n>\d+)$", RegexOptions.Compiled);
        public GHReleases()
        {
            var updaterVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            http.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream")
            );
            http.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("YTMediaControllerUpdater", updaterVersion));

            GitHubClient = new GitHubClient(new ProductHeaderValue("YTMediaControllerUpdaterSrv"));
        }

        public async Task<SemVersion> GetLatest(string channel)
        {
            var latest = await SelectReleaseByChannel(channel);

            var reg = new Regex(@"(\d+\.\d+\.\d+)(-\w+\.\d+)?");
            var matches = reg.Matches(latest.TagName);
            if (matches.Count == 0)
            {
                throw new Exception("Unable to parse latest version");
            }
            var tagVersion = matches[0].Value;

            SemVersion.TryParse(tagVersion, SemVersionStyles.Any, out var semv);

            latestRelese = latest;

            return semv;
        }

        private async Task<Release> SelectReleaseByChannel(string channel)
        {
            var allReleases = await GitHubClient.Repository.Release.GetAll(Owner, Repo);

            return allReleases
            .Where(r => !r.Draft && !string.IsNullOrWhiteSpace(r.TagName))
            .Select(r => new { r, m = TagRx.Match(r.TagName) })
            .Where(x => x.m.Success && string.Equals(x.m.Groups["chan"].Value, channel, StringComparison.OrdinalIgnoreCase))
            .Select(x => new
            {
                x.r,
                core = Version.Parse(x.m.Groups["core"].Value),
                n = int.Parse(x.m.Groups["n"].Value)
            })
            .OrderByDescending(x => x.core)
            .ThenByDescending(x => x.n)
            .Select(x => x.r)
            .FirstOrDefault();
        }

        private string SelectAssetUrl(string assetName)
        {
            foreach (var asset in latestRelese.Assets)
            {
                if (asset.Name == assetName)
                {
                    return asset.Url;
                }
            }
            return null;
        }

        public async Task<T> GetAsset<T>(string assetName)
        {

            if (latestRelese == null) {
                throw new Exception("Unable to get asset from release. Latest release is not set");
            }

            var assetUrl = SelectAssetUrl(assetName);
            if(assetUrl != null)
            {
                using (var resp = await http.GetAsync(assetUrl, HttpCompletionOption.ResponseContentRead))
                {
                    var body = await resp.Content.ReadAsStringAsync();

                    if(resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<T>(body);
                    }
                    else
                    {
                        throw new HttpRequestException($"Invalid status code when retreiving asset. Got: {resp.StatusCode}");
                    }
                }
            }

            throw new Exception($"Unable to find {assetName}");
        }

        public async Task<string> DownloadAsset(string assetName, string destDir)
        {
            if (latestRelese == null)
            {
                throw new Exception("Unable to get asset from release. Latest release is not set");
            }

            var assetUrl = SelectAssetUrl(assetName);
            var guid = Guid.NewGuid().ToString();
            var outputPath = Path.Combine(destDir, $"{guid}-{assetName}");


            using (HttpResponseMessage response = await http.GetAsync(assetUrl, HttpCompletionOption.ResponseHeadersRead))
            using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                          fileStream = new FileStream(outputPath, System.IO.FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                await contentStream.CopyToAsync(fileStream);
            }

            return outputPath;
        }
    }
}
