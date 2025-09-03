using Newtonsoft.Json;
using Octokit;
using Semver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Lifetime;
using System.Text;
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
        public GHReleases()
        {
            http.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream")
            );

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

            return allReleases.Where(r => !r.Draft && !string.IsNullOrEmpty(r.TagName))
            .Select(r =>
            {
                var tag = r.TagName.TrimStart('v', 'V');
                return new { Release = r, Sem = SemVersion.Parse(tag, SemVersionStyles.Strict) };
            })
            .Where(x => !string.IsNullOrEmpty(x.Sem.Prerelease) &&
                        (x.Sem.Prerelease == channel || x.Sem.Prerelease.StartsWith(channel + ".")))
            .OrderByDescending(x => x.Sem.ToString())
            .Select(x => x.Release)
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
                    var content = await http.GetStringAsync(assetUrl);
                    return JsonConvert.DeserializeObject<T>(content);
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
            var outputPath = Path.Combine(destDir, assetName);

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
