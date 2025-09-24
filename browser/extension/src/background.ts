import { ChromeBackgroundRuntime } from "./ChromeBackgroundRuntime";

const runtime = new ChromeBackgroundRuntime();

runtime
  .init()
  .then(() => {
    console.log("ChromeBackgroundRuntime initialized");
  })
  .catch(console.error);
export default runtime;
