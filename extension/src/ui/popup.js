


chrome.runtime.sendMessage({ action: "getBackendSettings" }, (response) => {
    console.log("Received response from background script:", response);
  if (response) {
    console.log("Backend settings:", response);
    document.getElementById("currentBackendServerPort").textContent = response.backendServerPort;
    document.getElementById("currentControlServerPort").textContent = response.controlServerPort;
  }
});

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