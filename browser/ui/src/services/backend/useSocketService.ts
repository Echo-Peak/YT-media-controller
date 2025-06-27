import { BackendService } from './BackendService';

let backendService: BackendService | null = null;

export const initBackendService = async () => {
  if (!backendService) {
    backendService = await BackendService.init();
  }
};

export const useBackendService = (): BackendService => {
  if (!backendService) {
    throw new Error('BackendService not initialized');
  }
  return backendService;
};
