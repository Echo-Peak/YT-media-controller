const { spawn } = require("child_process");
const path = require("path");
const fs = require("fs");
const settings = require("../backend/settings.example.json");

const makeNsisBin = "C:\\Program Files (x86)\\NSIS\\makensis.exe";
const branch = process.env.GITHUB_HEAD_REF || "Develop";

const selectEnv = (branch) => {
  switch (branch) {
    case "main":
      return "Release";
    case "staging":
      return "Staging";
    default:
      return "Develop";
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

const makeInstaller = async (cwd) => {
  console.log("Creating installer");
  const args = [
    `/DDEFAULTPORT=${settings.BackendServerPort}`,
    `/DINSTALLER_ENV=${selectEnv(branch)}`,
    "packager/installer.nsi",
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
  await makeInstaller(root);
})();
