export const useChromeRuntime = () => {
  return {
    sendEvent(event: { action: string; data?: Record<string, unknown> }) {
      chrome.runtime.sendMessage(event);
    },
  };
};
