import React, {
  createContext,
  useContext,
  useState,
  ReactNode,
  useEffect,
} from 'react';

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

export const DeviceInfoProvider: React.FC<DeviceInfoProviderProps> = ({
  children,
}) => {
  const [deviceInfo, setDeviceInfo] = useState<DeviceInfo>({
    deviceIp: undefined,
    devicePort: undefined,
    uiSocketServerPort: undefined,
    connectionError: undefined,
  });

  const location = window.location;
  const params = new URLSearchParams(location.search);
  const deviceIpParam = params.get('deviceIp');
  const devicePortParam = params.get('devicePort');
  const uiSocketServerPortParam = params.get('uiSocketServerPort');

  useEffect(() => {
    if (deviceIpParam) {
      setDeviceInfo((prev) => ({ ...prev, deviceIp: deviceIpParam }));
    }
    if (devicePortParam && uiSocketServerPortParam) {
      setDeviceInfo((prev) => ({
        ...prev,
        devicePort: parseInt(devicePortParam, 10),
        uiSocketServerPort: parseInt(uiSocketServerPortParam, 10),
      }));
    }
    if (!deviceIpParam || !devicePortParam) {
      setDeviceInfo({
        connectionError:
          'Unable to retrieve device information. Please ensure the backend service is running and try again.',
      });
    }
  }, []);

  return (
    <DeviceInfoContext.Provider value={deviceInfo}>
      {children}
    </DeviceInfoContext.Provider>
  );
};
