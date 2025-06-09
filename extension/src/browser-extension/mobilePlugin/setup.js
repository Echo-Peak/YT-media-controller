const uiElements = {
  qrCodeContainer: document.getElementById("qr-code-container"),
  currentBackendServerIp: document.getElementById("currentBackendServerIp"),
  currentBackendServerPort: document.getElementById("currentBackendServerPort"),
}

function generateQRCode(deviceIP, backendServerPort){
  new window.QRCode(uiElements.qrCodeContainer, `http://${deviceIP}:${backendServerPort}`);
}



window.onload = () => {
  const urlParams = new URLSearchParams(window.location.search);
  const deviceIP = urlParams.get("deviceIp");
  const backendServerPort = urlParams.get("backendServerPort");

  if (deviceIP && backendServerPort) {
    generateQRCode(deviceIP, backendServerPort);
    uiElements.currentBackendServerIp.textContent = deviceIP;
    uiElements.currentBackendServerPort.textContent = backendServerPort;
  }
  else {
    console.error("Device IP or Backend Server Port not provided in URL parameters.");
    qrCodeContainer.innerHTML = "<p>Error: Missing device IP or backend server port.</p>";
  }
}