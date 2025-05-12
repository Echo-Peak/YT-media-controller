


chrome.runtime.sendMessage({ action: "getBackendSettings" }, (response) => {
    console.log("Received response from background script:", response);
  if (response) {
    console.log("Backend settings:", response);
    document.getElementById("currentBackendServerPort").textContent = response.backendServerPort;
    document.getElementById("currentControlServerPort").textContent = response.controlServerPort;
  }
});


function currentBackendServerPort(){
  chrome.runtime.sendMessage({action: "updateBackendServerPort"}, (response) => {
    if (response && response.success) {
      console.log("Backend server port updated:", response);
      document.getElementById("currentBackendServerPort").textContent = response.port;
    }
  });
}

function currentControlServerPort(){
  chrome.runtime.sendMessage({action: "updateControlServerPort"}, (response) => {
    if (response && response.success) {
      console.log("Control server port updated:", response);
      document.getElementById("currentControlServerPort").textContent = response.port;
    }
  });
}

document.getElementById("updateBackendServerButton").addEventListener("click", currentBackendServerPort);
document.getElementById("updateControlServerButton").addEventListener("click", currentControlServerPort);