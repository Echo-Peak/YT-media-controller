import { useEffect, useRef } from 'react';
import { Box } from '@chakra-ui/react';
import { VideoPlayer, VideoPlayerRef } from '../VideoPlayer/VideoPlayer';
import { StreamablePlayerProps } from '../../types/StreamablePlayerProps';

export const DASHPlayer = ({
  sourceUrl,
  videoData,
  onError,
  onEnded,
}: StreamablePlayerProps) => {
  const playerRef = useRef<VideoPlayerRef>(null);

  useEffect(() => {
    if (playerRef.current) {
      playerRef.current.src = sourceUrl;
    }
  }, [playerRef, sourceUrl]);

  const handleError = (error: Error) => {
    onError(error);
  };

  return (
    <Box>
      <VideoPlayer
        ref={playerRef}
        videoData={videoData}
        onError={handleError}
        onEnd={onEnded}
      />
    </Box>
  );
};
