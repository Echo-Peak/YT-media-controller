let postMessageAsyncMock: jest.Mock;

jest.mock("./NativeHostApi", () => {
  postMessageAsyncMock = jest.fn();
  return {
    NativeHostApi: jest.fn().mockImplementation(() => ({
      postMessageAsync: postMessageAsyncMock,
      postMessage: jest.fn(),
      on: jest.fn(),
      init: jest.fn(),
    })),
  };
});

import { ChromeBackgroundRuntime } from "./ChromeBackgroundRuntime";

let runtime: ChromeBackgroundRuntime;
const deviceIp = "10.0.0.2";
const uiSocketServerPort = 1234;
const backendServerPort = 5678;
const extensionId = "abcdefghijklmnopabcdefghijklmnop";

beforeEach(() => {
  runtime = new ChromeBackgroundRuntime();
  jest.clearAllMocks();
  postMessageAsyncMock.mockImplementation(({ action }) => {
    if (action === "getUISocketServerPort")
      return Promise.resolve({ status: true, result: 1234 });
    if (action === "getBackendServerPort")
      return Promise.resolve({ status: true, result: 5678 });
    if (action === "getDeviceNetworkIp")
      return Promise.resolve({ status: true, result: "10.0.0.2" });
    return Promise.resolve({ status: true, result: null });
  });
  (chrome.runtime.getURL as jest.Mock).mockImplementation(
    (p: string) => `chrome-extension://${extensionId}/${p}`
  );
});

afterAll(() => {
  runtime = null as any;
});

it("init stores backend settings", async () => {
  await runtime.init();

  expect(chrome.storage.local.set).toHaveBeenCalledWith({
    uiSocketServerPort: uiSocketServerPort,
    backendServerPort: backendServerPort,
    deviceNetworkIp: deviceIp,
  });
});

it("should create context menu on install for mobile setup", async () => {
  await runtime.init();

  (chrome.runtime.onInstalled as any).callListeners({
    reason: "install",
  });

  expect(chrome.contextMenus.create).toHaveBeenCalledWith({
    id: "configure_mobile_plugin",
    title: "Configure Mobile Plugin",
    contexts: ["action"],
  });
});

it("should open external viewer when user clicks on extension icon", async () => {
  const created = { id: 101, windowId: 202 } as any;
  (chrome.tabs.create as jest.Mock).mockImplementation((_c, cb) => {
    cb(created) as any;
  });

  await runtime.init();
  (chrome.action.onClicked as any).callListeners({} as any);

  expect(global.chrome.tabs.create).toHaveBeenCalledWith(
    { url: `chrome-extension://${extensionId}/ui/index.html` },
    expect.any(Function)
  );
});

it("should open mobile setup dialog window when user opens context menu to configure mobile plugin", async () => {
  await runtime.init();

  (chrome.contextMenus.onClicked as any).callListeners({
    menuItemId: "configure_mobile_plugin",
  });

  expect(global.chrome.windows.create).toHaveBeenCalledWith({
    url: `chrome-extension://${extensionId}/ui/index.html?deviceIp=${deviceIp}&uiSocketServerPort=${uiSocketServerPort}`,
    type: "popup",
    width: 500,
    height: 600,
  });
  expect(global.chrome.tabs.create).not.toHaveBeenCalled();
});
