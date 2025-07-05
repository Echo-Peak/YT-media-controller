export type YTUrlSource = {
  originSource: string;
  dashStreamUrl: string;
  hlsStreamUrl: string;
  videoData: YTVideoData;
};

export type YTVideoData = {
  title: string;
  uploader: string;
};
