import { doesTabExist } from "./helpers/doesTabExists";

type Tab = {
  id: number;
  name: string;
};

type WindowTabs = {
  windowId: number;
  tabs: Tab[];
};

export class WindowManager {
  private windows: Map<string, WindowTabs> = new Map();
  public static windows = {
    main: "main",
    mobile: "mobile",
  };
  public static tabs = {
    externalViewer: "externalViewer",
    youtube: "youtube",
    mobileSetup: "mobileSetup",
  };

  add(windowName: string, windowId: number, tabId: Tab): void {
    if (!this.windows.has(windowName)) {
      this.windows.set(windowName, { windowId, tabs: [] });
    }
    this.windows.get(windowName)!.tabs.push(tabId);
  }

  createWindow = async (
    windowName: string,
    tabName: string,
    createData: chrome.windows.CreateData
  ) => {
    const chromeWindow = await chrome.windows.create(createData);
    const [firstTab] = await chrome.tabs.query({
      windowId: chromeWindow.id!,
      index: 0,
    });

    this.windows.set(windowName, {
      windowId: chromeWindow.id!,
      tabs: [{ id: firstTab.id!, name: tabName }],
    });
  };

  addTabToWindow(
    windowName: string,
    tabName: string,
    tab: chrome.tabs.CreateProperties
  ): void {
    chrome.tabs.create(tab, (createdTab) => {
      if (createdTab.id !== undefined) {
        console.log(`Tab created in window ${windowName}:`, createdTab.id);
        this.add(windowName, createdTab.windowId, {
          id: createdTab.id,
          name: tabName,
        });
      }
    });
  }

  async has(windowName: string, tabName: string): Promise<boolean> {
    const windowTabs = this.windows.get(windowName);
    if (!windowTabs) return false;

    for (const tab of windowTabs.tabs) {
      if (tab.name === tabName) {
        const exists = await doesTabExist(tab.id);
        if (!exists) {
          this.removeTab(windowName, tabName);
        }
        return exists;
      }
    }
    return false;
  }

  getWindowTabs(windowName: string): Tab[] {
    return this.windows.get(windowName)?.tabs ?? [];
  }

  removeWindow(windowName: string): void {
    this.windows.delete(windowName);
  }

  removeWindowById(windowId: number): void {
    for (const [name, window] of this.windows.entries()) {
      if (window.windowId === windowId) {
        this.windows.delete(name);
        return;
      }
    }
  }

  removeTab(windowName: string, tabName: string): void {
    const existingTabs = this.windows.get(windowName);
    if (!existingTabs) return;
    const updatedTabs = existingTabs.tabs.filter((tab) => tab.name !== tabName);
    this.windows.set(windowName, {
      windowId: existingTabs.windowId,
      tabs: updatedTabs,
    });
  }

  updateWindow(
    windowName: string,
    newFields: Partial<chrome.windows.UpdateInfo>
  ): void {
    const existingWindow = this.windows.get(windowName);
    if (!existingWindow) return;

    const windowId = existingWindow.windowId;
    chrome.windows.update(windowId, newFields);
  }

  update(
    windowName: string,
    tabName: string,
    newFields: Partial<chrome.tabs.UpdateProperties>
  ): void {
    const existingWindowTabs = this.windows.get(windowName);

    for (const tab of existingWindowTabs?.tabs || []) {
      if (tab.name === tabName) {
        console.log(
          `Updating tab ${tab.name}(${tab.id}) in window ${windowName}`,
          newFields
        );
        chrome.tabs.update(tab.id, newFields).catch((err) => {
          console.error(
            `Failed to update tab ${tab.id} in window ${windowName}:`,
            err
          );
        });
      }
    }
  }
}
