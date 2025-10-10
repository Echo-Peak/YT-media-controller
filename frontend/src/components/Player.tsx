import { useVideoSource } from '../providers/VideoSourceProvider';
import { useEffect, useState } from 'react';
import { EmbeddedYoutubePlayer } from './players/EmbeddedYoutubePlayer';
import { HLSPlayer } from './players/HLSPlayer';
import { NoVideoPlaying } from './dialogs/NoVideoPlaying';
import { DASHPlayer } from './players/DASHPlayer';
import { useInvokeApi } from '../services/useInvokeApi';

export const Player = () => {
  const { source, removeSource } = useVideoSource();
  const { openInExternalWindow } = useInvokeApi();
  const [HLSPlayerFailed, setHLSPlayerFailed] = useState(false);
  const [DASHPlayerFailed, setDASHPlayerFailed] = useState(false);

  useEffect(() => {
    if (!source) return;
    setHLSPlayerFailed(false);
    setDASHPlayerFailed(false);
  }, [source?.hlsStreamUrl, source?.dashStreamUrl, source?.originSource]);

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
    openInExternalWindow(source.originSource).catch(() => {
      console.log(
        `Failed to open URL in external window: ${source.originSource}`,
      );
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
