// import React from 'react';
// import { render, screen, fireEvent, act } from '@testing-library/react';

// jest.mock('../providers/VideoSourceProvider', () => {
//   return {
//     useVideoSource: jest.fn(),
//   };
// });

// jest.mock('../services/useChromeRuntime', () => {
//   return {
//     useChromeRuntime: jest.fn(),
//   };
// });

// let hlsOnError: ((e: Error) => void) | undefined;
// let hlsOnEnded: (() => void) | undefined;
// let dashOnError: ((e: Error) => void) | undefined;
// let dashOnEnded: (() => void) | undefined;
// let embedOnError: ((e: Error) => void) | undefined;
// let embedOnEnded: (() => void) | undefined;

// jest.mock('./players/HLSPlayer', () => ({
//   HLSPlayer: (props: any) => {
//     hlsOnError = props.onError;
//     hlsOnEnded = props.onEnded;
//     return (
//       <div data-testid="HLSPlayer">
//         <div data-testid="hls-url">{props.sourceUrl}</div>
//         <button
//           data-testid="hls-error"
//           onClick={() => props.onError(new Error('hls'))}
//         />
//         <button data-testid="hls-ended" onClick={() => props.onEnded()} />
//       </div>
//     );
//   },
// }));

// jest.mock('./players/DASHPlayer', () => ({
//   DASHPlayer: (props: any) => {
//     dashOnError = props.onError;
//     dashOnEnded = props.onEnded;
//     return (
//       <div data-testid="DASHPlayer">
//         <div data-testid="dash-url">{props.sourceUrl}</div>
//         <button
//           data-testid="dash-error"
//           onClick={() => props.onError(new Error('dash'))}
//         />
//         <button data-testid="dash-ended" onClick={() => props.onEnded()} />
//       </div>
//     );
//   },
// }));

// jest.mock('./players/EmbeddedYoutubePlayer', () => ({
//   EmbeddedYoutubePlayer: (props: any) => {
//     embedOnError = props.onError;
//     embedOnEnded = props.onEnded;
//     return (
//       <div data-testid="EmbeddedYoutubePlayer">
//         <div data-testid="embed-url">{props.sourceUrl}</div>
//         <button
//           data-testid="embed-error"
//           onClick={() => props.onError(new Error('embed'))}
//         />
//         <button data-testid="embed-ended" onClick={() => props.onEnded()} />
//       </div>
//     );
//   },
// }));

// jest.mock('./dialogs/NoVideoPlaying', () => ({
//   NoVideoPlaying: () => <div data-testid="NoVideoPlaying" />,
// }));

// const { useVideoSource } = jest.requireMock('../providers/VideoSourceProvider');
// const { useChromeRuntime } = jest.requireMock('../services/useChromeRuntime');

// const Player = require('./Player').Player;

// type Source = {
//   originSource: string;
//   hlsStreamUrl?: string;
//   dashStreamUrl?: string;
//   videoData?: any;
// };

// let currentSource: Source | null = null;
// const removeSourceMock = jest.fn();
// const sendEventMock = jest.fn();

// const setSource = (s: Source | null) => {
//   currentSource = s;
// };

// beforeEach(() => {
//   currentSource = null;
//   hlsOnError = undefined;
//   hlsOnEnded = undefined;
//   dashOnError = undefined;
//   dashOnEnded = undefined;
//   embedOnError = undefined;
//   embedOnEnded = undefined;
//   removeSourceMock.mockReset();
//   sendEventMock.mockReset();

//   useVideoSource.mockImplementation(() => ({
//     source: currentSource,
//     removeSource: removeSourceMock,
//   }));

//   useChromeRuntime.mockImplementation(() => ({
//     sendEvent: sendEventMock,
//   }));
// });

// const renderPlayer = () => render(<Player />);

// test('renders NoVideoPlaying when there is no source', () => {
//   setSource(null);
//   renderPlayer();
//   expect(screen.getByTestId('NoVideoPlaying')).toBeInTheDocument();
// });

// test('renders HLSPlayer by default when source exists', () => {
//   setSource({
//     originSource: 'https://youtube.com/watch?v=abc',
//     hlsStreamUrl: 'https://cdn/hls.m3u8',
//     dashStreamUrl: 'https://cdn/manifest.mpd',
//   });
//   renderPlayer();
//   expect(screen.getByTestId('HLSPlayer')).toBeInTheDocument();
//   expect(screen.getByTestId('hls-url').textContent).toBe(
//     'https://cdn/hls.m3u8',
//   );
// });

