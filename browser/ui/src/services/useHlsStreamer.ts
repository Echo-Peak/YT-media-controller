import Hls, { HlsConfig } from 'hls.js';
import { useRef } from 'react';

const hlsConfig: Partial<HlsConfig> = {
  maxBufferLength: 10,
  maxBufferSize: 30 * 1000 * 1000,
  maxBufferHole: 0.5,
  liveMaxLatencyDuration: 10,
  liveSyncDuration: 5,
  backBufferLength: 0,
  fragLoadingTimeOut: 60000,
  manifestLoadingTimeOut: 60000,
  xhrSetup: (xhr: XMLHttpRequest, url) => {
    xhr.timeout = 60000; // Set timeout for XHR requests
    xhr.ontimeout = () => {
      console.warn('XHR request timed out:', url);
    };
  },
};

const createHls = () => {
  if (Hls.isSupported()) {
    console.log('HLS is supported on this device');
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
  const cleanupStream = () => {
    if (hlsRef.current) {
      hlsRef.current.stopLoad();
      hlsRef.current.detachMedia();
      hlsRef.current.destroy();
      hlsRef.current = undefined;
    }
  };

  const loadStream = (streamUrl: string, mediaElement: HTMLMediaElement) => {
    if (hlsRef.current) {
      hlsRef.current.destroy();
    }
    hlsRef.current = createHls();
    hlsRef.current.loadSource(streamUrl);
    hlsRef.current.attachMedia(mediaElement);

    hlsRef.current.on(Hls.Events.ERROR, (event, data) => {
      const isNetworkError = data.type === Hls.ErrorTypes.NETWORK_ERROR;
      const isForbidden =
        (data.networkDetails as XMLHttpRequest).status === 403;

      if ((isNetworkError && isForbidden) || data.fatal) {
        cleanupStream();
        onFatalError(new Error(`HLS Error: ${data.type} - ${data.details}`));
        return;
      }
      console.warn('HLS non-fatal error:', data);
    });
  };

  return {
    loadStream,
    cleanupStream,
  };
};
