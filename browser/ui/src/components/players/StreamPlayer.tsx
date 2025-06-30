import { useEffect, useRef } from 'react';
import Hls from 'hls.js';
import { Box } from '@chakra-ui/react';

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

  useEffect(() => {
    if (!Hls.isSupported()) {
      onError(new Error('HLS is not supported on this device'));
      return;
    }

    const hls = new Hls({
      maxBufferLength: 10,
      maxMaxBufferLength: 60,
      maxBufferSize: 30 * 1000 * 1000,
      maxBufferHole: 0.5,
      liveMaxLatencyDuration: 10,
      liveSyncDuration: 5,
      backBufferLength: 0,
    });
    console.log('Loading HLS manifest from:', sourceUrl);
    hls.loadSource(sourceUrl);

    hls.on(Hls.Events.ERROR, (event, data) => {
      if (data.fatal) {
        onError(new Error(`HLS Error: ${data.type} - ${data.details}`));
      } else {
        console.warn('HLS non-fatal error:', data);
      }
    });

    if (playerRef.current) {
      hls.attachMedia(playerRef.current);
    }
  }, [playerRef]);

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
