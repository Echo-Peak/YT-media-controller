import { useEffect, RefObject } from 'react';

export function useSpacebarPlayToggle(
  playerRef: RefObject<HTMLVideoElement | null>,
) {
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.code === 'Space' || event.key === ' ') {
        event.preventDefault();
        const player = playerRef.current;
        if (player) {
          player.paused ? player.play() : player.pause();
        }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [playerRef]);
}
