const { spawn } = require("child_process");
const path = require("path");
const fs = require("fs");
const makeNsisBin = "C:\\Program Files (x86)\\NSIS\\makensis.exe";
const branch = process.env.GITHUB_HEAD_REF || "Develop";
const packageJson = require("../package.json");

const selectBuildEnv = (branch) => {
  switch (branch) {
    case "main":
      return "Stable";
    case "staging":
      return "Beta";
    default:
      return "Alpha";
  }
};

const ensureDir = async (dir) => {
  try {
    await fs.promises.access(dir);
  } catch (err) {
    if (err.code === "ENOENT") {
      await fs.promises.mkdir(dir, { recursive: true });
    } else {
      throw err;
    }
  }
};

const buildNumber = process.env.BUILD_NUMBER || "0";

const createAppVersionStr = () => {
  const channel = selectBuildEnv(branch).toLowerCase();
  return `${packageJson.version}-${channel}.${buildNumber}`;
};

const createAppVersionNum = () => {
  return `${packageJson.version}.${buildNumber}`;
};

const makeInstaller = async (cwd) => {
  console.log("Creating installer");
  const args = [
    `/DINSTALLER_BUILD_ENV=${selectBuildEnv(branch)}`,
    `/DAPP_VERSION_NUM=${createAppVersionNum()}`,
    `/DAPP_VERSION_STR=${createAppVersionStr()}`,
    "packager/installer.nsi",
  ];
  await ensureDir(path.join(cwd, "dist"));

  return new Promise((resolve, reject) => {
    const proc = spawn(makeNsisBin, args, { cwd, stdio: "inherit" });
    proc.on("close", resolve);
    proc.on("error", reject);
  });
};

const makeUninstaller = async (cwd) => {
  console.log("Creating uninstaller");
  const args = [
    `/DAPP_VERSION_NUM=${createAppVersionNum()}`,
    `/DAPP_VERSION_STR=${createAppVersionStr()}`,
    "packager/uninstaller.nsi",
  ];
  await ensureDir(path.join(cwd, "dist"));

  return new Promise((resolve, reject) => {
    const proc = spawn(makeNsisBin, args, { cwd, stdio: "inherit" });
    proc.on("close", resolve);
    proc.on("error", reject);
  });
};

(async () => {
  console.log("Creating NSIS installer");
  const root = process.cwd();
  console.log(`Root path: ${root}`);
  await makeUninstaller(root);
  await makeInstaller(root);
})();
