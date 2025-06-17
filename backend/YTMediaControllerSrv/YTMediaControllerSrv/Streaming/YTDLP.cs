using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv
{
    internal class YTDLP
    {
        public async Task<YTDlpJsonDump> GetVideoMetadata(string sourceUrl)
        {
            Console.WriteLine("Fetching json dump for {0}", sourceUrl);
            string jsonDump = await ExecCommand(sourceUrl, "--dump-json");
            if (string.IsNullOrWhiteSpace(jsonDump))
            {
                throw new Exception("Failed to retrieve video metadata. The output is empty.");
            }

            return JsonConvert.DeserializeObject<YTDlpJsonDump>(jsonDump);
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
                process.WaitForExit(); // Ensure all output is flushed
                if (process.ExitCode == 0 && output.Length > 0)
                {
                    tcs.TrySetResult(output.ToString().Trim());
                }
                else
                {
                    string err = error.Length > 0 ? error.ToString() : output.ToString();
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
    }
}
