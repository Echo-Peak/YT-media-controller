
import { useVideoSource } from '../providers/VideoSourceProvider';
import { useState } from 'react';
import { YoutubePlayer } from './players/YoutubePlayer';
import { StreamPlayer } from './players/StreamPlayer';
import { VideoPlayer } from './players/VideoPlayer';

export const Player = () => {
    const { source } = useVideoSource();
    const [streamPlayerFailed, setStreamPlayerFailed] = useState(false);

    if(!source) {
        return <div style={{ color: 'white', textAlign: 'center', marginTop: '20px' }}>No video source available</div>;
    }

    const onStreamError = (error: Error) => {
        console.error("Error loading video:", error);
        setStreamPlayerFailed(true);
    }


    if(streamPlayerFailed) {
        console.log('----------------', source);
        
        if(source.dashStreamUrl){
            return <VideoPlayer sourceUrl={source.dashStreamUrl} />;
        }else{
            return <YoutubePlayer sourceUrl={source.originSource} />
        }
    }

    return <StreamPlayer sourceUrl={source.hlsStreamUrl} onError={onStreamError}/>;

}