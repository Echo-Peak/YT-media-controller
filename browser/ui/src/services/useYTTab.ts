export const useYTTab = () => {
  const getYTTabId = (): Promise<number | undefined> => {
    return new Promise((resolve) => {
      chrome.runtime.sendMessage(
        { action: 'getYTTabId' },
        (response: { tabId: number }) => {
          if (chrome.runtime.lastError) {
            console.error(
              'Error getting YouTube tab ID:',
              chrome.runtime.lastError,
            );
            resolve(undefined);
          } else {
            resolve(response?.tabId);
          }
        },
      );
    });
  };

  return { getYTTabId };
};
