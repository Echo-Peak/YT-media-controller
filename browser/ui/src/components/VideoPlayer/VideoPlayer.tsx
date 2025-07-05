import React, {
  useState,
  useEffect,
  forwardRef,
  useRef,
  useImperativeHandle,
} from 'react';
import { VideoPlayerControlBar } from './VideoPlayerControlBar';
import { VideoPlayerTitleBar } from './VideoPlayerTitleBar';
import styled from '@emotion/styled';

type VideoPlayerProps = {
  ref: React.RefObject<HTMLVideoElement>;
  videoData?: {
    title: string;
    uploader: string;
  };
  onError?: (error: Error) => void;
};

const PlayerContainer = styled.div({
  position: 'fixed',
  top: 0,
  left: 0,
  width: '100vw',
  height: '100vh',
  backgroundColor: '#000',
  zIndex: 9999,
  display: 'flex',
  flexDirection: 'column',
});

const Video = styled.video({
  flex: 1,
  width: '100%',
  height: '100%',
  objectFit: 'contain',
  backgroundColor: '#000',
});

export type VideoPlayerRef = HTMLVideoElement | null;

export const VideoPlayer = forwardRef<VideoPlayerRef, VideoPlayerProps>(
  ({ videoData, onError }, ref) => {
    const internalVideoRef = useRef<HTMLVideoElement>(null);
    const [isPlaying, setIsPlaying] = useState(false);
    const [currentTime, setCurrentTime] = useState(0);
    const [duration, setDuration] = useState(0);
    const [isFullscreen, setIsFullscreen] = useState(false);
    const [hideUI, setHideUI] = useState(false);

    useEffect(() => {
      const video = internalVideoRef.current;
      if (!video) return;

      const handleTimeUpdate = () => setCurrentTime(video.currentTime);
      const handleLoadedMetadata = () => setDuration(video.duration);
      const handleVideoPlayEvent = () => {
        setIsPlaying(true);
        if (!hasEnteredFullscreen && document.fullscreenElement !== video) {
          video.requestFullscreen().catch(console.error);
          setIsFullscreen(true);
          hasEnteredFullscreen = true;
        }
      };
      const handleVideoEndEvent = () => {
        setIsPlaying(false);
      };
      const handleVideoPauseEvent = () => {
        setIsPlaying(false);
      };
      const handleVideoErrorEvent = () => {
        if (typeof onError === 'function') {
          onError(new Error('Unable to play video'));
        }
      };

      video.addEventListener('timeupdate', handleTimeUpdate);
      video.addEventListener('loadedmetadata', handleLoadedMetadata);

      let hasEnteredFullscreen = false;
      video.addEventListener('play', handleVideoPlayEvent);
      video.addEventListener('pause', handleVideoPauseEvent);
      video.addEventListener('ended', handleVideoEndEvent);
      video.addEventListener('error', handleVideoErrorEvent);

      return () => {
        video.removeEventListener('timeupdate', handleTimeUpdate);
        video.removeEventListener('loadedmetadata', handleLoadedMetadata);
        video.removeEventListener('play', handleVideoPlayEvent);
        video.removeEventListener('end', handleVideoEndEvent);
        video.removeEventListener('pause', handleVideoPauseEvent);
      };
    }, []);

    useImperativeHandle(
      ref,
      () => internalVideoRef.current as HTMLVideoElement,
    );

    const togglePlay = () => {
      const video = internalVideoRef.current;
      if (!video) return;

      if (video.paused) {
        video.play();
        setIsPlaying(true);
      } else {
        video.pause();
        setIsPlaying(false);
      }
    };

    const toggleFullscreen = () => {
      if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen().catch(console.error);
        setIsFullscreen(true);
      } else {
        document.exitFullscreen().catch(console.error);
        setIsFullscreen(false);
      }
    };

    const handleSeek = (e: React.ChangeEvent<HTMLInputElement>) => {
      const time = parseFloat(e.target.value);
      const video = internalVideoRef.current;
      if (video) {
        video.currentTime = time;
        setCurrentTime(time);
      }
    };

    useEffect(() => {
      let hideUITimeout: NodeJS.Timeout;

      const handleMouseMove = () => {
        if (isPlaying) {
          setHideUI(false);
          clearTimeout(hideUITimeout);
          hideUITimeout = setTimeout(() => {
            setHideUI(true);
          }, 3000);
        }
      };

      if (isPlaying) {
        window.addEventListener('mousemove', handleMouseMove);
        hideUITimeout = setTimeout(() => {
          setHideUI(true);
        }, 3000);
      }

      return () => {
        window.removeEventListener('mousemove', handleMouseMove);
        clearTimeout(hideUITimeout);
      };
    }, [isPlaying]);

    return (
      <PlayerContainer>
        <VideoPlayerTitleBar videoData={videoData} show={!hideUI} />

        <Video ref={internalVideoRef} autoPlay />

        <VideoPlayerControlBar
          show={!hideUI}
          togglePlay={togglePlay}
          isPlaying={isPlaying}
          currentTime={currentTime}
          duration={duration}
          handleSeek={handleSeek}
          toggleFullscreen={toggleFullscreen}
          isFullscreen={isFullscreen}
        />
      </PlayerContainer>
    );
  },
);
