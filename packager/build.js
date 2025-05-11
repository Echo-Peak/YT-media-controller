const { spawn } = require("child_process");
const path = require("path");
const fs = require("fs");

const makeNsisBin = "C:\\Program Files (x86)\\NSIS\\makensis.exe";

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
}

const makeInstaller = async (cwd) => {
  console.log("Creating installer");
  const args = ["packager/installer.nsi"];
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
