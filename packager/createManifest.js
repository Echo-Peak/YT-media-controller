const fs = require("fs");
const crypto = require("crypto");
const path = require("path");

const installer = path.join(
  __dirname,
  "../dist/YoutubeMediaControllerInstaller.exe"
);

const manifest = {
  installerComponent: "YoutubeMediaControllerInstaller.exe",
  sha256Checksum: crypto
    .createHash("sha256")
    .update(fs.readFileSync(installer))
    .digest("hex"),
};

fs.writeFileSync("./dist/manifest.json", JSON.stringify(manifest, null, 2));
