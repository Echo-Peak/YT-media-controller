export class ChromeFrontEndRuntime {
  constructor() {
    chrome.runtime.sendMessage({ action: "ytFrontendRuntimeLoaded" });
    chrome.runtime.onMessage.addListener(this.handleMessage);
  }

  private handleMessage = (
    message: any,
    sender: chrome.runtime.MessageSender,
    sendResponse: (response?: any) => void
  ) => {};

  public sendEvent = (event: { action: string; data?: any }) => {
    chrome.runtime.sendMessage(event);
  };

  public getYTTabId = (): Promise<number | undefined> => {
    return new Promise((resolve) => {
      chrome.runtime.sendMessage({ action: "getYTTabId" }, (response) => {
        if (chrome.runtime.lastError) {
          console.error(
            "Error getting YouTube tab ID:",
            chrome.runtime.lastError
          );
          resolve(undefined);
        } else {
          resolve(response?.tabId);
        }
      });
    });
  };
}
