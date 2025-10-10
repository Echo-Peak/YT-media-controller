use serde_json::json;

#[tauri::command]
pub fn get_local_ip() -> Result<serde_json::Value, String> {
    let ip = local_ip_address::local_ip().map_err(|e| e.to_string())?;
    Ok(json!({ "ip": ip.to_string() }))
}
