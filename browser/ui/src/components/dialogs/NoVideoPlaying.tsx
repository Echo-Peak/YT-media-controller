import React from 'react';
import { Box, Center, Heading, Image } from '@chakra-ui/react';

export const NoVideoPlaying = () => {
  return (
    <Box m={10}>
      <Heading textAlign="center" as="h1" color="gray.300" mb={4}>
        No video is currently playing.
      </Heading>

      <Box mt={2}>
        <Center>
          <Image
            src="./assets/no-video-playing.png"
            alt="No video playing"
            height="300px"
          />
        </Center>
      </Box>
      <Heading textAlign="center" as="h3" size="sm" color="gray.300" mt={10}>
        To play a video, use your phone to send a youtube link via "Sharing"
        menu.
      </Heading>
      <Heading textAlign="center" as="h5" mt={4} size="sm" color="gray.300">
        In a browser navigate to youtube and long-press on any video link, then
        select "Share" and choose "Send to TV" option. If using the YouTube
        mobile app, find the video you want to play and select the vertical
        ellipsis (three dots) next to the video title, then select "Share" and
        choose "Send to TV" option.
      </Heading>
    </Box>
  );
};
