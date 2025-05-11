const fs = require("fs");
const child_process = require("child_process");

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

const validateFile = (path, errorMessage) => {
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

const args = [
  "backend\\YTMediaControllerSrv\\YTMediaControllerSrv.sln",
  "/p:Configuration=Release",
];

const proc = child_process.spawn(selectMSBuild(), args, { stdio: "inherit" });

proc.on("close", (code) => {
  console.log(`child process exited with code ${code}`);
});
