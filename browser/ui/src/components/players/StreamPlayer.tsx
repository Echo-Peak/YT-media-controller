import { useEffect, useRef } from 'react';
import { Box } from '@chakra-ui/react';
import { useHlsStreamer } from '../../services/useHlsStreamer';

type StreamPlayerProps = {
  sourceUrl: string;
  onError: (error: Error) => void;
};

const containerStyles: React.CSSProperties = {
  width: '100vw',
  height: '100vh',
  position: 'fixed',
  overflow: 'hidden',
  top: 0,
  left: 0,
  zIndex: 9999,
};

export const StreamPlayer = ({ sourceUrl, onError }: StreamPlayerProps) => {
  const playerRef = useRef<HTMLMediaElement | undefined>(undefined);

  const onFatalError = (error: Error) => {
    console.error('Fatal error in StreamPlayer:', error);
    onError(error);
  };
  const { loadStream, cleanupStream } = useHlsStreamer(onFatalError);

  const onPlayHandler = () => {
    if (!playerRef.current) {
      return;
    }

    try {
      if (document.fullscreenElement !== playerRef.current) {
        if (playerRef.current?.requestFullscreen) {
          playerRef.current.requestFullscreen().catch(console.warn);
        }
      }
    } catch (e) {
      console.warn('Failed to enter fullscreen:', e);
    }
  };

  useEffect(() => {
    if (!sourceUrl || !playerRef.current) {
      console.warn('No source URL or player reference available');
      return;
    }

    console.log('Loading HLS manifest from:', sourceUrl);
    loadStream(sourceUrl, playerRef.current);

    if (playerRef.current) {
      playerRef.current.addEventListener('playing', onPlayHandler);
    }
    return () => {
      cleanupStream();
      if (playerRef.current) {
        playerRef.current.removeEventListener('playing', onPlayHandler);
      }
    };
  }, [playerRef, sourceUrl]);

  return (
    <Box style={containerStyles}>
      <video
        ref={playerRef as React.RefObject<HTMLVideoElement>}
        style={{ width: '100vw', height: '100vh' }}
        controls
        autoPlay
        crossOrigin="anonymous"
      >
        Your browser does not support the video tag.
      </video>
    </Box>
  );
};
