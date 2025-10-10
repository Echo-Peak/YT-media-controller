use serde_json::{json};
use tauri::{AppHandle, Manager};

#[tauri::command]
pub fn toggle_fullscreen(app: AppHandle) -> Result<serde_json::Value, String> {
    if let Some(win) = app.get_webview_window("main") {
        let is_fullscreen = win.is_fullscreen().map_err(|e| e.to_string())?;
        win.set_fullscreen(!is_fullscreen).map_err(|e| e.to_string())?;
        Ok(json!({ "ok": true, "fullscreen": !is_fullscreen }))
    } else {
        Err("main window not found".into())
    }
}