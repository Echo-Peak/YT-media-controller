import { useEffect, useRef } from 'react';
import { Box } from '@chakra-ui/react';

type Props = {
  sourceUrl: string;
};

declare global {
  interface Window {
    onYouTubeIframeAPIReady: () => void;
    YT: typeof YT;
  }
}

const containerStyles: React.CSSProperties = {
  width: '100vw',
  height: '100vh',
  top: 0,
  left: 0,
  position: 'fixed',
  overflow: 'hidden',
  zIndex: 9999,
};

export const VideoPlayer = ({ sourceUrl }: Props) => {
  const iframeRef = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    if (iframeRef.current) {
      iframeRef.current.src = sourceUrl;
      iframeRef.current.play().catch((error) => {
        console.error('Error playing video:', error);
      });
      iframeRef.current.addEventListener('playing', () => {
        if (iframeRef.current && iframeRef.current.requestFullscreen) {
          iframeRef.current.requestFullscreen().catch((err) => {
            console.error('Error requesting fullscreen:', err);
          });
        }
      });
    }

    console.log('VideoPlayer mounted with sourceUrl:', sourceUrl);
  }, [iframeRef, sourceUrl]);

  return (
    <Box style={containerStyles}>
      <video
        ref={iframeRef}
        style={{ width: 'inherit', height: 'inherit' }}
        controls
      />
      ;
    </Box>
  );
};
