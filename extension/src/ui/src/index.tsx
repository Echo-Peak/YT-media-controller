import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import { VideoSourceProvider } from './providers/VideoSourceProvider';
import { initBackendService } from './services/backend/useSocketService';
import { DeviceInfoProvider } from './providers/DeviceInfoProvider';
import { MobilePluginApp } from './MobilePluginApp';
import { ChakraProvider, createSystem, defaultConfig, defineConfig } from '@chakra-ui/react';


const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

const config = defineConfig({
  globalCss: {
    "html, body": {
      background: "#222",
      margin: 0,
      padding: 0,
    },
  }
})
const system = createSystem(defaultConfig, config)

const createMobilePluginApp = async () => {
  document.title = "Mobile plugin setup";
    root.render(
    <React.StrictMode>
      <ChakraProvider value={system}>
        <DeviceInfoProvider>
          <MobilePluginApp />
        </DeviceInfoProvider>
      </ChakraProvider>
    </React.StrictMode>
    );
}

const createVideoPlayerApp = async () => {
  await initBackendService();
  document.title = "Video Player";
  root.render(
    <React.StrictMode>
        <ChakraProvider value={system}>
          <VideoSourceProvider>
            <DeviceInfoProvider>
              <App />
            </DeviceInfoProvider>
          </VideoSourceProvider>
        </ChakraProvider>
    </React.StrictMode>
  );
}

(async () => {
  const urlParams = new URLSearchParams(window.location.search);
  const isMobilePluginSetup = urlParams.get('deviceIp') !== null;
  if (isMobilePluginSetup) {
    await createMobilePluginApp();
  } else {
    await createVideoPlayerApp();
  }
  
})();