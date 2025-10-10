import { invoke, isTauri } from '@tauri-apps/api/core';

const tryInvoke = async <T>(
  command: string,
  args?: Record<string, unknown>,
): Promise<T> => {
  if (!isTauri()) {
    throw new Error('Invoke API not available');
  }

  return await invoke(command, args);
};

export const useInvokeApi = () => {
  return {
    getAppSettings: async (): Promise<Record<string, unknown>> => {
      return await tryInvoke('get_app_settings_from_registry');
    },
    updateSetting: async (
      key: string,
      value: string | number | boolean,
    ): Promise<void> => {
      return await tryInvoke('update_app_setting_in_registry', {
        name: key,
        value,
      });
    },
    openInExternalWindow: async (url: string): Promise<void> => {
      return await tryInvoke('open_youtube_in_window', { raw: url });
    },
    enterFullscreen: async (): Promise<void> => {
      return await tryInvoke('enter_fullscreen');
    },
    exitFullscreen: async (): Promise<void> => {
      return await tryInvoke('exit_fullscreen');
    },
  };
};
