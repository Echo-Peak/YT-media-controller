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

const extractVideoId = (url: string): string => {
  const lastPart = url.split('/').pop();
  if(lastPart?.includes('watch?v=')) {
    return lastPart.split('watch?v=')[1];
  }
  return lastPart || '';
}


export const YoutubePlayer = ({ sourceUrl }: Props) => {
    const playerRef = useRef<YT.Player | undefined>(undefined);
    const iframeRef = useRef<HTMLDivElement>(null);
    const videoId = extractVideoId(sourceUrl);

    useEffect(() => {
    if (window.YT && window.YT.Player) return;

    const tag = document.createElement('script');
    tag.src = './libs/yt-iframe.js';
    document.body.appendChild(tag);
    window.onYouTubeIframeAPIReady = () => {
      if (iframeRef.current) {
        playerRef.current = new window.YT.Player(iframeRef.current, {
          playerVars:{
            autoplay: 1,
            controls: 1,
            modestbranding: 1,
            rel: 0,
            showinfo: 0,
          },
          videoId,
          events: {
            onReady: (event) => {
              event.target.playVideo();
            },
          },
        });
      }
    };
  }, []);

  useEffect(() => {
    if (playerRef.current && videoId) {
      playerRef.current.cueVideoById(videoId);
    }
  }, [videoId]);

  return (<div style={containerStyles}>
    <div ref={iframeRef} style={{width: 'inherit', height: 'inherit'}}/>;
  </div>)
}