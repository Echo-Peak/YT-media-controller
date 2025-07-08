import { ElementMap } from "./elementMap";

export const getPlaybackControlsState = (
  elementMap: ElementMap
): {
  currentTime?: number;
  duration?: Element;
  isPaused: boolean;
} => {
  const videoContainer =
    document.querySelector(elementMap.videoContainer) ?? undefined;
  const duration = document.querySelector(elementMap.duration) ?? undefined;
  const playButton = document.querySelector(elementMap.playButton) ?? undefined;

  return {
    currentTime: (videoContainer as HTMLVideoElement).currentTime,
    duration,
    isPaused: playButton?.getAttribute("data-title-no-tooltip") === "Play",
  };
};
