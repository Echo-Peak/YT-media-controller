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
  onEnd: () => void;
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
  ({ videoData, onError, onEnd }, ref) => {
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
        onEnd();
      };
      const handleVideoPauseEvent = () => {
        setIsPlaying(false);
      };
      const handleVideoErrorEvent = () => {
        if (typeof onError === 'function') {
          onError(new Error('Unable to play video'));
        }
      };
      const handleDoubleClick = () => {
        if (document.fullscreenElement) {
          document.exitFullscreen().catch(console.error);
          setIsFullscreen(false);
        } else {
          video.requestFullscreen().catch(console.error);
          setIsFullscreen(true);
        }
      };

      const handleSeekOperation = () => {
        const video = internalVideoRef.current;
        if (video) {
          video.blur();
        }
      };

      video.addEventListener('timeupdate', handleTimeUpdate);
      video.addEventListener('seeked', handleSeekOperation);
      video.addEventListener('loadedmetadata', handleLoadedMetadata);
      video.addEventListener('dblclick', handleDoubleClick);

      let hasEnteredFullscreen = false;
      video.addEventListener('play', handleVideoPlayEvent);
      video.addEventListener('pause', handleVideoPauseEvent);
      video.addEventListener('ended', handleVideoEndEvent);
      video.addEventListener('error', handleVideoErrorEvent);

      return () => {
        video.removeEventListener('timeupdate', handleTimeUpdate);
        video.removeEventListener('seeked', handleSeekOperation);
        video.removeEventListener('loadedmetadata', handleLoadedMetadata);
        video.removeEventListener('play', handleVideoPlayEvent);
        video.removeEventListener('end', handleVideoEndEvent);
        video.removeEventListener('pause', handleVideoPauseEvent);
        video.removeEventListener('dblclick', handleDoubleClick);
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
      } else {
        video.pause();
      }
    };

    const toggleFullscreen = () => {
      const video = internalVideoRef.current;
      if (!video) return;

      if (!document.fullscreenElement) {
        document.documentElement
          .requestFullscreen()
          .then(() => {
            video.blur();
          })
          .catch(console.error);
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

    useEffect(() => {
      const handleSpacebarToggle = () => {
        const player = internalVideoRef.current;
        if (
          player !== null &&
          document.activeElement !== internalVideoRef.current
        ) {
          isPlaying ? player.pause() : player.play();
        }
      };

      const handleSeekReverse = () => {
        const player = internalVideoRef.current;
        if (player) {
          player.currentTime = Math.max(0, player.currentTime - 5);
          setCurrentTime(player.currentTime);
        }
      };

      const handleSeekForward = () => {
        const player = internalVideoRef.current;
        if (player) {
          player.currentTime = Math.min(
            player.duration,
            player.currentTime + 5,
          );
          setCurrentTime(player.currentTime);
        }
      };

      const handleKeyDown = (event: KeyboardEvent) => {
        const activeTag = document.activeElement?.tagName.toLowerCase();
        if (
          activeTag === 'input' ||
          activeTag === 'textarea' ||
          activeTag === 'button'
        ) {
          return;
        }

        switch (event.code) {
          case 'Space':
            event.preventDefault();
            handleSpacebarToggle();
            break;
          case 'KeyF':
            event.preventDefault();
            toggleFullscreen();
            break;
          case 'ArrowLeft':
            event.preventDefault();
            handleSeekReverse();
            break;
          case 'ArrowRight':
            event.preventDefault();
            handleSeekForward();
            break;
          default:
            // Do nothing for other keys
            break;
        }
      };
      window.addEventListener('keydown', handleKeyDown);
      return () => {
        window.removeEventListener('keydown', handleKeyDown);
      };
    }, [isPlaying, internalVideoRef]);

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