// test('on HLS error, falls back to DASH when dashStreamUrl exists', () => {
//   setSource({
//     originSource: 'o',
//     hlsStreamUrl: 'h',
//     dashStreamUrl: 'd',
//   });
//   renderPlayer();
//   fireEvent.click(screen.getByTestId('hls-error'));
//   expect(screen.getByTestId('DASHPlayer')).toBeInTheDocument();
//   expect(screen.getByTestId('dash-url').textContent).toBe('d');
// });

// test('when both HLS and DASH fail, falls back to EmbeddedYoutubePlayer', () => {
//   setSource({
//     originSource: 'https://youtube.com/watch?v=xyz',
//     hlsStreamUrl: 'h',
//     dashStreamUrl: 'd',
//   });
//   renderPlayer();
//   fireEvent.click(screen.getByTestId('hls-error'));
//   fireEvent.click(screen.getByTestId('dash-error'));
//   expect(screen.getByTestId('EmbeddedYoutubePlayer')).toBeInTheDocument();
//   expect(screen.getByTestId('embed-url').textContent).toBe(
//     'https://youtube.com/watch?v=xyz',
//   );
// });

// test('if HLS fails and no dashStreamUrl, goes straight to EmbeddedYoutubePlayer', () => {
//   setSource({
//     originSource: 'https://youtube.com/watch?v=noDash',
//     hlsStreamUrl: 'h',
//   });
//   renderPlayer();
//   fireEvent.click(screen.getByTestId('hls-error'));
//   expect(screen.getByTestId('EmbeddedYoutubePlayer')).toBeInTheDocument();
//   expect(screen.getByTestId('embed-url').textContent).toBe(
//     'https://youtube.com/watch?v=noDash',
//   );
// });

// test('EmbeddedYoutubePlayer onError sends openInYTTab with originSource', () => {
//   setSource({
//     originSource: 'https://youtube.com/watch?v=send',
//     hlsStreamUrl: 'h',
//     dashStreamUrl: 'd',
//   });
//   renderPlayer();
//   fireEvent.click(screen.getByTestId('hls-error'));
//   fireEvent.click(screen.getByTestId('dash-error'));
//   act(() => {
//     embedOnError && embedOnError(new Error('x'));
//   });
//   expect(sendEventMock).toHaveBeenCalledWith({
//     action: 'openInYTTab',
//     data: { url: 'https://youtube.com/watch?v=send' },
//   });
// });

// test('onEnded from HLS calls removeSource', () => {
//   setSource({ originSource: 'o', hlsStreamUrl: 'h', dashStreamUrl: 'd' });
//   render(<Player />);
//   fireEvent.click(screen.getByTestId('hls-ended'));
//   expect(removeSourceMock).toHaveBeenCalledTimes(1);
// });

// test('onEnded from DASH calls removeSource (after HLS fails)', () => {
//   setSource({ originSource: 'o', hlsStreamUrl: 'h', dashStreamUrl: 'd' });
//   render(<Player />);
//   fireEvent.click(screen.getByTestId('hls-error')); // switch to DASH
//   removeSourceMock.mockClear();
//   fireEvent.click(screen.getByTestId('dash-ended'));
//   expect(removeSourceMock).toHaveBeenCalledTimes(1);
// });

// test('onEnded from Embedded calls removeSource (when no DASH)', () => {
//   setSource({ originSource: 'o', hlsStreamUrl: 'h' });
//   render(<Player />);
//   fireEvent.click(screen.getByTestId('hls-error')); // goes straight to Embedded
//   removeSourceMock.mockClear();
//   fireEvent.click(screen.getByTestId('embed-ended'));
//   expect(removeSourceMock).toHaveBeenCalledTimes(1);
// });

// test('resets failure flags when source changes (back to HLS)', () => {
//   setSource({
//     originSource: 'o1',
//     hlsStreamUrl: 'h1',
//     dashStreamUrl: 'd1',
//   });
//   const { rerender } = render(<Player />);
//   fireEvent.click(screen.getByTestId('hls-error'));
//   expect(screen.getByTestId('DASHPlayer')).toBeInTheDocument();

//   setSource({
//     originSource: 'o2',
//     hlsStreamUrl: 'h2',
//     dashStreamUrl: 'd2',
//   });
//   rerender(<Player />);
//   expect(screen.getByTestId('HLSPlayer')).toBeInTheDocument();
//   expect(screen.getByTestId('hls-url').textContent).toBe('h2');
// });
