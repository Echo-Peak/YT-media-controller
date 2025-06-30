import { io } from "socket.io-client";

export type SocketInterface = {
  send: (data: Record<string, unknown>) => void;
  on: (event: string, callback: (data: any) => void) => void;
}
export const createSocket = (wsUrl: string): SocketInterface => {
    const socket = io(wsUrl, {
      transports: ['websocket']
    });
    socket.on('connect', () => {
      console.log('Connected to server');
    });

    return {
      send(data: Record<string, unknown>) {
        socket.emit(JSON.stringify(data));
      },
      on(event: string, callback: (data: any) => void) {
        socket.on(event, callback);
      },
    }
}

