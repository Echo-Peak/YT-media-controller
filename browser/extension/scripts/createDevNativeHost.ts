import { writeFileSync } from "fs";
import path from "path";

const distDir = path.resolve(
  __dirname,
  "../../../dist/browser-extension-unpacked"
);
const extensionId = process.env.EXTENSION_ID;

if (!extensionId) {
  throw new Error("EXTENSION_ID environment variable is not set.");
}

console.log(`Creating native host manifest for extension ID: ${extensionId}`);

const nativeHostManifest = {
  name: "com.ytmediacontroller.app",
  manifest_version: 3,
  version: "1.0",
  description: "YT Native Host",
  path: "C:\\projects\\Personal\\2025\\YT-media-controller\\backend\\YTMediaControllerSrv\\YTMediaControllerHost\\bin\\Debug\\YTMediaControllerHost.exe",
  type: "stdio",
  allowed_origins: [`chrome-extension://${extensionId}`],
};

const outputPath = path.join(distDir, "nativeHost.json");

writeFileSync(outputPath, JSON.stringify(nativeHostManifest, null, 2), "utf8");
