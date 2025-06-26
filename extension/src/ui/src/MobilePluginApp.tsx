import { useEffect, useRef, useState } from "react";
import { useDeviceInfo } from "./providers/DeviceInfoProvider";
import QRCode from "react-qr-code";

export const MobilePluginApp = () => {
  const {deviceIp, devicePort} = useDeviceInfo();
  const [deviceEndpoint, setDeviceEndpoint] = useState<string>("");

  useEffect(() => {

    if(deviceIp && devicePort) {
      setDeviceEndpoint(`http://${deviceIp}:${devicePort}`);
    }

  }, [deviceIp, devicePort]);

  return (<div>
    <QRCode
      size={256}
      style={{ maxWidth: "256px", width: "256px" }}
      value={deviceEndpoint}
      viewBox={`0 0 256 256`}
    />

    <div>
      <h4 style={{ color:"white" }}>BackendServerIp: <span id="currentBackendServerIp">{deviceIp || "Unknown"}</span></h4>
      <h4 style={{ color:'white' }}>BackendServerPort: <span id="currentBackendServerPort">{devicePort || "Unknown"}</span></h4>
    </div>
  </div>
  )
}