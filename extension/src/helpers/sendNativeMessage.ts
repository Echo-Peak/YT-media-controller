export const sendNativeMessage = <T>(nativeHostId: string, data: Record<string, unknown>): Promise<T>  => {
  return new Promise((resolve, reject) => {
    chrome.runtime.sendNativeMessage(nativeHostId, data, (response) => {
      if (chrome.runtime.lastError) {
        console.error(chrome.runtime.lastError);
        reject(new Error(chrome.runtime.lastError.message));
      } else {
        resolve(response);
      }
    });
  });
}