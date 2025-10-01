# Privacy Policy for YTMediaControllerSrv

**Effective date:** September 30, 2025  
**Developer:** echopeakdev

YTMediaControllerSrv (“the Service”, “the App”) is a Windows service that
enables local-network control and communication with the companion Chrome
extension **YTMediaControllerBrowser**. This policy explains what the Service
does, what data it handles, and how that data is treated.

> This document is provided for informational purposes and is not legal advice.

---

## What YTMediaControllerSrv Does

- Creates/opens a **Windows Firewall inbound rule** to allow **local-network
  (LAN) devices** to send requests to a local HTTP server hosted by the Service.
- Establishes a **localhost WebSocket** connection with the Chrome extension
  **YTMediaControllerBrowser** to enable inter-process communication (IPC)
  between the Windows service and the browser extension runtime.

The Service is designed for **local-only operation**. It does **not** include
any built-in cloud connectivity, telemetry, or remote control features.

---

## Data the Service Processes

- **Control messages over HTTP (LAN):** The Service accepts local HTTP requests
  from devices on your LAN. These messages typically include control commands
  (e.g., play, queue) and may include URLs or metadata you provide.
- **IPC messages over localhost WebSocket:** The Service and the Chrome
  extension exchange control/status messages via `ws://127.0.0.1` (loopback
  only).

### No Personal Data Collection by Default

- The Service **does not intentionally collect, store, or transmit personal
  data** to the developer.
- Any data present in control or IPC messages (e.g., URLs you submit) remains
  **on your device/LAN** and is used solely to fulfill your request.

---

## Logs and Local Storage

- The Service may create **local diagnostic logs** (e.g., errors, start/stop
  events, request summaries).
- Logs, if enabled, are stored **locally on your machine** and are **not**
  transmitted to the developer.
- You may delete logs at any time by removing the files or uninstalling the
  Service. Log locations (if any) are documented in the app settings or readme.

---

## Network Scope & Security

- The Windows Firewall rule is intended to allow **inbound connections from the
  local network only**. Actual scope (e.g., subnet restrictions) depends on your
  Windows Firewall profile and configuration.
- The WebSocket channel is restricted to **localhost (127.0.0.1)**.
- **Encryption:** LAN HTTP and localhost WS traffic are not encrypted by
  default. Use the Service only on trusted networks and hosts. If you require
  encryption, place the Service behind a secure reverse proxy or VPN under your
  control.

---

## Data Sharing

- The Service does **not** share data with third parties.
- The Service does **not** use analytics, ads, or trackers.

---

## Children’s Privacy

The Service is a utility application intended for general audiences and does not
target children. It does not knowingly collect personal information from
children.

---

## Your Choices and Controls

- **Firewall & Network Settings:** You can modify or remove the created firewall
  rule via Windows Security → Firewall & network protection.
- **Logs:** You can disable, rotate, or delete logs if the Service exposes those
  options.
- **Uninstall:** You can remove the Service at any time via “Apps & Features”
  (Windows) or the provided uninstaller, which will stop local processing.

---

## Data Retention

- The Service does not retain data beyond what is necessary for operation (e.g.,
  in-memory state) and any **optional local logs**.
- No data is sent to the developer for storage.

---

## Changes to This Policy

If this policy changes, we will update the **Effective date** above and include
the revised policy with the software distribution or the project’s
documentation.

---

## Contact

If you have questions about this Privacy Policy, contact us at:  
**Developer:** echopeakdev

**Site:** https://github.com/Echo-Peak/YT-media-controller
