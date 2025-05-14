import { NativeHostApi } from "./NativeHostApi";
import { BackendSettings } from "./types/BackendSettings";
export class ChromeBackgroundRuntime {
  private ytTabs = new Set<number>();
  private nativeHost: NativeHostApi;
  
  constructor(){
    this.nativeHost = new NativeHostApi();
    this.nativeHost.on("PlayEvent", this.handlePlayEvent);
    
    chrome.runtime.onMessage.addListener(this.handleMessage);
    this.init();
  }

  private init = async () => {
    const backendSettings = await this.nativeHost.postMessageAsync<{status: boolean} & BackendSettings>({action: 'getBackendSettings'});
    const deviceNetworkIP = await this.nativeHost.postMessageAsync<{status: boolean, deviceNetworkIp: string}>( {action: 'getDeviceNetworkIp'});

    if(!backendSettings.status){
      throw new Error("Failed to get backend settings from native app");
    }
    
    if(!deviceNetworkIP.status){
      throw new Error("Failed to get device network IP from native app");
    }

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
        this.nativeHost.postMessageAsync<{status: boolean, message?: string, error?: string}>({ action: "updateBackendServerPort",  data: {port: message.port}  })
        .then((response)=> {
          sendResponse({status: response.status, message: response.message, error: response.error});
        }).catch(err => {
          sendResponse({status: false, error: err.message});
        })
        return true;
      }
      case "updateControlServerPort": {
        this.nativeHost.postMessageAsync<{status: boolean, message?: string, error?: string}>({ action: "updateControlServerPort",  data: {port: message.port}  })
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
        this.nativeHost.postMessage({action: "playbackEnded", data: { videoId: message.videoId } });
        break;
      }
      
      case "playbackStarted": {
        this.nativeHost.postMessage({action: "playbackStated", data: { videoId: message.videoId }});
        break;
      }
    }
  }

  public sendMessage(data: Record<string, unknown>) {
    chrome.runtime.sendMessage(data);
  }
}