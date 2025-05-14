import { createSocket, SocketInterface } from "./helpers/createSocket";
import { sendNativeMessage } from "./helpers/sendNativeMessage";
import { BackendSettings } from "./types/BackendSettings";
export class ChromeBackgroundRuntime {
  private clientSocket?: SocketInterface;
  private ytTabs = new Set<number>();
  private readonly nativeAppId = "com.ytmediacontroller.app";

  constructor(){
    this.init().catch(console.error)
    chrome.runtime.onMessage.addListener(this.handleMessage);
  }

  private setupCLientSocket = (port: string) => {
    this.clientSocket = createSocket(`http://localhost:${port}`);
    this.clientSocket.on("PlayEvent", this.handlePlayEvent);
    this.clientSocket.on("error", (error) => {
      console.error("Socket error:", error);
      this.clientSocket = undefined;
    });
    console.log("Client socket created");
  }

  private init = async () => {
    const backendSettings = await sendNativeMessage<{status: boolean} & BackendSettings>(this.nativeAppId, {action: 'getBackendSettings'});
    const deviceNetworkIP = await sendNativeMessage<{status: boolean, deviceNetworkIp: string}>(this.nativeAppId, {action: 'getDeviceNetworkIp'});

    if(!backendSettings.status){
      throw new Error("Failed to get backend settings from native app");
    }
    
    if(!deviceNetworkIP.status){
      throw new Error("Failed to get device network IP from native app");
    }

    this.setupCLientSocket(backendSettings.backendServerPort);
    chrome.storage.local.set({ 
      backendServerPort: backendSettings.backendServerPort,
      controlServerPort: backendSettings.controlServerPort,
      deviceNetworkIp: deviceNetworkIP.deviceNetworkIp
    });
  }

  private handlePlayEvent = (data: any) => {
    const firstYtTab = Array.from(this.ytTabs)[0];
      chrome.tabs.sendMessage(firstYtTab, { action: "playEvent", data });
  }

  private getFromLocalStorage = (key: string | string[], callback: (value: Record<string, unknown>) => void) => {
      chrome.storage.local.get(Array.isArray(key) ? key : [key], (result) => {
        callback(result);
      });
  }

  private handleMessage = (message: Record<string, string | number>, sender: chrome.runtime.MessageSender, sendResponse: (response?: any) => void) => {
    switch (message.action) {
      case "getDeviceNetworkIp": {
        this.getFromLocalStorage(["deviceNetworkIp", "backendServerPort"], sendResponse);
        return true;
      }
      case "getControlServerPort": {
        this.getFromLocalStorage("controlServerPort", sendResponse);
        return true;
      }
      case "getBackendServerPort": {
        this.getFromLocalStorage("backendServerPort", sendResponse);
        return true;
      }

      case "updateBackendServerPort": {
        sendNativeMessage<{status: boolean, message?: string, error?: string}>(this.nativeAppId, { action: "updateBackendServerPort",  port: message.port  })
        .then((response)=> {
          sendResponse({status: response.status, message: response.message, error: response.error});
        }).catch(err => {
          sendResponse({status: false, error: err.message});
        })
        return true;
      }
      case "updateControlServerPort": {
        sendNativeMessage<{status: boolean, message?: string, error?: string}>(this.nativeAppId, { action: "updateControlServerPort",  port: message.port  })
        .then((response)=> {
          sendResponse({status: response.status, message: response.message, error: response.error});
        }).catch(err => {
          sendResponse({status: false, error: err.message});
        })
        return true;
      }
      case "ytFrontendRuntimeLoaded": {
        this.ytTabs.add(message.tabId as number);
        break;
      }
      case "playbackEnded":{
        this.sendSocketMessage({event: "playbackEnded", data: {
          videoId: message.videoId
        }});
        break;
      }
      
      case "playbackStarted": {
        this.sendSocketMessage({event: "playbackStated", data: {videoId: message.videoId}});
        break;
      }
    }
  }

  private waitForClientSocket = async (): Promise<SocketInterface | undefined> => {
    const maxWaitTime = 30000;
    const interval = 100;
    let elapsedTime = 0;

    return new Promise((resolve) => {
      const checkSocket = () => {
        if (this.clientSocket) {
          resolve(this.clientSocket);
        } else if (elapsedTime >= maxWaitTime) {
          resolve(undefined);
        } else {
          elapsedTime += interval;
          setTimeout(checkSocket, interval);
        }
      };
      checkSocket();
    });
  };

  public sendSocketMessage = async (data: Record<string, unknown>) => {
    const socket = await this.waitForClientSocket();
    if (socket) {
      socket.send(data);
    } else {
      console.error("Client socket is not available after waiting for 30 seconds.");
    }
  };

  public sendMessage(data: Record<string, unknown>) {
    chrome.runtime.sendMessage(data);
  }
}