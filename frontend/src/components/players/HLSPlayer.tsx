import { useEffect, useRef } from 'react';
import { Box } from '@chakra-ui/react';
import { useHlsStreamer } from '../../services/useHlsStreamer';
import { VideoPlayer, VideoPlayerRef } from '../VideoPlayer/VideoPlayer';
import { StreamablePlayerProps } from '../../types/StreamablePlayerProps';

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

  useEffect(() => {
    if (!sourceUrl || !playerRef.current) {
      console.warn('No source URL or player reference available');
      return;
    }

    console.log('Loading HLS manifest from:', sourceUrl);
    loadStream(sourceUrl, playerRef.current);

    return () => {
      cleanupStream();
    };
  }, [playerRef, sourceUrl]);

  return (
    <Box style={containerStyles}>
      <VideoPlayer ref={playerRef} videoData={videoData} onEnd={onEnded} />
    </Box>
  );
};
