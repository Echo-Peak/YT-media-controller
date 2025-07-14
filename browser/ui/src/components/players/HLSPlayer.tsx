import { useEffect, useRef } from 'react';
import { Box } from '@chakra-ui/react';
import { useHlsStreamer } from '../../services/useHlsStreamer';
import { VideoPlayer, VideoPlayerRef } from '../VideoPlayer/VideoPlayer';
import { StreamablePlayerProps } from '../../types/StreamablePlayerProps';
import { useSpacebarPlayToggle } from '../VideoPlayer/useSpacebarToggle';

const containerStyles: React.CSSProperties = {
  width: '100vw',
  height: '100vh',
  position: 'fixed',
  overflow: 'hidden',
  top: 0,
  left: 0,
  zIndex: 9999,
};

export const HLSPlayer = ({
  sourceUrl,
  videoData,
  onError,
  onEnded,
}: StreamablePlayerProps) => {
  const playerRef = useRef<VideoPlayerRef>(null);

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

  useSpacebarPlayToggle(playerRef);

  return (
    <Box style={containerStyles}>
      <VideoPlayer ref={playerRef} videoData={videoData} onEnd={onEnded} />
    </Box>
  );
};
