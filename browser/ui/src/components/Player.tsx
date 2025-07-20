import { useVideoSource } from '../providers/VideoSourceProvider';
import { useState } from 'react';
import { EmbeddedYoutubePlayer } from './players/EmbeddedYoutubePlayer';
import { HLSPlayer } from './players/HLSPlayer';
import { NoVideoPlaying } from './dialogs/NoVideoPlaying';
import { DASHPlayer } from './players/DASHPlayer';
import { useChromeRuntime } from '../services/useChromeRuntime';

export const Player = () => {
  const { source, removeSource } = useVideoSource();
  const { sendEvent } = useChromeRuntime();
  const [HLSPlayerFailed, setHLSPlayerFailed] = useState(false);
  const [DASHPlayerFailed, setDASHPlayerFailed] = useState(false);

  if (!source) {
    return <NoVideoPlaying />;
  }

  const onVideoEnded = () => {
    removeSource();
  };

  const onHLSStreamError = (error: Error) => {
    console.error('Error loading video via HLS:', error);
    setHLSPlayerFailed(true);
  };

  const onDASHStreamError = (error: Error) => {
    console.error('Error loading video via DASH:', error);
    setDASHPlayerFailed(true);
  };

  const onEmbeddedYoutubePlayerError = (error: Error) => {
    console.error(error);
    sendEvent({
      action: 'openInYTTab',
      data: {
        url: source.originSource,
      },
    });
  };

  const streamPlayersFailed = HLSPlayerFailed && DASHPlayerFailed;
  const unableToUserDASH = HLSPlayerFailed && !source.dashStreamUrl;

  if (streamPlayersFailed || unableToUserDASH) {
    return (
      <EmbeddedYoutubePlayer
        sourceUrl={source.originSource}
        onError={onEmbeddedYoutubePlayerError}
        onEnded={onVideoEnded}
      />
    );
  }

  if (HLSPlayerFailed) {
    return (
      <DASHPlayer
        sourceUrl={source.dashStreamUrl}
        videoData={source.videoData}
        onError={onDASHStreamError}
        onEnded={onVideoEnded}
      />
    );
  }

  return (
    <HLSPlayer
      sourceUrl={source.hlsStreamUrl}
      videoData={source.videoData}
      onError={onHLSStreamError}
      onEnded={onVideoEnded}
    />
  );
};
