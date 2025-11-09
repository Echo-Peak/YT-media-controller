import {
  Box,
  Button,
  Container,
  Heading,
  Text,
  VStack,
  HStack,
  Icon,
} from '@chakra-ui/react';
import { useEffect } from 'react';
import { FaMobileAlt, FaShare, FaYoutube } from 'react-icons/fa';
import { useInvokeApi } from '../../services/useInvokeApi';
import { useFullScreen } from '../../providers/FullScreenProvider';
import styled from '@emotion/styled';

const MainContainer = styled(Container)({
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  justifyContent: 'center',
  minHeight: '100vh',
  position: 'relative',
  padding: '2rem',
});

const IconCircle = styled(Box)({
  width: '120px',
  height: '120px',
  borderRadius: '50%',
  backgroundColor: 'rgba(255, 0, 0, 0.1)',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  marginBottom: '2rem',
  border: '2px solid rgba(255, 0, 0, 0.3)',
});

const StepCard = styled(Box)({
  backgroundColor: 'rgba(255, 255, 255, 0.05)',
  borderRadius: '12px',
  padding: '1.5rem',
  border: '1px solid rgba(255, 255, 255, 0.1)',
  width: '100%',
  maxWidth: '500px',
  transition: 'all 0.3s ease',
  '&:hover': {
    backgroundColor: 'rgba(255, 255, 255, 0.08)',
    borderColor: 'rgba(255, 0, 0, 0.3)',
  },
});

export const NoVideoPlaying = () => {
  const { openMobileConfigWindow } = useInvokeApi();
  const { toggleFullscreen } = useFullScreen();

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      const activeTag = document.activeElement?.tagName.toLowerCase();
      if (
        activeTag === 'input' ||
        activeTag === 'textarea' ||
        activeTag === 'button'
      )
        return;

      switch (event.code) {
        case 'F11':
        case 'Escape':
        case 'KeyF':
          event.preventDefault();
          void toggleFullscreen();
          break;
        default:
          break;
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [toggleFullscreen]);

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
    <MainContainer overflow="hidden" maxW="container.xl">
      {/* Settings Button */}
      <Button
        position="absolute"
        top="1rem"
        right="1rem"
        colorScheme="red"
        size="sm"
        onClick={openWindow}
        backgroundColor="red.500"
        color="white"
        _hover={{
          backgroundColor: 'red.600',
        }}
      >
        Mobile Config
      </Button>

      <VStack gap={8} align="center" textAlign="center">
        {/* Main Icon */}
        <IconCircle>
          <Icon as={FaYoutube} boxSize={16} color="red.400" />
        </IconCircle>

        {/* Main Heading */}
        <VStack gap={4}>
          <Heading
            as="h1"
            size="2xl"
            color="gray.100"
            fontWeight="bold"
            letterSpacing="wide"
          >
            No Video Playing
          </Heading>
          <Text color="gray.400" fontSize="lg" maxW="600px">
            Send a YouTube video from your mobile device to get started
          </Text>
        </VStack>

        {/* Instructions */}
        <VStack gap={4} width="100%" maxW="600px">
          <StepCard>
            <HStack gap={4} align="start">
              <Icon as={FaMobileAlt} boxSize={6} color="red.400" mt={1} />
              <VStack align="start" gap={2} flex={1}>
                <Heading as="h3" size="md" color="gray.200">
                  Using Mobile Browser
                </Heading>
                <Text color="gray.400" fontSize="sm" lineHeight="tall">
                  Navigate to YouTube, long-press on any video link, select
                  "Share", then choose "Send to TV"
                </Text>
              </VStack>
            </HStack>
          </StepCard>

          <StepCard>
            <HStack gap={4} align="start">
              <Icon as={FaShare} boxSize={6} color="red.400" mt={1} />
              <VStack align="start" gap={2} flex={1}>
                <Heading as="h3" size="md" color="gray.200">
                  Using YouTube App
                </Heading>
                <Text color="gray.400" fontSize="sm" lineHeight="tall">
                  Find the video you want to play, tap the three dots (⋯) next
                  to the video title, select "Share", then choose "Send to TV"
                </Text>
              </VStack>
            </HStack>
          </StepCard>
        </VStack>

        {/* Keyboard Shortcuts Hint */}
        <Box mt={8} p={4} borderRadius="md" bg="rgba(255, 255, 255, 0.03)">
          <Text color="gray.500" fontSize="xs" textTransform="uppercase">
            Keyboard Shortcuts: Press <strong>F</strong>, <strong>F11</strong>,
            or <strong>ESC</strong> to toggle fullscreen
          </Text>
        </Box>
      </VStack>
    </MainContainer>
  );
};
