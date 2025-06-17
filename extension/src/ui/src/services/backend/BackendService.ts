import { getChromeStorageKeys } from '../helpers/getChromeStorageKeys';

type DataCallback = (data: Record<string, unknown>) => void;

export class BackendService {
  private socket: WebSocket | null = null;
  private dataListeners: Set<DataCallback> = new Set();
  private port: string | number | undefined;
  private reconnectInterval: number = 5000;
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null;
  private pendingMessages: Record<string, unknown>[] = [];

  private constructor(port: string | number | undefined) {
    this.port = port;
    this.connect();
  }

  static async init(): Promise<BackendService> {
    const { uiSocketServerPort } = await getChromeStorageKeys();
    const port = uiSocketServerPort || process.env.REACT_APP_API_SERVER_PORT;
    return new BackendService(port);
  }

  private connect() {
    if (!this.port) return;
    this.socket = new WebSocket(`ws://localhost:${this.port}`);

    this.socket.addEventListener('open', () => {
      if (this.reconnectTimer) {
        clearTimeout(this.reconnectTimer);
        this.reconnectTimer = null;
      }

      while (this.pendingMessages.length) {
        this.socket!.send(JSON.stringify(this.pendingMessages.shift()!));
      }
      this.pendingMessages = [];
    });

    this.socket.addEventListener('message', (event) => {
      try {
        const data = JSON.parse(event.data);
        this.dataListeners.forEach((cb) => cb(data));
      } catch (e) {
        // Optionally handle parse error
      }
    });

    this.socket.addEventListener('close', () => {
        this.scheduleReconnect();
    });

    this.socket.addEventListener('error', () => {
      this.scheduleReconnect();
    });
  }

  private scheduleReconnect() {
    if(this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
    }
    this.reconnectTimer = setTimeout(() => {
      console.warn('Reconnecting to backend service...');
      this.connect();
    }, this.reconnectInterval);
  }

  onData(callback: DataCallback) {
    this.dataListeners.add(callback);
  }

  offData(callback: DataCallback) {
    this.dataListeners.delete(callback);
  }

  sendData(data: Record<string, unknown>) {

    if (this.socket && this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(JSON.stringify(data));
    }else{
      console.warn('WebSocket is not open, queuing message:', data);
      this.pendingMessages.push(data);
    }
  }
}