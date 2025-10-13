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
    const { enterFullscreen, exitFullscreen } = useInvokeApi();
    const internalVideoRef = useRef<HTMLVideoElement>(null);
    const parentNodeRef = useRef<HTMLDivElement>(null);
    const [isPlaying, setIsPlaying] = useState(false);
    const [currentTime, setCurrentTime] = useState(0);
    const [duration, setDuration] = useState(0);
    const [isFullscreen, setIsFullscreen] = useState(false);
    const [hideUI, setHideUI] = useState(false);

    const togglingRef = useRef(false);

    const toggleFullscreen = useCallback(async () => {
      if (togglingRef.current) return;
      togglingRef.current = true;
      try {
        if (isFullscreen) {
          await exitFullscreen();
          setIsFullscreen(false);
        } else {
          await enterFullscreen();
          setIsFullscreen(true);
        }
      } finally {
        togglingRef.current = false;
      }
    }, [isFullscreen, enterFullscreen, exitFullscreen]);

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

    const hideControlBar = () => {
      setHideUI(true);
      document.body.style.cursor = 'none';
    };

    const showControlBar = () => {
      setHideUI(false);
      document.body.style.cursor = 'auto';
    };

    useEffect(() => {
      let hideUITimeout: NodeJS.Timeout;
      const handleMouseMove = () => {
        if (isPlaying) {
          showControlBar();
          clearTimeout(hideUITimeout);
          hideUITimeout = setTimeout(hideControlBar, 3000);
        }
      };
      if (isPlaying) {
        window.addEventListener('mousemove', handleMouseMove);
        hideUITimeout = setTimeout(hideControlBar, 3000);
      }
      return () => {
        window.removeEventListener('mousemove', handleMouseMove);
        clearTimeout(hideUITimeout);
      };
    }, [isPlaying, setHideUI]);

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
        event.preventDefault();
        const activeTag = document.activeElement?.tagName.toLowerCase();
        if (
          activeTag === 'input' ||
          activeTag === 'textarea' ||
          activeTag === 'button'
        )
          return;
        switch (event.code) {
          case 'Space':
            handleSpacebarToggle();
            break;
          case 'F11':
          case 'Escape':
          case 'KeyF':
            toggleFullscreen();
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
          toggleFullscreen={toggleFullscreen}
          isFullscreen={isFullscreen}
        />
      </PlayerContainer>
    );
  },
);
