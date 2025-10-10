import { DeviceInfoDto } from '../../types/DeviceInfoDto';
import { useInvokeApi } from '../useInvokeApi';
import { BackendService } from './BackendService';

let backendService: BackendService | null = null;

export const initBackendService = async () => {
  if (!backendService) {
    const { getAppSettings } = useInvokeApi();
    const appSettings = (await getAppSettings()) as DeviceInfoDto;
    const srv = new BackendService(appSettings.uiSocketServerPort);
    srv.init();
    backendService = srv;
  }
};

export const useBackendService = (): BackendService => {
  if (!backendService) {
    throw new Error('BackendService not initialized');
  }
  return backendService;
};
