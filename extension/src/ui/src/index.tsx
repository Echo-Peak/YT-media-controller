import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import { VideoSourceProvider } from './providers/VideoSourceProvider';
import { initBackendService } from './services/backend/useSocketService';
import { DeviceInfoProvider } from './providers/DeviceInfoProvider';
import { MobilePluginApp } from './MobilePluginApp';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

const createMobilePluginApp = async () => {
    root.render(
    <React.StrictMode>
      <DeviceInfoProvider>
        <MobilePluginApp />
      </DeviceInfoProvider>
    </React.StrictMode>
    );
}

const createVideoPlayerApp = async () => {
  await initBackendService();
  root.render(
    <React.StrictMode>
      <VideoSourceProvider>
        <DeviceInfoProvider>
          <App />
        </DeviceInfoProvider>
      </VideoSourceProvider>
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