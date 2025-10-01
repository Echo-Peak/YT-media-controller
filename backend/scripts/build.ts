import * as fs from "fs";
import * as child_process from "child_process";
import * as packageJson from "../../package.json";

const msBuildPaths = [
  [
    "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe",
    "Unable to check if 'user installed' 2019 MSBuild was found",
  ],
  [
    "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\BuildTools\\MSBuild\\Current\\Bin\\MSBuild.exe",
    "Unable to check if 'choco installed' 2019 MSBuild was found",
  ],
  [
    "C:\\Program Files (x86)\\Microsoft Visual Studio\\2022\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe",
    "Unable to check if 'user installed' 2022 MSBuild was found",
  ],
  [
    "C:\\Program Files (x86)\\Microsoft Visual Studio\\2022\\BuildTools\\MSBuild\\Current\\Bin\\MSBuild.exe",
    "Unable to check if 'choco installed' 2022 MSBuild was found",
  ],
];

const buildNumber = process.env.BUILD_NUMBER || "0";
const branch = process.env.GITHUB_HEAD_REF || "Develop";

const selectBuildEnv = () => {
  if (branch === "main") return "Stable";
  if (branch === "staging") return "Beta";
  return "Alpha";
};

const validateFile = (path: string, errorMessage: string) => {
  try {
    fs.statSync(path);
    return true;
  } catch (err) {
    console.log(errorMessage);
  }
  return false;
};

const selectMSBuild = () => {
  for (const [path, errorMessage] of msBuildPaths) {
    if (validateFile(path, errorMessage)) {
      return path;
    }
  }
  return "msbuild";
};

const buildExec = async (args: string[]): Promise<void> => {
  return new Promise((resolve, reject) => {
    const proc = child_process.spawn(selectMSBuild(), args, {
      stdio: "inherit",
    });

    proc.on("close", (code) => {
      console.log(`child process exited with code ${code}`);
      if (code === 0) {
        resolve();
      } else {
        reject(new Error(`Build process failed with code ${code}`));
      }
    });
  });
};

(async () => {
  const execArgs = [
    "backend\\YTMediaControllerSrv\\YTMediaControllerSrv.sln",
    `/p:Configuration=${selectBuildEnv()}`,
    `/p:BUILD_NUMBER=${buildNumber}`,
    `/p:VERSION_PREFIX=${packageJson.version}`,
  ];

  await buildExec(execArgs);
})();
