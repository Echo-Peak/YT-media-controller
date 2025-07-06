import { useVideoSource } from '../providers/VideoSourceProvider';
import { useState } from 'react';
import { EmbeddedYoutubePlayer } from './players/EmbeddedYoutubePlayer';
import { HLSPlayer } from './players/HLSPlayer';
import { NoVideoPlaying } from './dialogs/NoVideoPlaying';
import { DASHPlayer } from './players/DASHPlayer';

export const Player = () => {
  const { source } = useVideoSource();
  const [streamPlayerFailed, setStreamPlayerFailed] = useState(false);

  if (!source) {
    return <NoVideoPlaying />;
  }

  const onStreamError = (error: Error) => {
    console.error('Error loading video:', error);
    setStreamPlayerFailed(true);
  };

  const onUnableToPlayVideo = () => {
    console.error(
      'Unable to play video, opening via youtube.com in new tab:',
      source.originSource,
    );
    window.open(source.originSource, '_blank');
  };

  if (streamPlayerFailed) {
    if (source.dashStreamUrl) {
      return (
        <DASHPlayer
          sourceUrl={source.dashStreamUrl}
          videoData={source.videoData}
          onError={onStreamError}
        />
      );
    } else {
      return (
        <EmbeddedYoutubePlayer
          sourceUrl={source.originSource}
          onError={onUnableToPlayVideo}
        />
      );
    }
  }

  return (
    <HLSPlayer
      sourceUrl={source.hlsStreamUrl}
      videoData={source.videoData}
      onError={onStreamError}
    />
  );
};
