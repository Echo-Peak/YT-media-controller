use serde_json::json;
use tauri::{AppHandle, WebviewUrl, WebviewWindowBuilder};
use tauri::Manager;

const MOBILE_LABEL: &str = "mobile";

#[tauri::command]
pub async fn open_mobile_config_window(app: AppHandle) -> Result<serde_json::Value, String> {
    if let Some(win) = app.get_webview_window(MOBILE_LABEL) {
        let _ = win.show();
        let _ = win.set_focus();
        return Ok(json!({ "status": "focused", "windowLabel": MOBILE_LABEL }));
    }

    let url = WebviewUrl::App("index.html/mobile".into());
    let app_clone = app.clone();
    let label_clone = MOBILE_LABEL.to_string();
    tauri::async_runtime::spawn_blocking(move || {
        WebviewWindowBuilder::new(&app_clone, &label_clone, url)
            .title("Mobile plugin configuration")
            .build()
            .map_err(|e| e.to_string())
    })
    .await
    .map_err(|e| e.to_string())??;

    Ok(json!({ "status": "success", "windowLabel": MOBILE_LABEL }))
}