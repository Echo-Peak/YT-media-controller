import styled from '@emotion/styled';
import { IconButton } from '@chakra-ui/react';
import { FaPlay } from 'react-icons/fa';
import { FaPause } from 'react-icons/fa';
import { FaMaximize } from 'react-icons/fa6';
import { TbArrowsMinimize } from 'react-icons/tb';
import { useEffect } from 'react';

const ControlBarContainer = styled.div({
  backgroundColor: 'rgba(0, 0, 0, 0.7)',
  display: 'flex',
  alignItems: 'center',
  gap: 10,
  padding: 10,
  flexWrap: 'wrap',
  position: 'absolute',
  bottom: 0,
  left: 0,
  right: 0,
  zIndex: 10001,
});

const Button = styled(IconButton)({
  border: 'none',
  padding: '6px 12px',
  cursor: 'pointer',
});

const Slider = styled.input({
  flex: 1,
  accentColor: 'red',
});

export type VideoPlayerControlBarProps = {
  togglePlay: () => void;
  isPlaying: boolean;
  currentTime: number;
  duration: number;
  handleSeek: (event: React.ChangeEvent<HTMLInputElement>) => void;
  toggleFullscreen: () => void;
  isFullscreen: boolean;
  show: boolean;
};

const formatTime = (seconds: number) => {
  const mins = Math.floor(seconds / 60);
  const secs = Math.floor(seconds % 60);
  return `${mins}:${secs.toString().padStart(2, '0')}`;
};

const toggleCursor = (show: boolean) => {
  document.body.style.cursor = show ? 'default' : 'none';
};

export const VideoPlayerControlBar = (props: VideoPlayerControlBarProps) => {
  const {
    togglePlay,
    isPlaying,
    currentTime,
    duration,
    handleSeek,
    toggleFullscreen,
    isFullscreen,
    show,
  } = props;

  useEffect(() => {
    toggleCursor(show);
  }, [show]);

  return (
    <ControlBarContainer style={{ display: show ? 'flex' : 'none' }}>
      <Button onClick={togglePlay}>
        {isPlaying ? <FaPause color="white" /> : <FaPlay color="white" />}
      </Button>
      <span style={{ color: 'white' }}>
        {formatTime(currentTime)} / {formatTime(duration)}
      </span>
      <Slider
        type="range"
        min={0}
        max={duration}
        value={currentTime}
        step={0.1}
        onChange={handleSeek}
      />

      <Button onClick={toggleFullscreen}>
        {isFullscreen ? (
          <TbArrowsMinimize color="white" />
        ) : (
          <FaMaximize color="white" />
        )}
      </Button>
    </ControlBarContainer>
  );
};
