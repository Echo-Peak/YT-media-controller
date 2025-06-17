import { useEffect, useRef } from 'react';

type Props = {
  sourceUrl: string;
}

declare global {
  interface Window {
    onYouTubeIframeAPIReady: () => void;
    YT: typeof YT;
  }
}

const containerStyles: React.CSSProperties = {
  width: '100vw',
  height: '100vh',
  position: 'fixed',
  overflow: 'hidden',
  zIndex: 9999,
};

export const VideoPlayer = ({ sourceUrl }: Props) => {
    const iframeRef = useRef<HTMLVideoElement>(null);

    useEffect(() => {
    if (iframeRef.current) {
      iframeRef.current.src = sourceUrl;
      iframeRef.current.play().catch(error => {
        console.error("Error playing video:", error);
      });
    }

      console.log("VideoPlayer mounted with sourceUrl:", sourceUrl);
    }, [iframeRef, sourceUrl]);

  return (<div style={containerStyles}>
    <video ref={iframeRef} style={{width: 'inherit', height: 'inherit'}} controls/>;
  </div>)
}