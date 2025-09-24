import { BackendService } from './BackendService';

let backendService: BackendService | null = null;

export const initBackendService = async () => {
  if (!backendService) {
    const srv = new BackendService();
    await srv.init();
    backendService = srv;
  }
};

export const useBackendService = (): BackendService => {
  if (!backendService) {
    throw new Error('BackendService not initialized');
  }
  return backendService;
};
