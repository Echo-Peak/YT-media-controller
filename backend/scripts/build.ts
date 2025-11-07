import * as fs from "fs";
import * as child_process from "child_process";
import * as packageJson from "../../package.json";

const dotnetPaths = [
  [
    "C:\\Program Files\\dotnet\\dotnet.exe",
    "Unable to check if 'dotnet' was found",
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

const selectDotnet = () => {
  for (const [path, errorMessage] of dotnetPaths) {
    if (validateFile(path, errorMessage)) {
      return path;
    }
  }
  return "dotnet";
};

const buildExec = async (command: string): Promise<void> => {
  return new Promise((resolve, reject) => {
    const childProcess = child_process.exec(
      command,
      (error, stdout, stderr) => {
        if (error) {
          console.error(`Error executing command: ${error.message}`);
          reject(error);
        } else {
          resolve();
        }
      }
    );

    if (childProcess.stdout) {
      childProcess.stdout.on("data", (data) => {
        process.stdout.write(data);
      });
    }

    if (childProcess.stderr) {
      childProcess.stderr.on("data", (data) => {
        process.stderr.write(data);
      });
    }
  });
};

const projects = ["YTMediaControllerSrv", "YTMediaControllerUpdaterSrv"];

(async () => {
  for (const project of projects) {
    const execCommand = [
      `"${selectDotnet()}"`,
      "publish",
      `backend\\YTMediaControllerSrv\\${project}\\${project}.csproj`,
      `-c ${selectBuildEnv()}`,
      "-r win-x64",
      `-p:BUILD_NUMBER=${buildNumber}`,
      `-p:VERSION_PREFIX=${packageJson.version}`,
      "-p:PublishTrimmed=True",
    ].join(" ");

    await buildExec(execCommand);
  }
})();
