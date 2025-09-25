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

## Usage for end-users

**Download the installer**

Go to [releases section](https://github.com/Echo-Peak/YT-media-controller/tags)
and choose which channel/environment you would like to install.

There are 3 "channels" to choose from. Dev channel being the most unstable.
Release channel being the most stable.

- Dev
- Staging
- Release

Install the EXE installer.

**Install the APK**

Either install the .apk file via sideloading. To do this, you will need to go to
Settings > Security and select "Allow untrusted sources" then re-download the
.apk to install. After install, **make sure** you disable "Allow untrusted
sources" option!

If you have android studio installed on computer or at the very least a android
SDK environment, you can use **adb** to install the app.

- Download the .apk file
- open Command Prompt and check if abd is installed by typing `abd` and press
  enter
- On your android device, you will need to have it in "developer mode". To do
  this, it will vary by device manufacture, its usually done by opening
  Settings > About phone > Software information and tapping Build number 7
  times.
- Once device has been setup for developer mode, enable USB debugging via
  developer options. To do this, open Settings and scroll all the way to the
  bottom, you should see something like "Developer options". Open it and scroll
  slowly to find "USB debugging" and enable it.
- Plug in your android device to your computer. You should see a "Allow this
  device" prompt on the android device after a few seconds. Make sure you press
  allow.
- Back in command prompt, type `adb list` and press enter. You should see your
  device listed.
- Install the apk on the device by running `abd install <apk path>`. Copy/Paste
  the path of the .apk file downloaded and replace <apk path\> with it. e.g:
  `abd install C:\downloads\app-release-unsigned.apk`

**Installing the browser extension**

- After the app is installed. There is one more step that is needed.
- In your browser of choice, must be chromium based, go to Settings >
  Extensions > Manage extensions and enable Developer mode
- Click on "Load unpacked" and navigate to
  `C:\Program Files (x86)\YTMediaController\BrowserExtension` and click Select
  folder.

**Linking mobile to host**

- On the computer/host, Right-click on extension icon and select Configure
  Mobile plugin option
- On your android phone, open the YTMediaController app and tap on the camera
  view-box to load the QR code scanner.
- Point camera at the QR code thats displayed on the computer to link your
  phone.

**Testing**

- When all previous steps have been done, you should now be able to play/send
  any youtube link on your android device to the computer to be played without
  any ads / interruptions / tracking.
- Open a browser and find a youtube video you want to try. Hold tap on the video
  to open the context menu and select share.
- You should see YTMediaContrller as an option with the "Play video" as a
  action. Select on "Play video".
- Watch as the video will be playing on the computer within a couple seconds.

**Configurability**

There are a number of config options to change the behavior of the app. These
settings are located in the registry at this path:
`HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\YTMediaController`.

These are the possible settings:

- **backendServerPort** - Change the background HTTP server port. Changing this
  will require service restart.
- **uiSocketServerPort** - Change the IPC port between the UI and background
  service. Changing this will require service restart.
- **disableAutoUpdate** - If set to "true", it will disable the auto updater.
- **autoUpdateIntervalMins** - Changes how often the updater checks for an
  update. This is in minutes. Default is 4 hours (240 mins)
- **autoUpdateChannel** - Changes what update channel/environment to choose
  from. Use this if you want to go from "dev" to "release" if you want a more
  stable build. Its possible values are: "dev", "staging", "release"

Restarting the background service
`net stop YTMediaControllerService && net start YTMediaControllerService`

## Local Setup

- Clone the repo
- Navigate to the root folder
- Run `yarn`
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
