export type ChromeStorageKeys = {
  backendServerPort?: string;
  deviceNetworkIp?: string;
  apiServerPort?: string;
}

export const getChromeStorageKeys = async (): Promise<ChromeStorageKeys> => {
  return new Promise((resolve, reject) => {
    chrome.storage.local.get(["backendServerPort", "deviceNetworkIp", "apiServerPort"], (result) => {
      if (chrome.runtime.lastError) {
        reject(chrome.runtime.lastError);
      } else {
        resolve({
          backendServerPort: result.backendServerPort,
          deviceNetworkIp: result.deviceNetworkIp,
        });
      }
    });
  });
}