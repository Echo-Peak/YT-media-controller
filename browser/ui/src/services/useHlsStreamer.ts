import Hls from 'hls.js';
import { useRef } from 'react';

const hlsConfig = {
  maxBufferLength: 10,
  maxMaxBufferLength: 60,
  enableWorker: true,
  progressive: true,
  maxBufferSize: 30 * 1000 * 1000,
  maxBufferHole: 0.5,
  liveMaxLatencyDuration: 10,
  liveSyncDuration: 5,
  backBufferLength: 0,
};

const createHls = () => {
  if (Hls.isSupported()) {
    return new Hls(hlsConfig);
  } else {
    throw new Error('HLS is not supported on this device');
  }
};

export type useHlsStreamerArgs = {
  onFatalError: (error: Error) => void;
};
export type HlsStreamer = {
  loadStream: (streamUrl: string, mediaElement: HTMLMediaElement) => void;
  cleanupStream: () => void;
};

export const useHlsStreamer = (
  onFatalError: (error: Error) => void,
): HlsStreamer => {
  const hlsRef = useRef<Hls | undefined>(undefined);

  const loadStream = (streamUrl: string, mediaElement: HTMLMediaElement) => {
    if (hlsRef.current) {
      hlsRef.current.destroy();
    }
    hlsRef.current = createHls();
    hlsRef.current.loadSource(streamUrl);
    hlsRef.current.attachMedia(mediaElement);
    hlsRef.current.on(Hls.Events.ERROR, (event, data) => {
      if (data.fatal) {
        onFatalError(new Error(`HLS Error: ${data.type} - ${data.details}`));
      } else {
        console.warn('HLS non-fatal error:', data);
      }
    });
  };

  const cleanupStream = () => {
    if (hlsRef.current) {
      hlsRef.current.stopLoad();
      hlsRef.current.detachMedia();
      hlsRef.current.destroy();
      hlsRef.current = undefined;
    }
  };

  return {
    loadStream,
    cleanupStream,
  };
};
