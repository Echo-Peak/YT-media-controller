const chrome = require("jest-chrome");

Object.assign(global, chrome);

const makeEvent = () => {
  const listeners = new Set();
  const addListener = jest.fn((fn) => listeners.add(fn));
  const removeListener = jest.fn((fn) => listeners.delete(fn));
  const hasListener = jest.fn((fn) => listeners.has(fn));
  const callListeners = (...args) => listeners.forEach((fn) => fn(...args));
  return { addListener, removeListener, hasListener, callListeners };
};

global.chrome.action ||= {};
global.chrome.contextMenus ||= {
  create: jest.fn(),
  update: jest.fn(),
  remove: jest.fn(),
  removeAll: jest.fn(),
};

Object.defineProperty(global.chrome.action, "onClicked", {
  value: makeEvent(),
  configurable: true,
});

Object.defineProperty(global.chrome.contextMenus, "onClicked", {
  value: makeEvent(),
  configurable: true,
});
