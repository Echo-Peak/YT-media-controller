import { YTVideoData } from './YTUrlSource';

export type StreamablePlayerProps = {
  sourceUrl: string;
  videoData?: YTVideoData;
  onError: (error: Error) => void;
  onEnded: () => void;
};
