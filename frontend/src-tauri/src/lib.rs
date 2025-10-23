mod app_settings;
mod youtube_window_manager;
mod window_controls;
mod mobile_plugin_window_manager;
mod query_device_info;
use tauri::{Manager, WindowEvent};
use tauri::{
    menu::{Menu, MenuItem},
    tray::{MouseButton, TrayIconBuilder, TrayIconEvent},
};


#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
    .setup(|app| {
            let quit_i = MenuItem::with_id(app, "quit", "Quit", true, None::<&str>)?;
            let menu = Menu::with_items(app, &[&quit_i])?;

            let tray = TrayIconBuilder::new()
            .on_tray_icon_event(|tray, event| match event {
                TrayIconEvent::Click {
                button: MouseButton::Left,
                ..
                } => {
                let app = tray.app_handle();
                if let Some(window) = app.get_webview_window("main") {
                    let _ = window.unminimize();
                    let _ = window.show();
                    let _ = window.set_focus();
                }
                }
                _ => { }
            })
            .tooltip("YT Media Controller")
            .menu(&menu)
            .on_menu_event(|app, event| match event.id.as_ref() {
                "quit" => app.exit(0),
                _ => {}
            })
            .icon(app.default_window_icon().unwrap().clone())
            .build(app)?;

            if let Some(win) = app.get_webview_window("main") {
                let _ = win.maximize();
            }
            Ok(())
        })
        .on_window_event(|window, event| {
            if let WindowEvent::CloseRequested { api, .. } = event {
                let _ = window.hide();
                api.prevent_close();
            }
        })
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            app_settings::get_app_settings_from_registry,
            app_settings::update_app_setting_in_registry,
            youtube_window_manager::open_youtube_in_window,
            mobile_plugin_window_manager::open_mobile_config_window,
            query_device_info::get_local_ip,
            window_controls::enter_fullscreen,
            window_controls::exit_fullscreen,
            window_controls::focus_window,
            ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
