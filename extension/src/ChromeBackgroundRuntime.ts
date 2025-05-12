import { createSocket, SocketInterface } from "./helpers/createSocket";
export class ChromeBackgroundRuntime {
  private clientSocket?: SocketInterface;
  private ytTabs = new Set<number>();
  constructor(){

      this.clientSocket = createSocket(`http://localhost:56546`);
      this.clientSocket.on("PlayEvent", this.handlePlayEvent);
    
    chrome.runtime.onMessage.addListener(this.handleMessage);
  }

  private handlePlayEvent = (data: any) => {
    const firstYtTab = Array.from(this.ytTabs)[0];
      chrome.tabs.sendMessage(firstYtTab, { action: "playEvent", data });
  }

  private sendNativeMessage(data: Record<string, unknown>, callback: (response: any) => void) {
    chrome.runtime.sendNativeMessage("com.ytmediacontroller.app", data, callback);
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
        this.clientSocket?.send({event: "playbackEnded", data: {
          videoId: message.videoId
        }});
        break;
      }
      
      case "playbackStarted": {
        this.clientSocket?.send({event: "playbackStated", data: {videoId: message.videoId}});
        break;
      }
    }
  }

  public sendSocketMessage = (data: Record<string, unknown>) => {
    this.clientSocket?.send(data);
  }

  public sendMessage(data: Record<string, unknown>) {
    chrome.runtime.sendMessage(data);
  }
}