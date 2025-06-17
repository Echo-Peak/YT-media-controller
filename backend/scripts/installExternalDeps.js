const fs = require("fs");
const https = require("https");
const path = require("path");
const os = require("os");
const { exec } = require("child_process");
const { URL } = require("url");

const ytDlpVersion = "2025.06.09";
const installDir = path.join(__dirname, "../externalBins");

const forceUpdate = process.argv.includes("--force-update");

const createUuid = () => {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

const extractVia7z = (archivePath, dest) => {
  return new Promise((resolve, reject) => {
    const bin = "C:\\Program Files\\7-Zip\\7z.exe";
    const command = `"${bin}" e "${archivePath}" -o"${dest}" "*/bin/ffmpeg.exe" -y`;
    exec(command, (error, stdout, stderr) => {
      if (error) {
        console.error(`Error extracting archive: ${error.message}`);
        reject(error);
      } else if (stderr) {
        console.error(`Error output: ${stderr}`);
        reject(new Error(stderr));
      } else {
        console.log(`Extraction successful: ${stdout}`);
        resolve();
      }
    });
  })
}

const download = (url, dest, maxRedirects = 5) => {
  return new Promise((resolve, reject) => {
    const file = fs.createWriteStream(dest);

    const doRequest = (url, redirectsLeft) => {
      const urlParts = new URL(url);

      const options = {
        port: 443,
        hostname: urlParts.hostname,
        path: urlParts.pathname + urlParts.search,
        headers: {
          "User-Agent": "Mozilla/5.0",
        },
      };

      https.get(options, (response) => {
        if (response.statusCode >= 300 && response.statusCode < 400 && response.headers.location) {
          if (redirectsLeft === 0) return reject(new Error("Too many redirects"));
          return doRequest(new URL(response.headers.location, url).toString(), redirectsLeft - 1);
        }

        if (response.statusCode !== 200) {
          return reject(new Error(`Failed to download ${url}: ${response.statusCode}`));
        }

        response.pipe(file);
        file.on("finish", () => file.close(resolve));
      }).on("error", reject);
    };

    doRequest(url, maxRedirects);
  });
};

const downloadYtDlp = async (url) => {
  const filePath = path.join(installDir, "yt-dlp.exe");
  try {
    const exists = await fs.promises.stat(filePath);
    if (exists && !forceUpdate) {
      console.log("yt-dlp already exists, skipping download.");
      return;
    }
  }catch(err){
    // File does not exist, proceed with download
  }

  try {
    await download(url, filePath);
  }catch(err){
    console.error("Unable to download yt-dlp", err);
  }
}

const downloadFFMpeg = async (url) => {
  const filePath = path.join(installDir, "ffmpeg.exe");
  const tempPath = path.join(os.tmpdir(), `${createUuid()}-ffmpeg.7z`);
  try {
    const binExists = await fs.promises.stat(filePath);
    if (binExists && !forceUpdate) {
      console.log("ffmpeg already exists, skipping download.");
      return;
    }
  }catch(err){
    // File does not exist, proceed with download
  }

  try {
    await download(url, tempPath);
    await extractVia7z(tempPath, installDir);
  } catch (err) {
    console.error("Unable to download ffmpeg", err);
  }
}

(async () => {
  await fs.promises.mkdir(installDir, { recursive: true });
  await downloadYtDlp(`https://github.com/yt-dlp/yt-dlp/releases/download/${ytDlpVersion}/yt-dlp.exe`);
  await downloadFFMpeg("https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-essentials.7z");
})();