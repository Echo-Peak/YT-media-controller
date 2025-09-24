import { chrome } from "jest-chrome";
import { NativeHostApi } from "./NativeHostApi";
import { makePort } from "./mocks/helpers/makePort";
jest.useFakeTimers();

let nativeHostApi: NativeHostApi;

beforeEach(() => {
  jest.clearAllMocks();
});

afterEach(() => {
  nativeHostApi = null as any;
});

test("postMessageAsync resolves on response", async () => {
  const { port, onMessage, postMessage } = makePort();
  (chrome.runtime.connectNative as jest.Mock).mockReturnValue(port);

  nativeHostApi = new NativeHostApi();
  nativeHostApi.init();

  const p = nativeHostApi.postMessageAsync<{ ok: boolean }>({ action: "ping" });
  expect(postMessage).toHaveBeenCalledWith({ action: "ping" });
  onMessage.dispatch({ ok: true });
  await expect(p).resolves.toEqual({ ok: true });
});

test("postMessageAsync rejects if not connected", async () => {
  (chrome.runtime.connectNative as jest.Mock).mockReturnValue(undefined as any);

  nativeHostApi = new NativeHostApi();
  nativeHostApi.init();

  await expect(nativeHostApi.postMessageAsync({ action: "x" })).rejects.toThrow(
    "Not connected to native host"
  );
});

test("reconnect scheduled on disconnect", () => {
  const first = makePort();
  const second = makePort();
  (chrome.runtime.connectNative as jest.Mock)
    .mockReturnValueOnce(first.port)
    .mockReturnValueOnce(second.port);

  nativeHostApi = new NativeHostApi();
  nativeHostApi.init();

  first.onDisconnect.dispatch(first.port);
  jest.advanceTimersByTime(1000);
  expect(chrome.runtime.connectNative).toHaveBeenCalledTimes(2);
});
