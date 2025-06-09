import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import { VideoSourceProvider } from './providers/VideoSourceProvider';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);
root.render(
  <React.StrictMode>
    <VideoSourceProvider>
      <App />
    </VideoSourceProvider>
  </React.StrictMode>
);