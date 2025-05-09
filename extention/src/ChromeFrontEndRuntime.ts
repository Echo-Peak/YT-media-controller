import "@types/chrome";


export class ChromeFrontEndRuntime {
  constructor(){
    chrome.runtime.sendMessage({action: "ytFrontendRuntimeLoaded"});
    chrome.runtime.onMessage.addListener(this.handleMessage);
  }

  private handleMessage = (message: any, sender: chrome.runtime.MessageSender, sendResponse: (response?: any) => void) => {
    switch (message.action) {
      case "PlayEvent": {
        window.location.href = message.data.url;
        break;
      }
    }
  }

  public sendVideoEndedEvent = (videoId: string) => {
    chrome.runtime.sendMessage({action: "playbackElapsed", data:{
      videoId,
    }});
  }

  public sendVideoStartedEvent = (videoId: string) => {
    chrome.runtime.sendMessage({action: "playbackElapsed" ,data: {
      videoId,
    }});
  }

    public sendVideoElapsedTimeEvent = (videoId: string, elapsedTime: string, duration: string) => {
    chrome.runtime.sendMessage({action: "playbackElapsed", data:{
      videoId,
      elapsedTime,
      duration,
    }});
  }
}