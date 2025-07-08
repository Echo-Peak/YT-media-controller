export const doesTabExist = (tabId?: number): Promise<boolean> => {
  if (tabId === undefined) {
    return Promise.resolve(false);
  }
  return new Promise((resolve) => {
    chrome.tabs.get(tabId, (tab) => {
      if (chrome.runtime.lastError || !tab) {
        resolve(false);
      } else {
        resolve(true);
      }
    });
  });
};
