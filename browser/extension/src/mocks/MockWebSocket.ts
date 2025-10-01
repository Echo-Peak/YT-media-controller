export class MockWebSocket {
  static instances: MockWebSocket[] = [];
  static CONNECTING = 0;
  static OPEN = 1;
  static CLOSING = 2;
  static CLOSED = 3;
  static onSend: ((ws: MockWebSocket, data: any) => void) | null = null;

  url: string;
  readyState = MockWebSocket.CONNECTING;
  sends: string[] = [];
  listeners = new Map<string, Function[]>();

  constructor(url: string) {
    this.url = url;
    MockWebSocket.instances.push(this);
  }

  addEventListener(type: string, handler: any) {
    const list = this.listeners.get(type) || [];
    list.push(handler);
    this.listeners.set(type, list);
  }

  dispatch(type: string, event: any = {}) {
    (this.listeners.get(type) || []).forEach((h) => h(event));
  }

  open() {
    this.readyState = MockWebSocket.OPEN;
    this.dispatch("open", new Event("open"));
  }

  message(data: any) {
    const payload = typeof data === "string" ? data : JSON.stringify(data);
    this.dispatch("message", { data: payload } as MessageEvent);
  }

  error(e: any = new Event("error")) {
    this.dispatch("error", e);
  }

  close(code = 1000, reason = "close") {
    this.readyState = MockWebSocket.CLOSED;
    this.dispatch("close", { code, reason } as CloseEvent);
  }

  send(data: string) {
    this.sends.push(data);
    if (MockWebSocket.onSend) MockWebSocket.onSend(this, data);
  }
}
