using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using YTMediaControllerSrv.Server;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv.YTDLP
{
    internal class YtdlpExec
    {
        private VideoCache Cache { get; set; }

        public YtdlpExec(VideoCache videoCache) {
            Cache = videoCache;
        }
        public async Task<YTDlpJsonDump> Fetch(string sourceUrl)
        {
            Logger.Info($"Fetching json dump for {sourceUrl}");
            string jsonDump = await ExecCommand(sourceUrl, "--dump-json");
            if (string.IsNullOrWhiteSpace(jsonDump))
            {
                throw new Exception("Failed to retrieve video metadata. The output is empty.");
            }

            YTDlpJsonDump json = JsonConvert.DeserializeObject<YTDlpJsonDump>(jsonDump);
            Cache.Add(json.Id, json);
            return json;
        }

        private Task<string> ExecCommand(string sourceUrl, params string[] commandList)
        {
            var tcs = new TaskCompletionSource<string>();
            string ytdlpBin = PathResolver.GetYtDlpBin();

            if (!File.Exists(ytdlpBin))
            {
                throw new FileNotFoundException("yt-dlp binary not found", ytdlpBin);
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytdlpBin,
                    Arguments = string.Join(" ", commandList) + " " + sourceUrl,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    output.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    error.AppendLine(e.Data);
            };

            process.Exited += (sender, e) =>
            {
                process.WaitForExit();
                try
                {
                    VerifyOutput(output.ToString());
                    tcs.TrySetResult(output.ToString().Trim());

                }
                catch (Exception verifyError)
                {
                    string err = error.Length > 0 ? error.ToString() : verifyError.Message;
                    tcs.TrySetException(new Exception($"yt-dlp failed with exit code {process.ExitCode}. Output: {err}"));
                }

                process.Dispose();
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }
        private void VerifyOutput(string output)
        {
            if (output.Length == 0)
            {
                throw new Exception("yt-dlp returned empty output.");
            }
            if (!output.StartsWith("{\"id\":"))
            {
                throw new Exception("yt-dlp encountered an error: " + output.ToString());
            }
        }
    }
}
