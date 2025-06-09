import ReactPlayer from 'react-player'
import { useVideoSource } from '../providers/VideoSourceProvider';
import { useState } from 'react';

export const YTVideoPlayer = () => {
  const { source } = useVideoSource();
  const [isPlaying , setIsPlaying] = useState(false);

    if(!source) {
        return <div>Waiting for YouTube video...</div>;
    }

    const handleError = (error: any) => {
        console.error("Error loading video:", error);
    }

    const onReady = () => {
        setIsPlaying(true);
    }

    
    return (<div style={{ width: '100vw', height: '100vh', overflow: 'hidden' }}>
        <ReactPlayer 
        width="100%"
        height="100%"
        controls={true}
        url={source} 
        onError={handleError} 
        playing={isPlaying}
        onReady={onReady}
    />
    </div>
    )
}