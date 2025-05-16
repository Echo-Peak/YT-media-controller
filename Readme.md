# YT-media-controller

## Usage 
**Desktop app setup**
This project creates 2 build artifacts. The desktop app, which contains the background server, control server and browser extension. The mobile app, which in this case is a android APK file.

The apk file will need to be side-loaded via ADB.

After the desktop app is installed, the browser extension will need to be added your chromium based browser of choice. You will need to go into "Manage extension", enable develop mode and "Load Unpacked" and then select the root folder of the YTMediaController browser extension. This will be located in C:\Program Files\YTMediaController\ui

**Mobile app setup**
On the desktop computer, in the browser, open the extension via icon in extension bar. This will create a QR code of the device local network IP.
In the mobile app, open it and allow camera permissions then tap of the camera/code scanner UI to initiate the scanner. Scan the QR code to set/save the device IP.

In an app that shows youtube videos, or youtube links in general perhaps via youtube website in an browser, long press on a video or open the share menu and select YTMediaController.
Depending what you want to do, select "Play video" if you want to play the selected video immediately or "Queue video" if you want to play the selected video after the current video haas ended.

## Project Overview
This project consists of three components designed to enable seamless sending of YouTube video URLs from an Android device—via the native YouTube app or browser—without triggering casting issues, analytics events, or altering the YouTube recommendation feed.

## Inspiration
The driving force behind this project is a strong desire to eliminate ads—completely. I dont want youtube premium, I like casting, and I can't stand ads at all, and I want to avoid them at all costs. At the same time, I need a quick and seamless way to play YouTube videos on my HTPC. Since I primarily use Android devices, there’s no easier method than leveraging the native Android share menu to send YouTube links directly from any app or browser. This project makes that process effortless while bypassing ads and avoiding the overhead and limitations of YouTube's official casting features.


## 1. Browser Extension
The Chrome extension enables communication with a background Windows service that runs an HTTP server. It is active only on tabs that are already open to YouTube.

**Frontend / Content Script**
* Monitors the YouTube player to detect when a video starts or ends and sends corresponding events to the background script.
* Allows URL changes directly within the current YouTube tab.
* Displays a QR code containing the local device's IP address and the port used by the background server.
* Provides a simple UI to update the background server or control server port.
* Utilizes Chrome’s native messaging to communicate with a host executable (YTMediaControllerHost), which requires elevated privileges. As a result, a UAC prompt will appear when restarting the C# Windows service.

**Background Script**
* Listens for "playbackStarted" and "playbackEnded" events from the content script.
* Forwards these events to the Windows service via a WebSocket connection.

---

## 2. C# Windows Background Service
This Windows service runs under the "Local Network" service account to minimize permissions. It hosts two servers:
* An HTTP server for communication with external devices (e.g., the Android app).
* A WebSocket control server for internal communication with the browser extension.

The Android app interacts with the HTTP server by sending a playVideo request that includes the original YouTube URL from the SHARE_INTENT action.

---
### 3. Android App
This lightweight app enhances the Android share menu by adding two options:
* Send URL to YT Controller
* Queue URL to YT Controller

When either action is triggered (usually via a long press), the app:
* Validates that the shared URL is from YouTube.
* Sends the URL to the Windows service using the endpoint http://DEVICE_IP:PORT/playVideo.