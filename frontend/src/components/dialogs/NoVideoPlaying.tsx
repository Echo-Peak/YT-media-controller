import { Box, Button, Container, Heading } from '@chakra-ui/react';
import { useInvokeApi } from '../../services/useInvokeApi';

export const NoVideoPlaying = () => {
  const { openMobileConfigWindow } = useInvokeApi();

  const openWindow = () => {
    openMobileConfigWindow()
      .then(() => {
        console.log('Mobile config window opened');
      })
      .catch((err) => {
        console.error('Failed to open mobile config window:', err);
      });
  };
  return (
    <Container>
      <Box>
        <Button
          position="absolute"
          right={0}
          top={0}
          color="red"
          onClick={openWindow}
        >
          Open mobile config window
        </Button>
      </Box>
      <Box m={10}>
        <Heading textAlign="center" as="h1" color="gray.300" mb={4}>
          No video is currently playing.
        </Heading>

        <Heading textAlign="center" as="h3" size="sm" color="gray.300" mt={40}>
          To play a video, use your phone to send a youtube link via "Sharing"
          menu.
        </Heading>
        <Heading textAlign="center" as="h5" mt={8} size="sm" color="gray.300">
          In a browser navigate to youtube and long-press on any video link,
          then select "Share" and choose "Send to TV" option. If using the
          YouTube mobile app, find the video you want to play and select the
          vertical ellipsis (three dots) next to the video title, then select
          "Share" and choose "Send to TV" option.
        </Heading>
      </Box>
    </Container>
  );
};
