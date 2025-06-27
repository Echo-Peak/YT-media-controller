import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import { VideoSourceProvider } from './providers/VideoSourceProvider';
import { initBackendService } from './services/backend/useSocketService';
import { DeviceInfoProvider } from './providers/DeviceInfoProvider';
import { MobilePluginApp } from './MobilePluginApp';
import {
  ChakraProvider,
  createSystem,
  defaultConfig,
  defineConfig,
} from '@chakra-ui/react';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement,
);

const config = defineConfig({
  globalCss: {
    'html, body': {
      backgroundColor: 'gray.900',
      margin: 0,
      padding: 0,
    },
  },
});

const createMobilePluginApp = async () => {
  document.title = 'Mobile plugin setup';
  root.render(
    <React.StrictMode>
      <ChakraProvider value={createSystem(defaultConfig, config)}>
        <DeviceInfoProvider>
          <MobilePluginApp />
        </DeviceInfoProvider>
      </ChakraProvider>
    </React.StrictMode>,
  );
};

const createVideoPlayerApp = async () => {
  await initBackendService();
  document.title = 'Video Player';
  root.render(
    <React.StrictMode>
      <ChakraProvider value={createSystem(defaultConfig, config)}>
        <VideoSourceProvider>
          <DeviceInfoProvider>
            <App />
          </DeviceInfoProvider>
        </VideoSourceProvider>
      </ChakraProvider>
    </React.StrictMode>,
  );
};

(async () => {
  const urlParams = new URLSearchParams(window.location.search);
  const isMobilePluginSetup = urlParams.get('deviceIp') !== null;
  if (isMobilePluginSetup) {
    await createMobilePluginApp();
  } else {
    await createVideoPlayerApp();
  }
})();
