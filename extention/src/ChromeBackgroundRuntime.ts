import settings from "./settings.json";
import { createSocket, SocketInterface } from "./helpers/createSocket";
export class ChromeBackgroundRuntime {
  private clientSocket?: SocketInterface;
  private ytTabs = new Set<number>();
  constructor(){

      this.clientSocket = createSocket(`http://localhost:${settings.socketPort}`);
      this.clientSocket.on("PlayEvent", this.handlePlayEvent);
    
    chrome.runtime.onMessage.addListener(this.handleMessage);
  }

  private handlePlayEvent = (data: any) => {
    const firstYtTab = Array.from(this.ytTabs)[0];
      chrome.tabs.sendMessage(firstYtTab, { action: "playEvent", data });
  }

  private handleMessage = (message: any, sender: chrome.runtime.MessageSender, sendResponse: (response?: any) => void) => {
    switch (message.action) {
      case "ytFrontendRuntimeLoaded": {
        this.ytTabs.add(message.tabId);
        break;
      }
      case "playbackEnded":{
        this.clientSocket?.send({event: "playbackEnded", data: {
          videoId: message.videoId
        }});
        break;
      }
      case "playbackElapsed":{
        this.clientSocket?.send({event: "playbackElapsed", data: {
          videoId: message.videoId, 
          elapsedTime: message.elapsedTime,
          duration: message.duration,
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