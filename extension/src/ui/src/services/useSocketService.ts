import { getChromeStorageKeys } from "./helpers/getChromeStorageKeys";
import io from 'socket.io-client';

const getWSUrl = async (): Promise<string> => {
  const { apiServerPort } = await getChromeStorageKeys();
  const port = apiServerPort || process.env.REACT_APP_API_SERVER_PORT;

  return `ws://127.0.0.1:${port}`;
}

export const useBackendService = async () => {
  const wsUrl = await getWSUrl();
  const socket = io(wsUrl);

  const onData = (callback: (data: Record<string, unknown>) => void) => {
    socket.on('data', callback);
  };

  const sendData = (data: Record<string, unknown>) => {
    socket.emit('data', data);
  };

  return { onData, sendData };
}