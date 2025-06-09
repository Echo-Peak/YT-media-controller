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
        public YTDlpJsonDump GetVideoMetadata(string sourceUrl)
        {
            string jsonDump = ExecCommand(sourceUrl, "--dump-json");
            return JsonConvert.DeserializeObject<YTDlpJsonDump>(jsonDump);
        }

        private string ExecCommand(string sourceUrl, params string[] commandList)
        {
            string ytdlpBin = PathResolver.GetYtDlpBin();
            if (!File.Exists(ytdlpBin))
            {
                throw new FileNotFoundException("yt-dlp binary not found", ytdlpBin);
            }

            Process process = new Process();
            process.StartInfo.FileName = ytdlpBin;
            process.StartInfo.Arguments = string.Join(" ", commandList) + " " + sourceUrl;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception($"yt-dlp command failed with exit code {process.ExitCode}. Output: {output}");
            }
            if (string.IsNullOrEmpty(output))
            {
                throw new Exception("yt-dlp command returned empty output.");
            }
            return output.Trim();
        }
    }
}
