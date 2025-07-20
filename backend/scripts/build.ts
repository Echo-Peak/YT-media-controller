import * as fs from "fs";
import * as child_process from "child_process";
import path from "path";
import { writeFile } from "fs/promises";

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

const createBackendSettings = async (destPath: string): Promise<void> => {
  const settings = {
    BackendServerPort: 60166,
    UISocketServerPort: 52000,
  };
  try {
    await writeFile(destPath, JSON.stringify(settings, null, 2));
    console.log(`Settings file created at ${destPath}`);
  } catch (error: any) {
    console.error(`Error creating settings file: ${(error as Error).message}`);
  }
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
  const backendSettingsPath = path.resolve(__dirname, "../settings.json");
  const execArgs = [
    "backend\\YTMediaControllerSrv\\YTMediaControllerSrv.sln",
    "/p:Configuration=Release",
  ];

  await createBackendSettings(backendSettingsPath);
  await buildExec(execArgs);
})();
