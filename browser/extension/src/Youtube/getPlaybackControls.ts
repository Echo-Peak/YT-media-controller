export const getPlaybackControls = (
  containerSelector: string,
  currentTimeSelector: string,
  durationSelector: string
): {
  currentTime: Element;
  duration: Element;
} => {
  const selector = document.querySelector(containerSelector);
  const currentTime = selector?.querySelector(currentTimeSelector);
  const duration = selector?.querySelector(durationSelector);

  if (!currentTime || !duration) {
    throw new Error("Playback control elements not found");
  }

  return {
    currentTime,
    duration,
  };
};
