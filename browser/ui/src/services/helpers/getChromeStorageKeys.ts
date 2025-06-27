export type ChromeStorageKeys = {
  backendServerPort?: string;
  deviceNetworkIp?: string;
  uiSocketServerPort?: string;
};

export const getChromeStorageKeys = async (): Promise<ChromeStorageKeys> => {
  return new Promise<ChromeStorageKeys>((resolve, reject) => {
    if (!chrome || !chrome.storage || !chrome.storage.local) {
      //reject(new Error('Chrome storage API is not available'));
      resolve({});
      return;
    }
    chrome.storage.local.get(
      ['backendServerPort', 'deviceNetworkIp', 'uiSocketServerPort'],
      (result: ChromeStorageKeys) => {
        if (chrome.runtime.lastError) {
          reject(chrome.runtime.lastError);
        } else {
          resolve({
            backendServerPort: result.backendServerPort,
            uiSocketServerPort: result.uiSocketServerPort,
            deviceNetworkIp: result.deviceNetworkIp,
          });
        }
      },
    );
  });
};
