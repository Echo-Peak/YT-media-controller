import { NativeHostApi } from "./NativeHostApi";

export class ChromeBackgroundRuntime {
  private nativeHost: NativeHostApi;
  private deviceNetworkIP?: string;
  private uiSocketServerPort?: number;
  private backendServerPort?: number;
  private YTTabID?: number;
  private externalViewerTabId?: number;

  constructor() {
    this.nativeHost = new NativeHostApi();
    console.log("ChromeBackgroundRuntime initialized 2");

    chrome.runtime.onMessage.addListener(this.handleMessage);
    chrome.runtime.onInstalled.addListener(() => {
      chrome.contextMenus.create({
        id: "configure_mobile_plugin",
        title: "Configure Mobile Plugin",
        contexts: ["action"],
      });
    });
    chrome.action.onClicked.addListener(this.createPage);
    chrome.contextMenus.onClicked.addListener((info) => {
      if (info.menuItemId === "configure_mobile_plugin") {
        this.createMobilePluginSetup().catch((err) => {
          console.error("Failed to create mobile plugin setup window:", err);
        });
      }
    });
    this.createYTTab();
    this.init().catch(console.error);
  }

  private createYTTab() {
    chrome.tabs.create(
      { url: "https://www.youtube.com", pinned: true, active: false },
      (tab) => {
        this.YTTabID = tab.id;
        console.log("YouTube tab created with ID:", this.YTTabID);
      }
    );
  }

  private createPage = () => {
    chrome.tabs.create(
      {
        url: chrome.runtime.getURL("ui/index.html"),
      },
      (tab) => {
        this.externalViewerTabId = tab.id;
        console.log(
          "External viewer tab created with ID:",
          this.externalViewerTabId
        );
      }
    );
  };

  private init = async () => {
    const uiSocketServerPort = await this.nativeHost.postMessageAsync<{
      status: boolean;
      result: number;
    }>({ action: "getUISocketServerPort" });
    const backendServerPort = await this.nativeHost.postMessageAsync<{
      status: boolean;
      result: number;
    }>({ action: "getBackendServerPort" });
    const deviceNetworkIP = await this.nativeHost.postMessageAsync<{
      status: boolean;
      result: string;
    }>({ action: "getDeviceNetworkIp" });

    if (!uiSocketServerPort.status) {
      throw new Error("Failed to get backend settings from native app");
    }

    if (!deviceNetworkIP.status) {
      throw new Error("Failed to get device network IP from native app");
    }

    if (!backendServerPort.status) {
      throw new Error("Failed to get backend server port from native app");
    }

    this.uiSocketServerPort = uiSocketServerPort.result;
    this.deviceNetworkIP = deviceNetworkIP.result;
    this.backendServerPort = backendServerPort.result;
    await this.addBackendSettingsToStorage(
      this.uiSocketServerPort,
      this.backendServerPort,
      this.deviceNetworkIP
    );
  };

  private createMobilePluginSetup = async () => {
    if (!this.uiSocketServerPort || !this.deviceNetworkIP) {
      console.error(
        "Backend settings or device network IP is not available. Cannot create mobile plugin setup."
      );
      return;
    }

    const query = new URLSearchParams({
      deviceIp: this.deviceNetworkIP,
      uiSocketServerPort: this.uiSocketServerPort.toString(),
    });

    chrome.windows.create({
      url: chrome.runtime.getURL("ui/index.html") + "?" + query.toString(),
      type: "popup",
      width: 500,
      height: 600,
    });
  };

  private addBackendSettingsToStorage = async (
    uiSocketServerPort: number,
    backendServerPort: number,
    deviceNetworkIp: string
  ) => {
    await chrome.storage.local.set({
      uiSocketServerPort: uiSocketServerPort,
      backendServerPort: backendServerPort,
      deviceNetworkIp: deviceNetworkIp,
    });
  };

  private handleMessage = (
    message: Record<string, any>,
    sender: chrome.runtime.MessageSender,
    sendResponse: (response?: any) => void
  ) => {
    switch (message.action) {
      case "getYTTabId": {
        sendResponse({ tabId: this.YTTabID });
        break;
      }
      case "enforcementDialogRemoved": {
      }
    }
  };
}
