mod app_settings;
mod youtube_window_manager;
mod window_controls;
use tauri::{Manager};

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .setup(|app| {
        if let Some(win) = app.get_webview_window("main") {
            let _ = win.maximize();
        }
        Ok(())
        })
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            app_settings::get_app_settings_from_registry,
            app_settings::update_app_setting_in_registry,
            youtube_window_manager::open_youtube_in_window,
            window_controls::enter_fullscreen,
            window_controls::exit_fullscreen,
            ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
