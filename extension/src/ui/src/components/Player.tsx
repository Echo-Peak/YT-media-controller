
import { useVideoSource } from '../providers/VideoSourceProvider';
import { useState } from 'react';
import { YoutubePlayer } from './players/YoutubePlayer';
import { StreamPlayer } from './players/StreamPlayer';
import { VideoPlayer } from './players/VideoPlayer';
import { NoVideoPlaying } from './dialogs/NoVideoPlaying';

export const Player = () => {
    const { source } = useVideoSource();
    const [streamPlayerFailed, setStreamPlayerFailed] = useState(false);

    if(!source) {
        return <NoVideoPlaying />;
    }

    const onStreamError = (error: Error) => {
        console.error("Error loading video:", error);
        setStreamPlayerFailed(true);
    }


    if(streamPlayerFailed) {
        if(source.dashStreamUrl){
            return <VideoPlayer sourceUrl={source.dashStreamUrl} />;
        }else{
            return <YoutubePlayer sourceUrl={source.originSource} />
        }
    }

    return <StreamPlayer sourceUrl={source.hlsStreamUrl} onError={onStreamError}/>;

}