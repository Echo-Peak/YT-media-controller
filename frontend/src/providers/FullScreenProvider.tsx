import {
  createContext,
  useContext,
  useState,
  ReactNode,
  useRef,
  useCallback,
} from 'react';
import { useInvokeApi } from '../services/useInvokeApi';

interface FullScreenContextType {
  isFullscreen: boolean;
  toggleFullscreen: () => Promise<void>;
}

const FullScreenContext = createContext<FullScreenContextType | undefined>(
  undefined,
);

export const FullScreenProvider = ({
  children,
}: {
  children: ReactNode;
}) => {
  const { enterFullscreen, exitFullscreen } = useInvokeApi();
  const [isFullscreen, setIsFullscreen] = useState(false);
  const togglingRef = useRef(false);

  const toggleFullscreen = useCallback(async () => {
    if (togglingRef.current) return;
    togglingRef.current = true;
    try {
      if (isFullscreen) {
        await exitFullscreen();
        setIsFullscreen(false);
      } else {
        await enterFullscreen();
        setIsFullscreen(true);
      }
    } finally {
      togglingRef.current = false;
    }
  }, [isFullscreen, enterFullscreen, exitFullscreen]);

  return (
    <FullScreenContext.Provider value={{ isFullscreen, toggleFullscreen }}>
      {children}
    </FullScreenContext.Provider>
  );
};

export const useFullScreen = () => {
  const context = useContext(FullScreenContext);
  if (!context) {
    throw new Error('useFullScreen must be used within a FullScreenProvider');
  }
  return context;
};

