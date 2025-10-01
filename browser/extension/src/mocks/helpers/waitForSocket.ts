import { MockWebSocket } from "../MockWebSocket";

const flush = async () => {
  await Promise.resolve();
  await Promise.resolve();
};

export const waitForSocket = async (minCount = 1, tries = 10) => {
  for (let i = 0; i < tries; i++) {
    if (MockWebSocket.instances.length >= minCount) return;
    await flush();
  }
  throw new Error("WebSocket could not be created in time");
};
