import { BackendSettingsMessage } from "./types/BackendSettingsMessage";
import { WebSocketResponse } from "./types/WebSocketResponse";
import { WebSocketClient } from "./WebSocketClient";
import { WindowManager } from "./WindowManager";

export class ChromeBackgroundRuntime {
  private deviceNetworkIP?: string;
  private uiSocketServerPort?: number;
  private backendServerPort?: number;
  private wsClient?: WebSocketClient;
  private defaultWsPort = 52000;
  private windowManager = new WindowManager();

  constructor() {
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

    chrome.tabs.onRemoved.addListener(this.removeTabReference);
    chrome.windows.onRemoved.addListener(this.handleWindowClosed);
  }

  private removeTabReference = (tabId: number) => {
    const main = WindowManager.windows.main;
    this.windowManager.removeTab(main, WindowManager.tabs.externalViewer);
    this.windowManager.removeTab(main, WindowManager.tabs.youtube);
  };

  private handleWindowClosed = (windowId: number) => {
    this.windowManager.removeWindowById(windowId);
  };

  private createYTTab() {
    this.windowManager.addTabToWindow(
      WindowManager.windows.main,
      WindowManager.tabs.youtube,
      {
        url: "https://www.youtube.com",
        pinned: true,
        active: false,
      }
    );
  }

  private createPage = () => {
    this.windowManager
      .has(WindowManager.windows.main, WindowManager.tabs.externalViewer)
      .then((tabExists) => {
        if (tabExists) {
          this.windowManager.update(
            WindowManager.windows.main,
            WindowManager.tabs.externalViewer,
            { active: true }
          );

          console.log(
            "External viewer tab already exists, not creating a new one."
          );
          return;
        }

        this.windowManager.addTabToWindow(
          WindowManager.windows.main,
          WindowManager.tabs.externalViewer,
          {
            url: chrome.runtime.getURL("ui/index.html"),
            active: true,
          }
        );
      })
      .catch((err) => console.error("Error checking for existing tab:", err));
  };

  public init = async () => {
    try {
      const cachedWsPort = await chrome.storage.local.get("uiSocketServerPort");
      await this.attemptReconnect(cachedWsPort.uiSocketServerPort!);
    } catch (error) {
      throw new Error(`Failed to initialize WebSocket connection: ${error}`);
    }
  };

  private createSocketConnection = (options: {
    userDefinedPort?: number;
    defaultPort: number;
  }) => {
    const portToUse = options.userDefinedPort || options.defaultPort;
    this.uiSocketServerPort = portToUse;
    const wsUrl = `ws://localhost:${portToUse}/chromeBackend`;
    return new Promise<boolean>((resolve, reject) => {
      this.wsClient = new WebSocketClient<WebSocketResponse>(wsUrl);
      this.wsClient.on("open", () => {
        console.log(`WebSocket connected to ${wsUrl}`);
        resolve(true);
      });
      this.wsClient.on("message", (event: WebSocketResponse) => {
        try {
          if (event.Action === "backendSettings") {
            const settings = event.Data as BackendSettingsMessage;
            console.log("Received backend settings:", settings);
            this.deviceNetworkIP = settings.DeviceNetworkIp;
            this.backendServerPort = settings.BackendServerPort;
            this.uiSocketServerPort = settings.UISocketServerPort;
            this.windowManager.update(
              WindowManager.windows.mobile,
              WindowManager.tabs.mobileSetup,
              {
                url: this.createMobileConfigUrl(),
              }
            );
          }
        } catch (err) {
          console.error("Failed to parse WebSocket message:", err);
        }
      });
      this.wsClient.on("close", () => {
        console.warn(`WebSocket disconnected from ${wsUrl}`);
      });
      this.wsClient.on("error", (event) => {
        console.error("WebSocket error:", event);
        reject(new Error(`WebSocket error: ${event}`));
      });
      this.wsClient.send({
        Action: "getBackendSettings",
        Data: {},
      });
    });
  };

  private createMobileConfigUrl = () => {
    const deviceIp = this.deviceNetworkIP || "";
    const devicePort = this.backendServerPort || "";
    const uiSocketServerPort = this.uiSocketServerPort || "";

    const query = new URLSearchParams({
      deviceIp: deviceIp,
      devicePort: devicePort.toString(),
      uiSocketServerPort: uiSocketServerPort.toString(),
    });
    return chrome.runtime.getURL("ui/index.html") + "?" + query.toString();
  };

  private createMobilePluginSetup = async () => {
    const tabExists = await this.windowManager.has(
      WindowManager.windows.mobile,
      WindowManager.tabs.mobileSetup
    );

    if (tabExists) {
      this.windowManager.update(
        WindowManager.windows.mobile,
        WindowManager.tabs.mobileSetup,
        { active: true }
      );
    } else {
      this.windowManager.createWindow(
        WindowManager.windows.mobile,
        WindowManager.tabs.mobileSetup,
        {
          url: this.createMobileConfigUrl(),
          type: "popup",
          width: 500,
          height: 600,
        }
      );
    }
  };

  private openInYTTab = async (url: string) => {
    const tabExists = await this.windowManager.has(
      WindowManager.windows.main,
      WindowManager.tabs.youtube
    );

    if (tabExists) {
      this.windowManager.update(
        WindowManager.windows.main,
        WindowManager.tabs.youtube,
        { url: url, active: true }
      );
    } else {
      console.warn("YouTube tab does not exist, creating a new one.");
      this.createYTTab();
    }
  };

  private setFullScreen = async () => {
    this.windowManager.updateWindow(WindowManager.windows.main, {
      state: "fullscreen",
    });
  };

  private setExitFullScreen = async () => {
    this.windowManager.updateWindow(WindowManager.windows.main, {
      state: "normal",
    });
  };

  private attemptReconnect = async (userDefinedPort: number) => {
    console.log("Attempting to reconnect with port:", userDefinedPort);
    try {
      await this.createSocketConnection({
        userDefinedPort,
        defaultPort: this.defaultWsPort,
      });

      console.log("Storing UI Socket Server Port:", this.uiSocketServerPort);

      await chrome.storage.local.set({
        uiSocketServerPort: this.uiSocketServerPort,
      });
      this.uiSocketServerPort = userDefinedPort;

      this.windowManager.update(
        WindowManager.windows.mobile,
        WindowManager.tabs.mobileSetup,
        {
          url: this.createMobileConfigUrl(),
        }
      );

      console.log("Reconnection successful");
    } catch (error) {
      console.error("Reconnection attempt failed:", error);
    }
  };

  private handleMessage = (
    message: Record<string, any>,
    sender: chrome.runtime.MessageSender,
    sendResponse: (response?: any) => void
  ) => {
    switch (message.action) {
      case "openInYTTab": {
        this.openInYTTab(message.data.url).catch(console.error);
        break;
      }
      case "setFullScreen": {
        this.setFullScreen().catch(console.error);
        break;
      }
      case "setExitFullScreen": {
        this.setExitFullScreen().catch(console.error);
        break;
      }
      case "updateUiSocketServerPort": {
        const newPort = parseInt(message.data.uiSocketServerPort);
        console.log("Updating UI Socket Server Port to:", newPort);
        this.attemptReconnect(newPort).catch(console.error);
        break;
      }
    }
  };
}
