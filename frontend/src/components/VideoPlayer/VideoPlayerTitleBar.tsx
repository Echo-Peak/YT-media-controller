import styled from '@emotion/styled';

const Titlebar = styled.div({
  backgroundColor: 'rgba(0, 0, 0, 0.7)',
  padding: '10px 15px',
  position: 'absolute',
  top: 0,
  left: 0,
  right: 0,
  zIndex: 10001,
});

const Title = styled.h3({
  margin: 0,
  fontSize: 18,
  color: '#fff',
});

export type VideoPlayerTitleBarProps = {
  videoData?: {
    title: string;
    uploader: string;
  };
  show: boolean;
};

export const VideoPlayerTitleBar = (props: VideoPlayerTitleBarProps) => {
  const { videoData, show } = props;
  return (
    <Titlebar style={{ display: show ? 'block' : 'none' }}>
      <Title>
        {videoData?.uploader} - {videoData?.title}
      </Title>
    </Titlebar>
  );
};
