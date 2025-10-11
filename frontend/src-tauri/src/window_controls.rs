use serde_json::{json};
use tauri::{AppHandle, Manager};

#[tauri::command]
pub fn enter_fullscreen(app: AppHandle) -> Result<serde_json::Value, String> {
    if let Some(win) = app.get_webview_window("main") {
        win.set_fullscreen(true).map_err(|e| e.to_string())?;
        Ok(json!({ "ok": true, "fullscreen": true }))
    } else {
        Err("main window not found".into())
    }
}


#[tauri::command]
pub fn exit_fullscreen(app: AppHandle) -> Result<serde_json::Value, String> {
    if let Some(win) = app.get_webview_window("main") {
        win.set_fullscreen(false).map_err(|e| e.to_string())?;
        Ok(json!({ "ok": true, "fullscreen": false }))
    } else {
        Err("main window not found".into())
    }
}