function generateQRCode(deviceIP, backendServerPort){
  const qrCodeElm = document.getElementById("qr-code-container");

  // imported via CDN
  const qr = qrcode(4, "L");
  qr.addData(`http://${deviceIP}:${backendServerPort}`);
  qr.make();
  qrCodeElm.innerHTML = qr.createImgTag();
}

function validatePort(port) {
  const portNumber = parseInt(port, 10);
  if (isNaN(portNumber) || portNumber < 1 || portNumber > 65535) {
    return false;
  }
  return true;
}

function updateBackendServerPort(){
  const inputData = document.getElementById("backendServerPortInput").value;

  if(!validatePort(inputData)){
    return;
  }

  chrome.runtime.sendMessage({action: "updateBackendServerPort", port: parseInt(inputData)}, (response) => {
    console.log("Received response from background script:", response);
    if (response && response.success) {
      console.log("Backend server port updated:", response, inputData);
      document.getElementById("currentBackendServerPort").textContent = parseInt(inputData);
    }
  });
}

function updateControlServerPort(){
  const inputData = document.getElementById("backendServerPortInput").value;
  if(!validatePort(inputData)){
    return;
  }
  chrome.runtime.sendMessage({action: "currentControlServerPort", port: parseInt(inputData)}, (response) => {
    if (response && response.success) {
      console.log("Control server port updated:", response);
      document.getElementById("currentControlServerPort").textContent = response.port;
    }
  });
}

document.getElementById("updateBackendServerButton").addEventListener("click", updateBackendServerPort);
document.getElementById("updateControlServerButton").addEventListener("click", updateControlServerPort);

function handleGetBackendServerPort(response) {
  console.log("handleGetBackendServerPort() Received response from background script:", response);
  if (response && response.backendServerPort) {
    document.getElementById("currentBackendServerPort").textContent = response.backendServerPort;
  }
}

function handleGetDeviceNetworkIp(response) {
  console.log("handleGetDeviceNetworkIp() Received response from background script:", response);
  if (response && response.deviceNetworkIp) {
    generateQRCode(response.deviceNetworkIp, response.backendServerPort);
  }
}

function handleGetControlServerPort(response) {
  console.log("handleGetControlServerPort() Received response from background script:", response);
  if (response && response.controlServerPort) {
    document.getElementById("currentControlServerPort").textContent = response.controlServerPort;
  }
}

window.addEventListener("DOMContentLoaded", () => {
  chrome.runtime.sendMessage({ action: "getBackendServerPort" }, handleGetBackendServerPort);
  chrome.runtime.sendMessage({ action: "getDeviceNetworkIp" }, handleGetDeviceNetworkIp);
  chrome.runtime.sendMessage({ action: "getControlServerPort" }, handleGetControlServerPort);
});