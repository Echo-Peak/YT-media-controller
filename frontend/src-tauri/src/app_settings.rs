use serde_json::{json, Value};
use winreg::RegKey;
use winreg::enums::{HKEY_LOCAL_MACHINE, KEY_READ, KEY_WOW64_32KEY, KEY_WOW64_64KEY, RegType, KEY_SET_VALUE};
use base64::{Engine as _, engine::general_purpose};

const REG_PATH: &str = r"SOFTWARE\WOW6432Node\YTMediaController";
const KEYS: &[&str] = &[
    "backendServerPort", 
    "uiSocketServerPort", 
    "disableAutoUpdate", 
    "autoUpdateIntervalMins",
    "autoUpdateChannel"
    ];

fn utf16le_to_string(bytes: &[u8]) -> String {
    let mut u16s = Vec::with_capacity(bytes.len() / 2);
    let mut i = 0;
    while i + 1 < bytes.len() { u16s.push(u16::from_le_bytes([bytes[i], bytes[i + 1]])); i += 2; }
    String::from_utf16_lossy(&u16s).trim_end_matches('\u{0}').to_string()
}

#[tauri::command]
pub fn get_app_settings_from_registry() -> Result<Value, String> {
    let hive = RegKey::predef(HKEY_LOCAL_MACHINE);
    let key = hive.open_subkey_with_flags(REG_PATH, KEY_READ | KEY_WOW64_32KEY).or_else(|_| {
        hive.open_subkey_with_flags(REG_PATH, KEY_READ | KEY_WOW64_64KEY)
    }).map_err(|e| e.to_string())?;

    let mut out = serde_json::Map::new();
    for name in KEYS {
        match key.get_raw_value(name) {
            Ok(raw) => {
                let v = match raw.vtype {
                    RegType::REG_SZ | RegType::REG_EXPAND_SZ => json!(utf16le_to_string(&raw.bytes)),
                    RegType::REG_DWORD => {
                        let mut b = [0u8; 4]; b[..raw.bytes.len().min(4)].copy_from_slice(&raw.bytes[..raw.bytes.len().min(4)]);
                        json!(u32::from_le_bytes(b))
                    }
                    _ => json!(general_purpose::STANDARD.encode(&raw.bytes)),
                };
                out.insert((*name).to_string(), v);
            }
            Err(_) => { out.insert((*name).to_string(), json!(null)); }
        }
    }
    Ok(Value::Object(out))
}

#[tauri::command]
pub fn update_app_setting_in_registry(name: String, value: String) -> Result<Value, String> {
    let hklm = RegKey::predef(HKEY_LOCAL_MACHINE);

    let key = hklm
        .open_subkey_with_flags(REG_PATH, KEY_SET_VALUE | KEY_WOW64_64KEY)
        .or_else(|_| hklm.open_subkey_with_flags(REG_PATH, KEY_SET_VALUE | KEY_WOW64_32KEY))
        .map_err(|e| format!("open key failed: {e}"))?;

    key.set_value(&name, &value).map_err(|e| e.to_string())?;

    Ok(json!({ "ok": true }))
}