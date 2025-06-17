import React, { createContext, useContext, useState, ReactNode, useEffect } from 'react';
import { useBackendService } from '../services/backend/useSocketService';
import { DeviceInfoDto } from '../types/DeviceInfoDto';

type DeviceInfo = {
  deviceIp?: string;
  devicePort?: number;
};

const DeviceInfoContext = createContext<DeviceInfo>({
  deviceIp: undefined,
  devicePort: undefined,
});

export const useDeviceInfo = () => useContext(DeviceInfoContext);

type DeviceInfoProviderProps = {
  children: ReactNode;
};

export const DeviceInfoProvider: React.FC<DeviceInfoProviderProps> = ({ children }) => {
  const [deviceInfo, setDeviceInfo] = useState<DeviceInfo>({
    deviceIp: undefined,
    devicePort: undefined,
  });

    const wsService = useBackendService();
  
    useEffect(() => {
      const handleData = (payload: Record<string, unknown>) => {
        console.log('Received device info:', payload);
        if (payload.action === 'deviceInfo') {
          const data = payload.data as DeviceInfoDto;
          setDeviceInfo(data);
        }
      };
      wsService.onData(handleData);
      wsService.sendData({
        action: 'getDeviceInfo',
      })
    },[wsService])

  return (
    <DeviceInfoContext.Provider value={deviceInfo}>
      {children}
    </DeviceInfoContext.Provider>
  );
};