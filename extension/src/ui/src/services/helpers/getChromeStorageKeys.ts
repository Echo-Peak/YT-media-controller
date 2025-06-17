export type ChromeStorageKeys = {
  backendServerPort?: string;
  deviceNetworkIp?: string;
  uiSocketServerPort?: string;
}

export const getChromeStorageKeys = async (): Promise<ChromeStorageKeys> => {
  return new Promise((resolve, reject) => {
    chrome.storage.local.get(["backendServerPort", "deviceNetworkIp", "uiSocketServerPort"], (result) => {
      if (chrome.runtime.lastError) {
        reject(chrome.runtime.lastError);
      } else {
        resolve({
          backendServerPort: result.backendServerPort,
          uiSocketServerPort: result.uiSocketServerPort,
          deviceNetworkIp: result.deviceNetworkIp,
        });
      }
    });
  });
}