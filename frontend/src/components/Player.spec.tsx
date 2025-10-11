/* eslint-disable @typescript-eslint/no-require-imports */
/* eslint-disable @typescript-eslint/no-unsafe-assignment */
/* eslint-disable @typescript-eslint/no-unsafe-member-access */
/* eslint-disable @typescript-eslint/no-unsafe-call */
/* eslint-disable @typescript-eslint/no-unsafe-return */
import { render, screen, fireEvent, act } from '@testing-library/react';
import { Player } from './Player';

jest.mock('../providers/VideoSourceProvider', () => ({
  useVideoSource: jest.fn(),
}));

jest.mock('../services/useInvokeApi', () => ({
  useInvokeApi: jest.fn(),
}));

jest.mock('./players/EmbeddedYoutubePlayer', () => ({
  EmbeddedYoutubePlayer: (props: any) => (
    <div data-testid="EmbeddedYoutubePlayer">
      <button onClick={() => props.onError(new Error('embedded'))}>
        embedded-error
      </button>
      <button onClick={props.onEnded}>embedded-ended</button>
      <span>{props.sourceUrl}</span>
    </div>
  ),
}));

jest.mock('./players/HLSPlayer', () => ({
  HLSPlayer: (props: any) => (
    <div data-testid="HLSPlayer">
      <button onClick={() => props.onError(new Error('hls'))}>hls-error</button>
      <button onClick={props.onEnded}>hls-ended</button>
      <span>{props.sourceUrl}</span>
    </div>
  ),
}));

jest.mock('./players/DASHPlayer', () => ({
  DASHPlayer: (props: any) => (
    <div data-testid="DASHPlayer">
      <button onClick={() => props.onError(new Error('dash'))}>
        dash-error
      </button>
      <button onClick={props.onEnded}>dash-ended</button>
      <span>{props.sourceUrl}</span>
    </div>
  ),
}));

jest.mock('./dialogs/NoVideoPlaying', () => ({
  NoVideoPlaying: () => <div data-testid="NoVideoPlaying" />,
}));

const useVideoSource = require('../providers/VideoSourceProvider')
  .useVideoSource as jest.Mock;
const useInvokeApi = require('../services/useInvokeApi')
  .useInvokeApi as jest.Mock;

const makeSource = (
  overrides?: Partial<{
    originSource: string;
    hlsStreamUrl: string | null;
    dashStreamUrl: string | null;
    videoData: any;
  }>,
) => ({
  originSource: 'https://youtube.com/watch?v=abc',
  hlsStreamUrl: 'http://hls/stream.m3u8',
  dashStreamUrl: 'http://dash/manifest.mpd',
  videoData: { id: 'v1' },
  ...overrides,
});

describe('Player', () => {
  let removeSource: jest.Mock;
  let openInExternalWindow: jest.Mock;

  beforeEach(() => {
    removeSource = jest.fn();
    openInExternalWindow = jest.fn().mockResolvedValue(undefined);
    useInvokeApi.mockReturnValue({ openInExternalWindow });
    jest.spyOn(console, 'error').mockImplementation(() => {});
    jest.spyOn(console, 'log').mockImplementation(() => {});
  });

  afterEach(() => {
    jest.resetAllMocks();
  });

  test('renders NoVideoPlaying when no source', () => {
    useVideoSource.mockReturnValue({ source: undefined, removeSource });
    render(<Player />);
    expect(screen.getByTestId('NoVideoPlaying')).toBeInTheDocument();
  });

  test('renders HLSPlayer by default when source exists', () => {
    useVideoSource.mockReturnValue({ source: makeSource(), removeSource });
    render(<Player />);
    expect(screen.getByTestId('HLSPlayer')).toBeInTheDocument();
  });

  test('on HLS error with DASH available, switches to DASHPlayer', () => {
    useVideoSource.mockReturnValue({ source: makeSource(), removeSource });
    render(<Player />);
    fireEvent.click(screen.getByText('hls-error'));
    expect(screen.getByTestId('DASHPlayer')).toBeInTheDocument();
  });

  test('on HLS error without DASH url, falls back to EmbeddedYoutubePlayer', () => {
    useVideoSource.mockReturnValue({
      source: makeSource({ dashStreamUrl: null }),
      removeSource,
    });
    render(<Player />);
    fireEvent.click(screen.getByText('hls-error'));
    expect(screen.getByTestId('EmbeddedYoutubePlayer')).toBeInTheDocument();
    expect(
      screen.getByText('https://youtube.com/watch?v=abc'),
    ).toBeInTheDocument();
  });

  test('when both HLS and DASH fail, falls back to EmbeddedYoutubePlayer', () => {
    useVideoSource.mockReturnValue({ source: makeSource(), removeSource });
    render(<Player />);
    fireEvent.click(screen.getByText('hls-error'));
    expect(screen.getByTestId('DASHPlayer')).toBeInTheDocument();
    fireEvent.click(screen.getByText('dash-error'));
    expect(screen.getByTestId('EmbeddedYoutubePlayer')).toBeInTheDocument();
  });

  test('HLS onEnded calls removeSource', () => {
    useVideoSource.mockReturnValue({ source: makeSource(), removeSource });
    render(<Player />);
    fireEvent.click(screen.getByText('hls-ended'));
    expect(removeSource).toHaveBeenCalledTimes(1);
  });

  test('DASH onEnded calls removeSource', () => {
    useVideoSource.mockReturnValue({ source: makeSource(), removeSource });
    render(<Player />);
    fireEvent.click(screen.getByText('hls-error'));
    fireEvent.click(screen.getByText('dash-ended'));
    expect(removeSource).toHaveBeenCalledTimes(1);
  });

  test('Embedded onError tries to open external window with originSource', async () => {
    useVideoSource.mockReturnValue({
      source: makeSource({ dashStreamUrl: null }),
      removeSource,
    });
    render(<Player />);
    fireEvent.click(screen.getByText('hls-error'));
    await act(async () => {
      fireEvent.click(screen.getByText('embedded-error'));
    });
    expect(openInExternalWindow).toHaveBeenCalledWith(
      'https://youtube.com/watch?v=abc',
    );
  });

  test('Embedded onError handles rejection path (logs failure)', async () => {
    openInExternalWindow.mockRejectedValueOnce(new Error('fail'));
    useVideoSource.mockReturnValue({
      source: makeSource({ dashStreamUrl: null }),
      removeSource,
    });
    render(<Player />);
    fireEvent.click(screen.getByText('hls-error'));
    await act(async () => {
      fireEvent.click(screen.getByText('embedded-error'));
    });
    expect(openInExternalWindow).toHaveBeenCalledTimes(1);
    expect(console.log).toHaveBeenCalledWith(
      'Failed to open URL in external window: https://youtube.com/watch?v=abc',
    );
  });

  test('error flags reset when source changes', () => {
    const first = makeSource({
      originSource: 'https://youtube.com/watch?v=one',
    });
    const second = makeSource({
      originSource: 'https://youtube.com/watch?v=two',
      hlsStreamUrl: 'http://hls/stream2.m3u8',
    });

    let current = first;
    useVideoSource.mockImplementation(() => ({
      source: current,
      removeSource,
    }));

    const { rerender } = render(<Player />);
    fireEvent.click(screen.getByText('hls-error'));
    expect(screen.getByTestId('DASHPlayer')).toBeInTheDocument();

    current = second;
    rerender(<Player />);
    expect(screen.getByTestId('HLSPlayer')).toBeInTheDocument();
  });
});
