// /* eslint-disable @typescript-eslint/no-unsafe-member-access */
// /* eslint-disable @typescript-eslint/no-unsafe-call */
// /* eslint-disable @typescript-eslint/no-unsafe-assignment */
// /* eslint-disable @typescript-eslint/unbound-method */
// jest.useFakeTimers();
// import {
//   MockWebSocket,
//   wsInstances,
//   wsCtorCalls,
//   resetWSMock,
// } from './mocks/MockWebSocket';

// const origWebSocket = global.WebSocket as any;

// beforeEach(() => {
//   resetWSMock();
//   jest.clearAllMocks();
//   delete process.env.VITE_API_SERVER_PORT;
// });

// afterEach(() => {
//   jest.restoreAllMocks();
// });

// beforeAll(() => {
//   Object.defineProperty(global, 'WebSocket', {
//     value: MockWebSocket,
//     writable: true,
//   });
// });

// afterAll(() => {
//   Object.defineProperty(global, 'WebSocket', { value: origWebSocket });
// });

// jest.mock('../helpers/getChromeStorageKeys', () => ({
//   getChromeStorageKeys: jest.fn(),
// }));

// const { getChromeStorageKeys } = jest.requireMock(
//   '../helpers/getChromeStorageKeys',
// );
// const componentNamespace = 'externalViewer';

// describe('BackendService (original API)', () => {
//   test('init uses uiSocketServerPort from chrome storage and connects', async () => {
//     getChromeStorageKeys.mockResolvedValue({ uiSocketServerPort: 8081 });
//     const addListenerSpy = jest.spyOn(chrome.runtime.onMessage, 'addListener');

//     const { BackendService } = await import('./BackendService');
//     const svc = new BackendService(5000);
//     svc.init();

//     expect(wsCtorCalls[0]).toBe(`ws://localhost:8081/${componentNamespace}`);
//     expect(addListenerSpy).toHaveBeenCalledTimes(1);
//   });

//   test('sendData queues before socket open, then flushes on open', async () => {
//     getChromeStorageKeys.mockResolvedValue({ uiSocketServerPort: 9001 });
//     const { BackendService } = await import('./BackendService');
//     const svc = new BackendService();
//     await svc.init();

//     const ws = wsInstances[0];
//     svc.sendData({ a: 1 });
//     svc.sendData({ b: 2 });
//     expect(ws.sends).toHaveLength(0);

//     ws.open();
//     expect(ws.sends.map((s) => JSON.parse(s))).toEqual([{ a: 1 }, { b: 2 }]);
//   });

//   test('chrome.runtime messages are queued, then flushed on open', async () => {
//     getChromeStorageKeys.mockResolvedValue({ uiSocketServerPort: 9100 });

//     const addListenerSpy = jest.spyOn(chrome.runtime.onMessage, 'addListener');

//     const { BackendService } = await import('./BackendService');
//     const svc = new BackendService();
//     await svc.init();

//     const relay = addListenerSpy.mock.calls[0][0];
//     const ws = wsInstances[0];

//     relay({ action: 'ping', data: { x: 1 } }, {} as any, () => {});
//     expect(ws.sends).toHaveLength(0);

//     ws.open();
//     expect(JSON.parse(ws.sends[0])).toEqual({ action: 'ping', data: { x: 1 } });
//   });

//   test('onData listeners receive parsed messages; offData stops them', async () => {
//     getChromeStorageKeys.mockResolvedValue({ uiSocketServerPort: 9200 });

//     const { BackendService } = await import('./BackendService');
//     const svc = new BackendService();
//     await svc.init();

//     const ws = wsInstances[0];
//     ws.open();

//     const cb = jest.fn();
//     svc.onData(cb);

//     ws.message(JSON.stringify({ hello: 'world' }));
//     expect(cb).toHaveBeenCalledWith({ hello: 'world' });

//     cb.mockClear();
//     svc.offData(cb);
//     ws.message(JSON.stringify({ again: true }));
//     expect(cb).not.toHaveBeenCalled();
//   });

//   test('close triggers reconnect after 5s (single reconnect)', async () => {
//     getChromeStorageKeys.mockResolvedValue({ uiSocketServerPort: 9300 });

//     const { BackendService } = await import('./BackendService');
//     const svc = new BackendService();
//     await svc.init();

//     expect(wsCtorCalls).toHaveLength(1);
//     wsInstances[0].close();

//     jest.advanceTimersByTime(4999);
//     expect(wsCtorCalls).toHaveLength(1);

//     jest.advanceTimersByTime(1);
//     expect(wsCtorCalls).toHaveLength(2);
//     expect(wsCtorCalls[1]).toBe(`ws://localhost:9300/${componentNamespace}`);
//   });

//   test('error triggers reconnect after 5s', async () => {
//     getChromeStorageKeys.mockResolvedValue({ uiSocketServerPort: 9400 });

//     const { BackendService } = await import('./BackendService');
//     const svc = new BackendService();
//     await svc.init();

//     wsInstances[0].error();
//     jest.advanceTimersByTime(5000);
//     expect(wsCtorCalls).toHaveLength(2);
//   });

//   test('multiple close/error before timer elapses → only one reconnect', async () => {
//     getChromeStorageKeys.mockResolvedValue({ uiSocketServerPort: 9500 });

//     const { BackendService } = await import('./BackendService');
//     const svc = new BackendService();
//     await svc.init();

//     const ws = wsInstances[0];
//     ws.close();
//     ws.error();
//     ws.close();

//     jest.advanceTimersByTime(5000);
//     expect(wsCtorCalls).toHaveLength(2);
//   });

//   test('pending buffer flushed exactly once on open', async () => {
//     getChromeStorageKeys.mockResolvedValue({ uiSocketServerPort: 9600 });

//     const { BackendService } = await import('./BackendService');
//     const svc = new BackendService();
//     await svc.init();

//     const ws = wsInstances[0];
//     svc.sendData({ a: 1 });
//     svc.sendData({ b: 2 });

//     ws.open();
//     expect(ws.sends.map((s) => JSON.parse(s))).toEqual([{ a: 1 }, { b: 2 }]);

//     ws.open();
//     expect(ws.sends.map((s) => JSON.parse(s))).toEqual([{ a: 1 }, { b: 2 }]);
//   });

//   test('sendData sends immediately when socket is OPEN', async () => {
//     getChromeStorageKeys.mockResolvedValue({ uiSocketServerPort: 9700 });
//     const { BackendService } = await import('./BackendService');
//     const svc = new (BackendService as any)();
//     await svc.init();

//     const ws = wsInstances[0];
//     ws.open();
//     svc.sendData({ z: 9 });
//     expect(JSON.parse(ws.sends[0])).toEqual({ z: 9 });
//   });
// });
