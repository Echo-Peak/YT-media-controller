import { ChromeBackgroundRuntime } from "./ChromeBackgroundRuntime";
import { waitForSocket } from "./mocks/helpers/waitForSocket";
import { MockWebSocket } from "./mocks/MockWebSocket";
import { WebSocketResponse } from "./types/WebSocketResponse";
import { WebSocketClient } from "./WebSocketClient";
import { WindowManager } from "./WindowManager";

const flush = async () => {
  await Promise.resolve();
  await Promise.resolve();
};

describe("ChromeBackgroundRuntime", () => {
  const extensionId = "abcdefghijklmnopabcdefghijklmnop";
  let runtime: ChromeBackgroundRuntime;
  const deviceIp = "192.168.1.50";
  const uiSocketServerPort = 1234;
  const backendServerPort = 9100;

  beforeAll(() => {
    (global as any).WebSocket = MockWebSocket as any;
  });

  beforeEach(() => {
    runtime = new ChromeBackgroundRuntime();
    jest.useFakeTimers();
    jest.clearAllMocks();
    (chrome.storage.local.get as jest.Mock).mockResolvedValue({
      uiSocketServerPort,
      backendServerPort,
      deviceNetworkIp: deviceIp,
    });

    (chrome.runtime.getURL as jest.Mock).mockReturnValue(
      `chrome-extension://${extensionId}/ui/index.html`
    );
  });

  afterEach(() => {
    jest.useRealTimers();
    MockWebSocket.onSend = null;
    MockWebSocket.instances.length = 0;
  });

  it('should create websocket client connection and send "getBackendSettings" event upon initialization and update mobile settings UI/window', async () => {
    const windowManagerUpdateTabSpy = jest
      .spyOn(WindowManager.prototype, "update")
      .mockImplementation(() => {});

    chrome.tabs.update = jest.fn();
    const initProm = runtime.init();
    await waitForSocket(1);
    const wsForRuntime = MockWebSocket.instances[0];
    wsForRuntime.open();
    await initProm;

    const client = new WebSocketClient<WebSocketResponse>(
      "ws://localhost:1234"
    );
    await waitForSocket(2);
    const clientWs = MockWebSocket.instances[1];
    const msgSpy = jest.fn();
    client.on("message", msgSpy);

    MockWebSocket.onSend = (ws, data) => {
      const parsed = JSON.parse(data);
      if (parsed.Action === "getBackendSettings") {
        ws.message({
          Action: "backendSettings",
          Data: {
            DeviceNetworkIp: deviceIp,
            UISocketServerPort: uiSocketServerPort,
            BackendServerPort: backendServerPort,
          },
        });
      }
    };

    clientWs.open();
    client.send({ Action: "getBackendSettings", Data: {} });

    expect(msgSpy).toHaveBeenCalledWith({
      Action: "backendSettings",
      Data: {
        DeviceNetworkIp: deviceIp,
        UISocketServerPort: uiSocketServerPort,
        BackendServerPort: backendServerPort,
      },
    });

    expect(windowManagerUpdateTabSpy).toHaveBeenCalled();
  });

  it("should create context menu on install for mobile setup", async () => {
    const initProm = runtime.init();
    await waitForSocket(1);
    const wsForRuntime = MockWebSocket.instances[0];
    wsForRuntime.open();
    await initProm;

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

    (chrome.action.onClicked as any).callListeners({} as any);

    const initProm = runtime.init();
    await waitForSocket(1);
    const wsForRuntime = MockWebSocket.instances[0];
    wsForRuntime.open();
    await initProm;

    expect(chrome.tabs.create).toHaveBeenCalledWith(
      { url: `chrome-extension://${extensionId}/ui/index.html`, active: true },
      expect.any(Function)
    );
  });

  it("should open mobile setup dialog window when user opens context menu to configure mobile plugin", async () => {
    (chrome.storage.local.get as jest.Mock).mockResolvedValue({
      uiSocketServerPort,
    });

    (chrome.tabs.query as jest.Mock).mockResolvedValue([
      { id: 1, windowId: 10 },
    ]);
    (chrome.windows.create as jest.Mock).mockResolvedValue({ id: 100 } as any);
    const initProm = runtime.init();

    await waitForSocket(1);
    const wsForRuntime = MockWebSocket.instances[0];
    wsForRuntime.open();
    await initProm;

    wsForRuntime.message(
      JSON.stringify({
        Action: "backendSettings",
        Data: {
          DeviceNetworkIp: deviceIp,
          UISocketServerPort: uiSocketServerPort,
          BackendServerPort: backendServerPort,
        },
      })
    );

    await flush();

    (chrome.contextMenus.onClicked as any).callListeners({
      menuItemId: "configure_mobile_plugin",
    });

    await flush();

    expect(chrome.windows.create).toHaveBeenCalledWith({
      url: `chrome-extension://${extensionId}/ui/index.html?deviceIp=${deviceIp}&devicePort=${backendServerPort}&uiSocketServerPort=${uiSocketServerPort}`,
      type: "popup",
      width: 500,
      height: 600,
    });
  });
});
