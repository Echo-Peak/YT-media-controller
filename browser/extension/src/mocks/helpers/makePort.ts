type Listener<T extends any[] = any[]> = (...args: T) => void;

const makeEvent = <T extends any[] = any[]>() => {
  const listeners = new Set<Listener<T>>();
  const addListener = (fn: Listener<T>) => listeners.add(fn);
  const removeListener = (fn: Listener<T>) => listeners.delete(fn);
  const dispatch = (...args: T) => listeners.forEach((l) => l(...args));
  return { addListener, removeListener, dispatch };
};

export const makePort = () => {
  const onMessage = makeEvent<[any]>();
  const onDisconnect = makeEvent<[chrome.runtime.Port]>();
  const postMessage = jest.fn();
  const port = {
    name: "com.ytmediacontroller.app",
    onMessage,
    onDisconnect,
    postMessage,
  } as unknown as chrome.runtime.Port;
  return { port, onMessage, onDisconnect, postMessage };
};
