const fs = require("fs");
const crypto = require("crypto");
const path = require("path");

const distDir = path.join(__dirname, "../dist");
const installer = path.join(distDir, "YoutubeMediaControllerInstaller.exe");

const manifest = {
  installerComponent: "YoutubeMediaControllerInstaller.exe",
  sha256Checksum: crypto
    .createHash("sha256")
    .update(fs.readFileSync(installer))
    .digest("hex"),
};

fs.writeFileSync(
  path.join(distDir, "manifest.json"),
  JSON.stringify(manifest, null, 2)
);
