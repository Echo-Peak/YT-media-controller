import { useVideoSource } from '../providers/VideoSourceProvider';
import { useState } from 'react';
import { YoutubePlayer } from './players/YoutubePlayer';
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
      return <YoutubePlayer sourceUrl={source.originSource} />;
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
