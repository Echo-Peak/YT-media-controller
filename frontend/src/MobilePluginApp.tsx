import React from 'react';
import { useEffect, useState } from 'react';
import { useDeviceInfo } from './providers/DeviceInfoProvider';
import QRCode from 'react-qr-code';
import {
  Box,
  Button,
  Center,
  Code,
  Container,
  Field,
  Heading,
  NumberInput,
  Text,
} from '@chakra-ui/react';
import { useInvokeApi } from './services/useInvokeApi';

export const MobilePluginApp = () => {
  const { deviceIp, devicePort, uiSocketServerPort, connectionError } =
    useDeviceInfo();

  const invokeApi = useInvokeApi();

  const [deviceEndpoint, setDeviceEndpoint] = useState<string>('');
  const [wsPort, setWsPort] = useState<number | null>(
    uiSocketServerPort || null,
  );
  const [wsPortError, setWsPortError] = useState<string>('');

  useEffect(() => {
    if (deviceIp && devicePort) {
      setDeviceEndpoint(`http://${deviceIp}:${devicePort}/mobile`);
    }
  }, [deviceIp, devicePort]);

  const updateWSPort = () => {
    invokeApi
      .updateSetting('uiSocketServerPort', wsPort || 0)
      .then(() => {
        console.log('Updated setting');
        window.location.reload();
      })
      .catch(console.error);
  };

  const onWsPortChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(e.target.value, 10);
    if (isNaN(value) || value < 1000 || value > 65535) {
      setWsPortError(
        'Please enter a valid port number between 1000 and 65535.',
      );
      return;
    }
    setWsPortError('');
    setWsPort(value);
  };

  if (connectionError) {
    return (
      <Container maxWidth="sm">
        <Box style={{ textAlign: 'center', marginTop: '20px' }}>
          <Heading as="h1" color="red.500">
            Error
          </Heading>
          <Heading color="white" as="h3" size="md" mt={4}>
            {connectionError}
          </Heading>
        </Box>

        <Box style={{ textAlign: 'center', marginTop: '20px' }}>
          <Text color="white" mb={4}>
            Check to see if the WS port is correct by checking:
            <Code>
              HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\YTMediaController
            </Code>
            And then checking the value of "UiSocketServerPort".
          </Text>
          <Text color="white" mb={4}>
            The current WS port being used is: <Code>{uiSocketServerPort}</Code>
            If the <Code>current port</Code> is different than the{' '}
            <Code>UiSocketServerPort</Code> setting in registry. Use the text
            box below to change it.
          </Text>
        </Box>

        <Box mt={4} style={{ textAlign: 'center', color: 'white' }}>
          <Field.Root invalid={!!wsPortError}>
            <Field.Label>Enter Number</Field.Label>

            <NumberInput.Root
              min={1000}
              max={65535}
              width="100%"
              onChange={onWsPortChange}
            >
              <NumberInput.Control />
              <NumberInput.Input />
            </NumberInput.Root>
            <Field.ErrorText>
              The port number needs be between 1000 and 65535.
            </Field.ErrorText>
          </Field.Root>

          <Button
            disabled={!!wsPortError || wsPort === null}
            style={{ background: 'teal' }}
            onClick={updateWSPort}
            mt={4}
          >
            Set WS port
          </Button>
        </Box>
      </Container>
    );
  }

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
