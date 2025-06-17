import React, { createContext, useContext, useState, ReactNode, useEffect } from 'react';
import { getChromeStorageKeys } from '../services/helpers/getChromeStorageKeys';

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

    useEffect(() => {
      getChromeStorageKeys().then(data => {
        const {backendServicePort, deviceNetworkIp} = data as Record<string, unknown>;
        setDeviceInfo({
          deviceIp: deviceNetworkIp as string | undefined,
          devicePort: backendServicePort ? parseInt(backendServicePort as string, 10) : undefined,
        })
      }).catch(err => {
        console.error("Error fetching Chrome storage keys:", err);
      })
    },[])

  return (
    <DeviceInfoContext.Provider value={deviceInfo}>
      {children}
    </DeviceInfoContext.Provider>
  );
};