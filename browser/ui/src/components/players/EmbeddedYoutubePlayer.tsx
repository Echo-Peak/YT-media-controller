import { useEffect, useLayoutEffect, useRef } from 'react';

type Props = {
  sourceUrl: string;
  onError: (err: Error) => void;
  onEnded: () => void;
};

declare global {
  interface Window {
    onYouTubeIframeAPIReady: () => void;
    YT: typeof YT;
  }
}

const notAllowedInEmbeddedPlayers =
  'The owner of the requested video does not allow it to be played in embedded players.';

const ytErrorDescriptionMap = {
  2: 'The request contains an invalid parameter value.',
  5: 'The requested content cannot be played in an HTML5 player.',
  100: 'The video requested was not found.',
  101: notAllowedInEmbeddedPlayers,
  150: notAllowedInEmbeddedPlayers,
  200: 'The video is unavailable.',
  201: 'The video is private.',
};

const containerStyles: React.CSSProperties = {
  width: '100vw',
  height: '100vh',
  position: 'fixed',
  overflow: 'hidden',
  zIndex: 9999,
};

const extractVideoId = (url: string): string => {
  const lastPart = url.split('/').pop();
  if (lastPart?.includes('watch?v=')) {
    return lastPart.split('watch?v=')[1];
  }
  return lastPart || '';
};

export const EmbeddedYoutubePlayer = ({
  sourceUrl,
  onError,
  onEnded,
}: Props) => {
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
          playerVars: {
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
            onStateChange: (event) => {
              if (event.data === window.YT.PlayerState.ENDED) {
                onEnded();
              }
            },
            onError: (eventCode) => {
              const err =
                ytErrorDescriptionMap[eventCode.data] ||
                `Unknown error - ${eventCode.data}`;
              onError(new Error(`YouTube Player Error: ${err}`));
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

  useLayoutEffect(() => {}, []);

  return (
    <div style={containerStyles}>
      <div ref={iframeRef} style={{ width: 'inherit', height: 'inherit' }} />;
    </div>
  );
};
