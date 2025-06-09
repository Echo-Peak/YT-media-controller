import React, { createContext, useContext, useState, ReactNode } from 'react';

interface VideoSourceContextType {
  source: string | null;
}

const VideoSourceContext = createContext<VideoSourceContextType | undefined>(undefined);

export const VideoSourceProvider = ({ children }: { children: ReactNode }) => {
  const [source, setSource] = useState<string | null>("https://manifest.googlevideo.com/api/manifest/hls_variant/expire/1749347812/ei/hJlEaNOrAvLcybgPvJqA4AE/ip/70.76.29.239/id/d0eaa5f1dbe63cd5/source/youtube/requiressl/yes/xpc/EgVo2aDSNQ%3D%3D/playback_host/rr2---sn-ni5f-t5he.googlevideo.com/met/1749326212%2C/mh/gX/mm/31%2C29/mn/sn-ni5f-t5he%2Csn-tt1e7nlz/ms/au%2Crdu/mv/m/mvi/2/pl/22/rms/au%2Cau/hfr/1/demuxed/1/tts_caps/1/maudio/1/initcwndbps/3520000/bui/AY1jyLN3yAFYt_0ShM2JG4p1O8D-Je1-boDwXjfS0RewnE5m-SQkmn9mE0eapm0pEshqwMvGqiLIsnk0/spc/l3OVKXoesR_NZYaJ8ycyuuSa_SJSDYkXxcel6GC9Lplr9fD1sak/vprv/1/go/1/rqh/5/mt/1749325775/fvip/2/nvgoi/1/short_key/1/ncsapi/1/keepalive/yes/dover/13/itag/0/playlist_type/DVR/sparams/expire%2Cei%2Cip%2Cid%2Csource%2Crequiressl%2Cxpc%2Chfr%2Cdemuxed%2Ctts_caps%2Cmaudio%2Cbui%2Cspc%2Cvprv%2Cgo%2Crqh%2Citag%2Cplaylist_type/sig/AJfQdSswRgIhAIvH3YZkXonoMfG2m9JojSdvpZYv2SctcRZVHQqcotkOAiEAu20WORGbdj1dSr57KW3Qer94mAttp5FfxovspbDYQ1U%3D/lsparams/playback_host%2Cmet%2Cmh%2Cmm%2Cmn%2Cms%2Cmv%2Cmvi%2Cpl%2Crms%2Cinitcwndbps/lsig/APaTxxMwRQIhAN-5oc7Vvpm_bnN_4QiT5cVccnsq4peZukRe6jJz0BsgAiA6dSugzXKoZ6Rall8I-3ltzvuoNiwcG8pMk7lIFiDgUg%3D%3D/file/index.m3u8");

  return (
    <VideoSourceContext.Provider value={{ source }}>
      {children}
    </VideoSourceContext.Provider>
  );
};

export const useVideoSource = () => {
  const context = useContext(VideoSourceContext);
  if (!context) {
    throw new Error('useVideoSource must be used within a VideoSourceProvider');
  }
  return context;
};