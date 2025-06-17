import React, { createContext, useContext, useState, ReactNode, useRef, useEffect } from 'react';
import { YTUrlSource } from '../types/YTUrlSource';
import { useBackendService } from '../services/backend/useSocketService';

interface VideoSourceContextType {
  source: YTUrlSource | undefined;
}

const VideoSourceContext = createContext<VideoSourceContextType | undefined>(undefined);

export const VideoSourceProvider = ({ children }: { children: ReactNode }) => {
  const [source, setSource] = useState<YTUrlSource | undefined>(undefined);
  const currentPlayData = useRef<YTUrlSource | undefined>(undefined);

  const wsService = useBackendService();

  useEffect(() => {
    const handleData = (payload: Record<string, unknown>) => {
      if (payload.action === 'playVideo') {
        console.log('Received playVideo data:', payload);
        const data = payload.data as YTUrlSource;
        currentPlayData.current = data;
        setSource(data);
      }
    };
    wsService.onData(handleData);
  },[wsService])



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