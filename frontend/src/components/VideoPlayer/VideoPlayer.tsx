import React, {
  useState,
  useEffect,
  forwardRef,
  useRef,
  useImperativeHandle,
  useCallback,
} from 'react';
import { VideoPlayerControlBar } from './VideoPlayerControlBar';
import { VideoPlayerTitleBar } from './VideoPlayerTitleBar';
import styled from '@emotion/styled';
import { useInvokeApi } from '../../services/useInvokeApi';
import { useFullScreen } from '../../providers/FullScreenProvider';

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
    const { focusWindow } = useInvokeApi();
    const { isFullscreen, toggleFullscreen } = useFullScreen();
    const internalVideoRef = useRef<HTMLVideoElement>(null);
    const parentNodeRef = useRef<HTMLDivElement>(null);
    const [isPlaying, setIsPlaying] = useState(false);
    const [currentTime, setCurrentTime] = useState(0);
    const [duration, setDuration] = useState(0);
    const [hideUI, setHideUI] = useState(false);

    useEffect(() => {
      const parent = parentNodeRef.current;
      if (!parent) return;
      const onDblClick = (e: MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        toggleFullscreen();
      };
      parent.addEventListener('dblclick', onDblClick, { passive: false });
      return () => {
        parent.removeEventListener('dblclick', onDblClick);
      };
    }, [toggleFullscreen]);

    useEffect(() => {
      const video = internalVideoRef.current;
      if (!video) return;

      const handleTimeUpdate = () => setCurrentTime(video.currentTime);
      const handleLoadedMetadata = () => setDuration(video.duration);
      const handleVideoPlayEvent = () => setIsPlaying(true);
      const handleVideoEndEvent = () => {
        setIsPlaying(false);
        // Always show control bar and cursor when video ends
        setHideUI(false);
        document.body.style.cursor = 'auto';
        onEnd();
      };
      const handleVideoPauseEvent = () => setIsPlaying(false);
      const handleVideoErrorEvent = () => {
        if (typeof onError === 'function')
          onError(new Error('Unable to play video'));
      };
      const handleSeekOperation = () => {
        const v = internalVideoRef.current;
        if (v) v.blur();
      };

      video.addEventListener('timeupdate', handleTimeUpdate);
      video.addEventListener('seeked', handleSeekOperation);
      video.addEventListener('loadedmetadata', handleLoadedMetadata);
      video.addEventListener('play', handleVideoPlayEvent);
      video.addEventListener('pause', handleVideoPauseEvent);
      video.addEventListener('ended', handleVideoEndEvent);
      video.addEventListener('error', handleVideoErrorEvent);

      return () => {
        video.removeEventListener('timeupdate', handleTimeUpdate);
        video.removeEventListener('seeked', handleSeekOperation);
        video.removeEventListener('loadedmetadata', handleLoadedMetadata);
        video.removeEventListener('play', handleVideoPlayEvent);
        video.removeEventListener('pause', handleVideoPauseEvent);
        video.removeEventListener('ended', handleVideoEndEvent);
        video.removeEventListener('error', handleVideoErrorEvent);
      };
    }, [onEnd, onError]);

    useImperativeHandle(
      ref,
      () => internalVideoRef.current as HTMLVideoElement,
    );

    const togglePlay = () => {
      const video = internalVideoRef.current;
      if (!video) return;
      if (video.paused) video.play();
      else video.pause();
    };

    const handleSeek = (e: React.ChangeEvent<HTMLInputElement>) => {
      const time = parseFloat(e.target.value);
      const video = internalVideoRef.current;
      if (video) {
        video.currentTime = time;
        setCurrentTime(time);
      }
    };

    const hideControlBar = useCallback(() => {
      setHideUI(true);
      document.body.style.cursor = 'none';
    }, []);

    const showControlBar = useCallback(() => {
      setHideUI(false);
      document.body.style.cursor = 'auto';
    }, []);

    useEffect(() => {
      let hideUITimeout: NodeJS.Timeout;
      const handleMouseMove = () => {
        if (isPlaying) {
          showControlBar();
          clearTimeout(hideUITimeout);
          hideUITimeout = setTimeout(hideControlBar, 3000);
        } else {
          // When not playing, always show control bar on mouse move
          showControlBar();
        }
      };

      // Always listen to mouse move, but only auto-hide when playing
      window.addEventListener('mousemove', handleMouseMove);

      if (isPlaying) {
        hideUITimeout = setTimeout(hideControlBar, 3000);
      } else {
        // When video is not playing, ensure control bar is visible
        showControlBar();
      }

      return () => {
        window.removeEventListener('mousemove', handleMouseMove);
        clearTimeout(hideUITimeout);
      };
    }, [isPlaying, showControlBar, hideControlBar]);

    useEffect(() => {
      const handleSpacebarToggle = () => {
        const player = internalVideoRef.current;
        if (
          player !== null &&
          document.activeElement !== internalVideoRef.current
        ) {
          if (isPlaying) {
            player.pause();
          } else {
            player.play();
          }
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
        )
          return;

        event.preventDefault();
        switch (event.code) {
          case 'Space':
            handleSpacebarToggle();
            break;
          case 'F11':
          case 'Escape':
          case 'KeyF':
            void toggleFullscreen();
            break;
          case 'ArrowLeft':
            handleSeekReverse();
            break;
          case 'ArrowRight':
            handleSeekForward();
            break;
          default:
            break;
        }
      };
      window.addEventListener('keydown', handleKeyDown);
      return () => {
        window.removeEventListener('keydown', handleKeyDown);
      };
    }, [isPlaying, toggleFullscreen]);

    useEffect(() => {
      if (isPlaying) {
        focusWindow();
      }
    }, [isPlaying]);

    return (
      <PlayerContainer ref={parentNodeRef}>
        <VideoPlayerTitleBar videoData={videoData} show={!hideUI} />
        <Video ref={internalVideoRef} autoPlay />
        <VideoPlayerControlBar
          show={!hideUI}
          togglePlay={togglePlay}
          isPlaying={isPlaying}
          currentTime={currentTime}
          duration={duration}
          handleSeek={handleSeek}
          toggleFullscreen={() => {
            void toggleFullscreen();
          }}
          isFullscreen={isFullscreen}
        />
      </PlayerContainer>
    );
  },
);
