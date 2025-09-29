import { WebSocketResponse } from "./types/WebSocketResponse";

type WebSocketEvent = "open" | "message" | "close" | "error";

type WebSocketEventMap<TMessage> = {
  open: Event;
  message: TMessage;
  close: CloseEvent;
  error: Event;
};

export class WebSocketClient<T = WebSocketResponse> {
  private ws: WebSocket | null = null;
  private handlers = new Map<WebSocketEvent, Function[]>();
  private queue: string[] = [];
  private reconnect = true;
  private interval = 5000;

  constructor(private url: string) {
    this.connect();
  }

  private connect() {
    this.ws = new WebSocket(this.url);

    this.ws.addEventListener("open", (e) => {
      this.flush();
      this.emit("open", e);
    });

    this.ws.addEventListener("message", (e) => {
      const parsed = JSON.parse((e as MessageEvent).data as string) as T;
      this.emit("message", parsed);
    });

    this.ws.addEventListener("close", (e) => {
      this.emit("close", e);
      if (this.reconnect) setTimeout(() => this.connect(), this.interval);
    });

    this.ws.addEventListener("error", (e) => this.emit("error", e));
  }

  on<K extends WebSocketEvent>(
    event: K,
    handler: (v: WebSocketEventMap<T>[K]) => void
  ) {
    const handlers = this.handlers.get(event) || [];
    handlers.push(handler as any);
    this.handlers.set(event, handlers);
  }

  send(data: WebSocketResponse) {
    if (this.ws && this.ws.readyState === WebSocket.OPEN)
      this.ws.send(JSON.stringify(data));
    else {
      this.queue.push(JSON.stringify(data));
      if (!this.ws || this.ws.readyState === WebSocket.CLOSED) this.connect();
    }
  }

  private flush() {
    while (
      this.queue.length &&
      this.ws &&
      this.ws.readyState === WebSocket.OPEN
    )
      this.ws.send(this.queue.shift()!);
  }

  private emit<K extends WebSocketEvent>(
    event: K,
    payload: WebSocketEventMap<T>[K]
  ) {
    this.handlers.get(event)?.forEach((h) => (h as any)(payload));
  }

  close() {
    this.reconnect = false;
    this.ws?.close();
  }
}
