export type ListenerMap = Record<string, Array<(ev: unknown) => void>>;

export const wsInstances: MockWebSocket[] = [];
export let wsCtorCalls: string[] = [];

export function resetWSMock() {
  wsInstances.length = 0;
  wsCtorCalls = [];
}

export class MockWebSocket {
  static CONNECTING = 0;
  static OPEN = 1;
  static CLOSING = 2;
  static CLOSED = 3;

  url: string;
  readyState = MockWebSocket.CONNECTING;
  sends: string[] = [];
  private _listeners: ListenerMap = {
    open: [],
    message: [],
    close: [],
    error: [],
  };

  constructor(url: string) {
    this.url = url;
    wsInstances.push(this);
    wsCtorCalls.push(url);
  }

  addEventListener(type: keyof ListenerMap, cb: (ev: any) => void) {
    (this._listeners[type] ??= []).push(cb);
  }

  removeEventListener(type: keyof ListenerMap, cb: (ev: any) => void) {
    this._listeners[type] = (this._listeners[type] || []).filter(
      (f) => f !== cb,
    );
  }

  send(payload: string) {
    this.sends.push(payload);
  }

  // --- test helpers to trigger events ---
  open() {
    this.readyState = MockWebSocket.OPEN;
    (this._listeners.open || []).forEach((cb) => cb({}));
  }
  close() {
    this.readyState = MockWebSocket.CLOSED;
    (this._listeners.close || []).forEach((cb) => cb({}));
  }
  error() {
    (this._listeners.error || []).forEach((cb) => cb({}));
  }
  message(data: any) {
    (this._listeners.message || []).forEach((cb) => cb({ data }));
  }
}
