import { ChromeFrontEndRuntime } from "./ChromeFrontEndRuntime";

const runtime = new ChromeFrontEndRuntime();


const selectPlaybackControls = () => {
  const selector = document.querySelector("#movie_player > div.ytp-chrome-bottom > div.ytp-chrome-controls > div.ytp-left-controls > div.ytp-time-display.notranslate > span.ytp-time-wrapper > div")
  return {
    currentTime: selector?.querySelector("span.ytp-time-current"),
    duration: selector?.querySelector("span.ytp-time-duration"),
  }
}

const isFirstFrame = (time: string) : boolean => {
  const parts = time.split(':');
  if(parts.length === 2){
    return parseInt(parts[0]) === 0 && parseInt(parts[1]) === 0;
  }else if(parts.length === 3){
    return parseInt(parts[1]) === 0 && parseInt(parts[2]) === 0;
  }
  return false;
}

const selectVideoId = () => {
  const urlParts = new URL(window.location.href);
  if(urlParts.pathname.startsWith("/shorts")){
    const shortUrlParts = window.location.href.split('/');
    return shortUrlParts[shortUrlParts.length - 1].split('?')[0];
  }else if(urlParts.pathname.startsWith("/watch")){
    return window.location.href.split('v=')[1].split('&')[0];
  }
  return null;
}

const startPlaybackWatcher = () => {
  const controls = selectPlaybackControls();
  let waitIndex = 0;
  let videoPlaying = false;
  const videoId = selectVideoId();

  if(!videoId){
    console.error("No video ID found");
    return;
  }

  let interval = setInterval(() => {

      if(waitIndex > 20){
        clearInterval(interval);
        return;
      }

      if(!controls.currentTime || !controls.duration){
        waitIndex++;
        return;
      }

      const currentTime = (controls.currentTime as HTMLElement).innerText;
      const duration = (controls.duration as HTMLElement).innerText;

      if(currentTime === duration){
        clearInterval(interval);
        runtime.sendVideoEndedEvent(videoId);
        return;
      }

      if(!isFirstFrame(currentTime)){
        if(!videoPlaying){
          runtime.sendVideoStartedEvent(videoId);
          videoPlaying = true;
        }
        runtime.sendVideoElapsedTimeEvent(videoId, currentTime, duration);
      }
  }, 1000);
}


window.onload = function () {
  startPlaybackWatcher();
}