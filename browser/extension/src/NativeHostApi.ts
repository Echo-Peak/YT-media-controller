export type NativeHostMessage = {
  action: string;
  data?: Record<string, unknown>;
};

export class NativeHostApi {
  private readonly nativeAppId = "com.ytmediacontroller.app";
  private nativeHost?: chrome.runtime.Port;
  private eventListeners: Record<string, Array<(msg: NativeHostMessage) => void>> = {};

  constructor() {
    this.attemptReconnect();
  }

  private attemptReconnect = () => {
    this.nativeHost = chrome.runtime.connectNative(this.nativeAppId);
    this.nativeHost.onMessage.addListener(this.handleMessage);
    this.nativeHost.onDisconnect.addListener(this.handleDisconnect);
  };

  private handleMessage = (msg: NativeHostMessage) => {
    if(msg.action){
      this.emitMessage(msg.action, msg.data);
    }
  };

  private emitMessage = (action: string, data?: Record<string, unknown>) => {
    const listeners = this.eventListeners[action];
    if (listeners) {
      for (const listener of listeners) {
        listener({ action, data });
      }
    }
  }

  private handleDisconnect = () => {
    console.error("Disconnected from native host");
    this.nativeHost?.onDisconnect.removeListener(this.handleDisconnect);
    this.nativeHost?.onMessage.removeListener(this.handleMessage);
    this.nativeHost = undefined;
    setTimeout(this.attemptReconnect, 1000);
  };

  public postMessage = (message: NativeHostMessage) => {
    if (!this.nativeHost) {
      console.error("Not connected to native host");
      return;
    }
    this.nativeHost.postMessage(message);
  };

  public postMessageAsync = <T>(message: NativeHostMessage): Promise<T> => {
    return new Promise((resolve, reject) => {
      if (!this.nativeHost) {
        reject(new Error("Not connected to native host"));
        return;
      }

      const listener = (response: T) => {
        this.nativeHost?.onMessage.removeListener(listener);
        resolve(response);
      };

      this.nativeHost.onMessage.addListener(listener);
      try {
        this.nativeHost.postMessage(message);
      } catch (error) {
        this.nativeHost.onMessage.removeListener(listener);
        reject(error);
      }
    });
  };

  public on = (event: string, callback: (msg: NativeHostMessage) => void) => {
    if (!this.eventListeners[event]) {
      this.eventListeners[event] = [];
    }
    this.eventListeners[event].push(callback);
  };
}
