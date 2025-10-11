import { BackendService } from './BackendService';

class MockWebSocket extends EventTarget {
  static instances: MockWebSocket[] = [];
  static CONNECTING = 0;
  static OPEN = 1;
  static CLOSING = 2;
  static CLOSED = 3;

  url: string;
  readyState = MockWebSocket.CONNECTING;
  sent: string[] = [];

  constructor(url: string) {
    super();
    this.url = url;
    MockWebSocket.instances.push(this);
  }

  send(data: string) {
    this.sent.push(data);
  }

  open() {
    this.readyState = MockWebSocket.OPEN;
    this.dispatchEvent(new Event('open'));
  }

  close() {
    this.readyState = MockWebSocket.CLOSED;
    this.dispatchEvent(new Event('close'));
  }

  error() {
    this.dispatchEvent(new Event('error'));
  }

  message(data: string) {
    const ev = new Event('message') as any;
    ev.data = data;
    this.dispatchEvent(ev);
  }

  addEventListener(type: string, listener: EventListenerOrEventListenerObject) {
    return super.addEventListener(type, listener);
  }
}

describe('BackendService', () => {
  const realWS = global.WebSocket;

  beforeAll(() => {
    global.WebSocket = MockWebSocket as unknown as typeof WebSocket;
  });

  afterAll(() => {
    global.WebSocket = realWS;
  });

  beforeEach(() => {
    jest.useFakeTimers();
    jest.spyOn(console, 'warn').mockImplementation(() => {});
    MockWebSocket.instances = [];
    jest.clearAllTimers();
    jest.clearAllMocks();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  test('does not connect when port is falsy', () => {
    const svc = new BackendService(0 as unknown as number);
    svc.init();
    expect(MockWebSocket.instances.length).toBe(0);
  });

  test('connects to expected URL', () => {
    const svc = new BackendService(1234);
    svc.init();
    expect(MockWebSocket.instances.length).toBe(1);
    expect(MockWebSocket.instances[0].url).toBe(
      'ws://localhost:1234/externalViewer',
    );
  });

  test('queues messages until socket opens and then flushes', () => {
    const svc = new BackendService(3000);
    svc.init();
    const ws = MockWebSocket.instances[0];

    svc.sendMessage({ action: 'ping', data: { x: 1 } });
    expect(console.warn).toHaveBeenCalled();
    expect(ws.sent).toHaveLength(0);

    ws.open();
    expect(ws.sent).toEqual([
      JSON.stringify({ action: 'ping', data: { x: 1 } }),
    ]);
  });

  test('sends immediately when socket is already open', () => {
    const svc = new BackendService(3001);
    svc.init();
    const ws = MockWebSocket.instances[0];
    ws.open();

    svc.sendMessage({ action: 'hello' });
    expect(ws.sent).toEqual([JSON.stringify({ action: 'hello' })]);
  });

  test('forwards parsed message data to listeners', () => {
    const svc = new BackendService(3002);
    const cb = jest.fn();
    svc.onData(cb);
    svc.init();
    const ws = MockWebSocket.instances[0];
    ws.open();

    ws.message(JSON.stringify({ a: 1, b: 'x' }));
    expect(cb).toHaveBeenCalledWith({ a: 1, b: 'x' });
  });

  test('ignores invalid JSON messages', () => {
    const svc = new BackendService(3003);
    const cb = jest.fn();
    svc.onData(cb);
    svc.init();
    const ws = MockWebSocket.instances[0];
    ws.open();

    ws.message('not-json');
    expect(cb).not.toHaveBeenCalled();
  });

  test('offData unsubscribes a listener', () => {
    const svc = new BackendService(3004);
    const cb1 = jest.fn();
    const cb2 = jest.fn();
    svc.onData(cb1);
    svc.onData(cb2);
    svc.offData(cb1);

    svc.init();
    const ws = MockWebSocket.instances[0];
    ws.open();

    ws.message(JSON.stringify({ z: 9 }));
    expect(cb1).not.toHaveBeenCalled();
    expect(cb2).toHaveBeenCalledWith({ z: 9 });
  });

  test('schedules reconnect on close', () => {
    const svc = new BackendService(3005);
    svc.init();
    const first = MockWebSocket.instances[0];
    first.close();

    expect(MockWebSocket.instances.length).toBe(1);
    jest.advanceTimersByTime(4999);
    expect(MockWebSocket.instances.length).toBe(1);
    jest.advanceTimersByTime(1);
    expect(MockWebSocket.instances.length).toBe(2);
  });

  test('schedules reconnect on error', () => {
    const svc = new BackendService(3006);
    svc.init();
    const first = MockWebSocket.instances[0];
    first.error();

    jest.advanceTimersByTime(5000);
    expect(MockWebSocket.instances.length).toBe(2);
  });

  test('flushes queued messages after reconnect opens', () => {
    const svc = new BackendService(3007);
    svc.init();
    const ws1 = MockWebSocket.instances[0];

    svc.sendMessage({ action: 'a' });
    ws1.error();
    jest.advanceTimersByTime(5000);

    const ws2 = MockWebSocket.instances[1];
    expect(ws2.sent).toHaveLength(0);
    ws2.open();
    expect(ws2.sent).toEqual([JSON.stringify({ action: 'a' })]);
  });
});
