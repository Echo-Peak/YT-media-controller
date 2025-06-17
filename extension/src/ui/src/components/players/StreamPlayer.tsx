import { useEffect, useRef } from 'react';
import Hls from 'hls.js';

type StreamPlayerProps = {
  sourceUrl: string;
  onError: (error: Error) => void;
}

const containerStyles: React.CSSProperties = {
  width: '100vw',
  height: '100vh',
  position: 'fixed',
  overflow: 'hidden',
  zIndex: 9999,
};

export const StreamPlayer = ({ sourceUrl, onError}: StreamPlayerProps) => {
    const playerRef = useRef<HTMLMediaElement | undefined>(undefined);

    
    useEffect(() => {
      if(!Hls.isSupported()) {
        onError(new Error("HLS is not supported on this device"));
        return;
      }
      
      var hls = new Hls();
      console.log("Loading HLS manifest from:", sourceUrl);
      hls.loadSource(sourceUrl);

      hls.on(Hls.Events.ERROR, (event, data) => {
        console.error("HLS Error:", data);
        onError(new Error(`HLS Error: ${data.type} - ${data.details} - ${data.fatal ? 'Fatal' : 'Non-fatal'}`));
      });

      if(playerRef.current) {
        hls.attachMedia(playerRef.current);
      }
    }, [playerRef])
    

    return <div style={containerStyles}>
        <video
          ref={playerRef as React.RefObject<HTMLVideoElement>}
          style={{ width: '100vw', height: '100vh' }}
          controls
          autoPlay
          crossOrigin="anonymous"
        >
          Your browser does not support the video tag.
        </video>
    </div>
}