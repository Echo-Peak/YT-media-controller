use regex::Regex;
use serde_json::json;
use tauri::{AppHandle, WebviewUrl, WebviewWindowBuilder};
use url::Url;
use uuid::Uuid;

#[tauri::command]
pub fn open_youtube_in_window(app: AppHandle, raw: String) -> Result<serde_json::Value, String> {
    let url = Url::parse(&raw).map_err(|e| e.to_string())?;
    let host = url.host_str().ok_or("no host")?.to_ascii_lowercase();
    let re = Regex::new(r"(^|\.)((youtube\.com)|(youtube-nocookie\.com)|(youtu\.be))$").unwrap();
    if !re.is_match(&host) {
        return Err("not a YouTube URL".into());
    }

    // Give each window a unique label if you might open multiple
    let label = format!("yt-{}",Uuid::new_v4());
    WebviewWindowBuilder::new(&app, &label, WebviewUrl::External(url))
        .title("YouTube")
        .build()
        .map_err(|e| e.to_string())?;

    Ok(json!({ "ok": true, "id": label }))
}