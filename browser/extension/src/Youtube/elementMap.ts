export type ElementMap = {
  enforcementDialog: {
    viewModel: string;
    container: string;
  };
  videoContainer: string;
  duration: string;
  playButton: string;
};

const rootControlsSelector =
  "#movie_player > div.ytp-chrome-bottom > div.ytp-chrome-controls > div.ytp-left-controls";
export const elementMap = {
  enforcementDialog: {
    viewModel:
      "body > ytd-app > ytd-popup-container > tp-yt-paper-dialog > ytd-enforcement-message-view-model",
    container: "body > ytd-app > ytd-popup-container",
  },
  videoContainer: "#movie_player > div.html5-video-container > video",
  duration: `${rootControlsSelector} > div.ytp-time-display.notranslate > span.ytp-time-wrapper > div > span.ytp-time-duration`,
  playButton: `${rootControlsSelector} > button`,
};
