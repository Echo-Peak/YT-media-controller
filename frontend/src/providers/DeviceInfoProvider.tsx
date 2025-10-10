import React, {
  createContext,
  useContext,
  useState,
  ReactNode,
  useEffect,
} from 'react';
import { useInvokeApi } from '../services/useInvokeApi';

type DeviceInfo = {
  deviceIp?: string;
  devicePort?: number;
  uiSocketServerPort?: number;
  connectionError?: string;
};

const DeviceInfoContext = createContext<DeviceInfo>({
  deviceIp: undefined,
  devicePort: undefined,
  uiSocketServerPort: undefined,
  connectionError: undefined,
});

export const useDeviceInfo = () => useContext(DeviceInfoContext);

type DeviceInfoProviderProps = {
  children: ReactNode;
};

const convertToInt = (value: unknown): number | undefined => {
  if (typeof value === 'number') {
    return value;
  }
  if (typeof value === 'string') {
    const parsed = parseInt(value, 10);
    return isNaN(parsed) ? undefined : parsed;
  }
  return undefined;
};

export const DeviceInfoProvider: React.FC<DeviceInfoProviderProps> = ({
  children,
}) => {
  const [deviceInfo, setDeviceInfo] = useState<DeviceInfo>({
    deviceIp: undefined,
    devicePort: undefined,
    uiSocketServerPort: undefined,
    connectionError: undefined,
  });

  const invokeApi = useInvokeApi();

  useEffect(() => {
    invokeApi
      .getAppSettings()
      .then((result) => {
        console.log('Fetched app settings:', result);
        setDeviceInfo({
          deviceIp: result.deviceIp as string,
          devicePort: convertToInt(result.devicePort),
          uiSocketServerPort: convertToInt(result.uiSocketServerPort),
          connectionError: undefined,
        });
      })
      .catch((err) => {
        console.error('Failed to fetch app settings:', err);
        setDeviceInfo({
          deviceIp: undefined,
          devicePort: undefined,
          uiSocketServerPort: undefined,
          connectionError: 'Failed to connect to backend',
        });
      });
  }, []);

  return (
    <DeviceInfoContext.Provider value={deviceInfo}>
      {children}
    </DeviceInfoContext.Provider>
  );
};
