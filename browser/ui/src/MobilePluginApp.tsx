import React from 'react';
import { useEffect, useState } from 'react';
import { useDeviceInfo } from './providers/DeviceInfoProvider';
import QRCode from 'react-qr-code';
import { Box, Center, Container, Heading } from '@chakra-ui/react';

export const MobilePluginApp = () => {
  const { deviceIp, devicePort } = useDeviceInfo();
  const [deviceEndpoint, setDeviceEndpoint] = useState<string>('');

  useEffect(() => {
    if (deviceIp && devicePort) {
      setDeviceEndpoint(`http://${deviceIp}:${devicePort}`);
    }
  }, [deviceIp, devicePort]);

  return (
    <Container maxWidth="sm">
      <Box style={{ textAlign: 'center', marginBottom: '20px' }}>
        <Heading as="h1" color="white">
          Scan the QR Code to connect
        </Heading>
        <Heading color="white" as="h3" size="md">
          Make sure your device is connected to the same network
        </Heading>
      </Box>
      <Box mb={10}>
        <Center>
          <QRCode
            size={256}
            style={{ maxWidth: '256px', width: '256px' }}
            value={deviceEndpoint}
            viewBox={`0 0 256 256`}
          />
        </Center>
      </Box>

      <Box textAlign={'center'}>
        <Heading as="h5" size="sm" color="white">
          Local IP: {deviceIp || 'Unknown'}
        </Heading>
        <Heading as="h5" size="sm" color="white">
          Local Port: {devicePort || 'Unknown'}
        </Heading>
      </Box>
    </Container>
  );
};
