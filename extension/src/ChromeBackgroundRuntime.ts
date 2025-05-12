import { createSocket, SocketInterface } from "./helpers/createSocket";
export class ChromeBackgroundRuntime {
  private clientSocket?: SocketInterface;
  private ytTabs = new Set<number>();
  private readonly nativeAppId = "com.ytmediacontroller.app";
  constructor(){
    this.init();
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

  private init = () => {
    chrome.storage.local.get(["backendServerPort", "controlServerPort"], (result) => {
      if(result){
        const { backendServerPort } = result;
        this.setupCLientSocket(backendServerPort);
      }else{
        this.sendNativeMessage({action: 'getSettings'}, (response)=> {
          if(response){
            this.setupCLientSocket(response.backendServerPort);
            chrome.storage.local.set({ 
              backendServerPort: response.backendServerPort,
              controlServerPort: response.controlServerPort,
            });
          }
        });
      }
    });
  }

  private handlePlayEvent = (data: any) => {
    const firstYtTab = Array.from(this.ytTabs)[0];
      chrome.tabs.sendMessage(firstYtTab, { action: "playEvent", data });
  }

  private sendNativeMessage(data: Record<string, unknown>, callback: (response: any) => void) {
    chrome.runtime.sendNativeMessage(this.nativeAppId, data, callback);
  }

  private handleMessage = (message: Record<string, string | number>, sender: chrome.runtime.MessageSender, sendResponse: (response?: any) => void) => {
    switch (message.action) {
      case "getBackendSettings":{
        chrome.storage.local.get(["backendServerPort", "controlServerPort"], (result) => {
          const { backendServerPort, controlServerPort } = result;
          sendResponse({ backendServerPort, controlServerPort });
        });
        return true;
      }

      case "updateBackendServerPort": {
        this.sendNativeMessage({ action: "updateBackendServerPort", port: message.port  }, sendResponse);
        return true;
      }
      case "updateControlServerPort": {
        this.sendNativeMessage({ action: "updateControlServerPort", port: message.port  }, sendResponse);
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