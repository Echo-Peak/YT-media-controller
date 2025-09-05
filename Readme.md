# YT-media-controller

[![Current build status](https://github.com/Echo-Peak/YT-media-controller/actions/workflows/main-wf.yml/badge.svg)](https://github.com/Echo-Peak/YT-media-controller/actions/workflows/main-wf.yml)

## Inspiration

The driving force behind this project is that I watch a lot of YouTube content,
but I don’t support the way YouTube is monetized or the way data is collected
about what you watch. Another motivation is to bypass the ad-blocking mechanisms
YouTube implements at this time.  
I needed a quick and seamless way to send a YT video from mobile to my dedicated
HTPC. Using Android’s casting feature—especially via the YT app—is not an option
because casting affects the Android device.

Since I primarily use Android devices, the easiest way to get a YT video to play
on my HTPC is by leveraging Android’s share menu to send localhost API requests
to the HTPC. From a usability perspective, it’s as easy as "long-pressing" on a
video—whether it’s in a browser or the YT app—and selecting "Share," then
pressing the "Play video" button. Three steps.

## Overview

This project consists of 3 components:

**The mobile plugin**

Currently only supports Android.  
This lightweight app enhances the Android share menu by adding two options:

- Send URL to YT Controller
- Queue URL to YT Controller

When either action is triggered (usually via a long press), the app:

- Validates that the shared URL is from YouTube.
- Sends the URL to the Windows service using the endpoint
  `POST http://DEVICE_IP:PORT/mobile/playVideo`.

**The C# backend server**

This Windows service runs under the "Local Network" service account to minimize
permissions. It hosts two servers:

- An HTTP server for communication with external devices (e.g., the Android
  app).
- A WebSocket control server for internal communication with the browser
  extension.

The Android app interacts with the HTTP server by sending a `playVideo` request
that includes the original YouTube URL from the `SHARE_INTENT` action.

**The external viewer (UI / browser extension)**

The browser extension is a 3-part component that allows communication with an
external C# HTTP server and WebSocket server so that the mobile app can send a
YT link and have it play within the UI.

The parts are as follows:

- **The external viewer**
  - This is the UI that can play HLS, DASH, and YouTube iframe videos.
  - This viewer communicates with the C# backend server via a local WebSocket
    connection.

- **The mobile setup**
  - This is a UI that renders a QR code containing the device's local network IP
    and the port of the C# HTTP server.

- **The native host exec**
  - This is used to retrieve the local device IP and port of the C# HTTP server
    and store it within the extension context.
  - This only runs once when the extension loads.

These three components are designed to enable seamless sending of YouTube video
URLs from an Android device—via a "long-press" on video content. The idea is to
send a YouTube video to a designated PC, like an HTPC, without the analytics
gathering that occurs during casting or affecting the YouTube recommendation
feed of your personal account.

## Local Setup

- Clone the repo
- Navigate to the root folder
- Run `yarn`
- Adjust `backend/settings.example.json` ports if needed
- Run `yarn build-service` to build the C# backend service and native host exec
- Run `yarn build-browser` to build the extension
- Open a Chromium browser and go to Settings > Manage Extensions
  - Ensure Developer Mode is enabled
  - Click on "Load unpacked"
  - Navigate to the cloned repo folder and select the
    `dist/browser-extension-unpacked` folder
  - Copy the extension ID, then in your terminal set the environment variable:
    `EXTENSION_ID=<your extension id>` (replace with the ID you copied)
- Back in your terminal, re-run `yarn build-browser`
- In the browser, go back to the extension page, reload the extension, and then
  restart the browser  
  **Native host exec IPC is not functional until the next browser restart!**
- Either launch Visual Studio as admin or run `YTMediaControllerSrv.exe` as
  admin
- In the browser, click the Extensions/Puzzle icon in the top bar, then pin the
  YTMediaController extension
- Finally, click the YTMediaController extension to open the viewer. Then
  right-click the extension icon to open the mobile config UI
- Use your Android phone with the YTMediaController app running, then scan the
  QR code to link your phone to the C# backend server
- That’s it! You can now send any YouTube video from mobile to PC anonymously
