import { ChromeFrontEndRuntime } from "../ChromeFrontEndRuntime";
import { getPlaybackControls } from "./getPlaybackControls";
import { isFirstFrame } from "./isFirstFrame";
import { selectVideoId } from "./selectVideoId";

const runtime = new ChromeFrontEndRuntime();

const elements = {
  enforcementDialog: {
    viewModel:
      "body > ytd-app > ytd-popup-container > tp-yt-paper-dialog > ytd-enforcement-message-view-model",
    container: "body > ytd-app > ytd-popup-container",
  },
  playback: {
    container:
      "#movie_player > div.ytp-chrome-bottom > div.ytp-chrome-controls > div.ytp-left-controls > div.ytp-time-display.notranslate > span.ytp-time-wrapper > div",
    currentTime: "span.ytp-time-current",
    duration: "span.ytp-time-duration",
  },
};

const startPlaybackWatcher = () => {
  let timeoutId: NodeJS.Timeout | null = null;
  const controls = getPlaybackControls(
    elements.playback.container,
    elements.playback.currentTime,
    elements.playback.duration
  );
  let waitIndex = 0;
  let videoPlaying = false;
  const videoId = selectVideoId(window.location.href);

  if (!videoId) {
    console.error("No video ID found");
    return;
  }

  const playbackTick = () => {
    if (waitIndex > 20) {
      clearInterval(timeoutId!);
      return;
    }
    if (!controls.currentTime || !controls.duration) {
      waitIndex++;
      return;
    }

    const currentTime = (controls.currentTime as HTMLElement).innerText;
    const duration = (controls.duration as HTMLElement).innerText;

    if (currentTime === duration) {
      clearInterval(timeoutId!);
      runtime.sendEvent({
        action: "playbackEnded",
        data: {
          videoId,
        },
      });
      return;
    }

    if (!isFirstFrame(currentTime)) {
      if (!videoPlaying) {
        runtime.sendEvent({
          action: "playbackStarted",
          data: {
            videoId,
          },
        });
        videoPlaying = true;
      }
      runtime.sendEvent({
        action: "playbackElapsed",
        data: {
          videoId,
          elapsedTime: currentTime,
          duration,
        },
      });
    }
  };

  timeoutId = setInterval(playbackTick, 1000);
};

function startDialogBlocker() {
  let timeoutId: NodeJS.Timeout | null = null;
  const removeDialog = () => {
    const dialog = document.querySelector(elements.enforcementDialog.viewModel);
    if (dialog) {
      const container = document.querySelector(
        elements.enforcementDialog.container
      );
      if (container) {
        container.removeChild(dialog);
      }
      console.log("Removed enforcement dialog");
      clearTimeout(timeoutId!);
      runtime.sendEvent({
        action: "enforcementDialogRemoved",
      });
    }
  };

  timeoutId = setInterval(removeDialog, 1000);
}

window.onload = function () {
  const isWatchable = window.location.href.includes("youtube.com/watch");
  if (isWatchable) {
    startDialogBlocker();
    startPlaybackWatcher();
  }
};
