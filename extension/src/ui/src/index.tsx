import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import { VideoSourceProvider } from './providers/VideoSourceProvider';
import { initBackendService } from './services/backend/useSocketService';
import { DeviceInfoProvider } from './providers/DeviceInfoProvider';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

(async () => {
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
})();