import { ChromeFrontEndRuntime } from "../ChromeFrontEndRuntime";
import {
  convertDurationToSeconds,
  convertSecondsToDuration,
} from "./convertDuration";
import { elementMap } from "./elementMap";
import { getPlaybackControlsState } from "./getPlaybackControls";
import { selectVideoId } from "./selectVideoId";

const runtime = new ChromeFrontEndRuntime();
const maxSecondsToWait = 30;

const startPlaybackWatcher = () => {
  let timeoutId: NodeJS.Timeout | null = null;
  let waitIndex = 0;
  let videoPlaying = false;
  let playbackStarted = false;
  const videoId = selectVideoId(window.location.href);

  if (!videoId) {
    console.error("No video ID found");
    return;
  }

  const playbackTick = () => {
    const controls = getPlaybackControlsState(elementMap);
    if (waitIndex > maxSecondsToWait) {
      console.error(
        `Playback controls not found after ${maxSecondsToWait} seconds`
      );
      clearInterval(timeoutId!);
      return;
    }

    if (!controls.duration || !controls.currentTime) {
      waitIndex++;
      return;
    }

    const elapsedTimeInSeconds = Math.floor(controls.currentTime);

    if (controls.isPaused && videoPlaying) {
      videoPlaying = false;
      runtime.sendEvent({
        action: "webPlaybackPaused",
        data: {
          videoId,
          elapsedTime: convertSecondsToDuration(elapsedTimeInSeconds),
          duration: (controls.duration as HTMLSpanElement).innerText,
        },
      });
    } else if (!controls.isPaused && playbackStarted) {
      videoPlaying = true;
      runtime.sendEvent({
        action: "webPlaybackElapsed",
        data: {
          videoId,
          elapsedTime: convertSecondsToDuration(elapsedTimeInSeconds),
          duration: (controls.duration as HTMLSpanElement).innerText,
        },
      });
    }

    const duration = (controls.duration as HTMLElement).innerText;

    if (elapsedTimeInSeconds === convertDurationToSeconds(duration)) {
      clearInterval(timeoutId!);
      runtime.sendEvent({
        action: "webPlaybackEnded",
        data: {
          videoId,
        },
      });
      return;
    }

    if (elapsedTimeInSeconds < 10 && !playbackStarted) {
      playbackStarted = true;
      runtime.sendEvent({
        action: "webPlaybackStarted",
        data: {
          videoId,
        },
      });
    }
  };

  timeoutId = setInterval(playbackTick, 1000);
};

function startDialogBlocker() {
  let timeoutId: NodeJS.Timeout | null = null;
  const removeDialog = () => {
    const dialog = document.querySelector(
      elementMap.enforcementDialog.viewModel
    );
    if (dialog) {
      const container = document.querySelector(
        elementMap.enforcementDialog.container
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
