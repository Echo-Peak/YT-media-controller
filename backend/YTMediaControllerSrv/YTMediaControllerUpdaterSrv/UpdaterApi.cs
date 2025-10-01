using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerUpdaterSrv.Types;

namespace YTMediaControllerUpdaterSrv
{
    internal class UpdaterApi
    {
        private string channelsUrl = "https://raw.githubusercontent.com/Echo-Peak/YT-media-controller/refs/heads/update-metadata/docs/channels.json";
        private HttpClient http = new HttpClient();
        public async Task<ChannelVersionInfo> GetLatest(string channel)
        {
            using (var resp = await http.GetAsync(channelsUrl, HttpCompletionOption.ResponseContentRead))
            {
                var body = await resp.Content.ReadAsStringAsync();

                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var channelsJson = JsonConvert.DeserializeObject<ChannelsDto>(body);
                    return channelsJson.Select(channel);
                }
                else
                {
                    throw new HttpRequestException($"Invalid status code when retreiving asset. Got: {resp.StatusCode}");
                }
            }
        }

        public async Task<string> Download(ChannelVersionInfo versionInfo, string destDir)
        {
            if (versionInfo == null)
            {
                throw new Exception("Unable to get version info");
            }
            string[] urlParts = versionInfo.InstallerUrl.Split('/');
            var assetName = urlParts[urlParts.Length - 1];
            var guid = Guid.NewGuid().ToString();
            var outputPath = Path.Combine(destDir, $"{guid}-{assetName}");


            using (HttpResponseMessage response = await http.GetAsync(versionInfo.InstallerUrl, HttpCompletionOption.ResponseHeadersRead))
            using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                          fileStream = new FileStream(outputPath, System.IO.FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                await contentStream.CopyToAsync(fileStream);
            }

            return outputPath;
        }
    }
}
