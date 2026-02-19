use serde_json::{json, Map, Value};

#[cfg(windows)]
use winreg::enums::{
    RegType, HKEY_LOCAL_MACHINE, KEY_READ, KEY_SET_VALUE, KEY_WOW64_32KEY, KEY_WOW64_64KEY,
};
#[cfg(windows)]
use winreg::RegKey;

#[cfg(windows)]
use base64::{engine::general_purpose, Engine as _};

#[cfg(windows)]
const REG_PATH: &str = r"SOFTWARE\WOW6432Node\YTMediaController";

#[cfg(windows)]
const KEYS: &[&str] = &[
    "backendServerPort",
    "uiSocketServerPort",
    "disableAutoUpdate",
    "autoUpdateIntervalMins",
];

#[derive(serde::Serialize)]
struct AppSettings {
    backend_server_port: u32,
    ui_socket_server_port: u32,
    disable_auto_update: bool,
    auto_update_interval_mins: u32,
}

impl Default for AppSettings {
    fn default() -> Self {
        Self {
            backend_server_port: 60166,
            ui_socket_server_port: 52000,
            disable_auto_update: false,
            auto_update_interval_mins: 320,
        }
    }
}

impl AppSettings {
    fn get_default_value(&self, key: &str) -> Value {
        match key {
            "backendServerPort" => json!(self.backend_server_port),
            "uiSocketServerPort" => json!(self.ui_socket_server_port),
            "disableAutoUpdate" => json!(self.disable_auto_update),
            "autoUpdateIntervalMins" => json!(self.auto_update_interval_mins),
            _ => Value::Null,
        }
    }
}

#[cfg(windows)]
fn defaults_map() -> Map<String, Value> {
    let app = AppSettings::default();
    let mut m = Map::new();
    for k in KEYS {
        m.insert((*k).to_string(), app.get_default_value(k));
    }
    m
}

#[cfg(not(windows))]
fn defaults_map() -> Map<String, Value> {
    let app = AppSettings::default();
    let mut m = Map::new();
    m.insert(
        "backendServerPort".to_string(),
        json!(app.backend_server_port),
    );
    m.insert(
        "uiSocketServerPort".to_string(),
        json!(app.ui_socket_server_port),
    );
    m.insert(
        "disableAutoUpdate".to_string(),
        json!(app.disable_auto_update),
    );
    m.insert(
        "autoUpdateIntervalMins".to_string(),
        json!(app.auto_update_interval_mins),
    );
    m
}

#[cfg(windows)]
fn utf16le_to_string(bytes: &[u8]) -> String {
    let mut u16s = Vec::with_capacity(bytes.len() / 2);
    let mut i = 0;
    while i + 1 < bytes.len() {
        u16s.push(u16::from_le_bytes([bytes[i], bytes[i + 1]]));
        i += 2;
    }
    String::from_utf16_lossy(&u16s)
        .trim_end_matches('\u{0}')
        .to_string()
}

#[tauri::command]
pub fn get_app_settings_from_registry() -> Result<Value, String> {
    #[cfg(windows)]
    {
        get_app_settings_from_registry_windows()
    }
    #[cfg(not(windows))]
    {
        get_app_settings_from_registry_linux()
    }
}

#[cfg(windows)]
fn get_app_settings_from_registry_windows() -> Result<Value, String> {
    let mut out = defaults_map();

    let hive = RegKey::predef(HKEY_LOCAL_MACHINE);
    let key = hive
        .open_subkey_with_flags(REG_PATH, KEY_READ | KEY_WOW64_32KEY)
        .or_else(|_| hive.open_subkey_with_flags(REG_PATH, KEY_READ | KEY_WOW64_64KEY));

    let key = match key {
        Ok(k) => k,
        Err(_) => return Ok(Value::Object(out)),
    };

    for &name in KEYS {
        match key.get_raw_value(name) {
            Ok(raw) => {
                let v = match raw.vtype {
                    RegType::REG_SZ | RegType::REG_EXPAND_SZ => {
                        let s = utf16le_to_string(&raw.bytes);
                        if name == "disableAutoUpdate" {
                            json!(matches!(
                                s.to_ascii_lowercase().as_str(),
                                "1" | "true" | "yes"
                            ))
                        } else if name == "backendServerPort"
                            || name == "uiSocketServerPort"
                            || name == "autoUpdateIntervalMins"
                        {
                            if let Ok(n) = s.parse::<u32>() {
                                json!(n)
                            } else {
                                out[name].clone()
                            }
                        } else {
                            json!(s)
                        }
                    }
                    RegType::REG_DWORD => {
                        let mut b = [0u8; 4];
                        let len = raw.bytes.len().min(4);
                        b[..len].copy_from_slice(&raw.bytes[..len]);
                        let n = u32::from_le_bytes(b);
                        if name == "disableAutoUpdate" {
                            json!(n != 0)
                        } else {
                            json!(n)
                        }
                    }
                    _ => json!(general_purpose::STANDARD.encode(&raw.bytes)),
                };
                out.insert(name.to_string(), v);
            }
            Err(_) => {}
        }
    }

    Ok(Value::Object(out))
}

#[cfg(not(windows))]
fn get_app_settings_from_registry_linux() -> Result<Value, String> {
    Ok(Value::Object(defaults_map()))
}

#[tauri::command]
pub fn update_app_setting_in_registry(name: String, value: String) -> Result<Value, String> {
    #[cfg(windows)]
    {
        update_app_setting_in_registry_windows(name, value)
    }
    #[cfg(not(windows))]
    {
        update_app_setting_in_registry_linux(name, value)
    }
}

#[cfg(windows)]
fn update_app_setting_in_registry_windows(name: String, value: String) -> Result<Value, String> {
    let hklm = RegKey::predef(HKEY_LOCAL_MACHINE);
    let key = hklm
        .open_subkey_with_flags(REG_PATH, KEY_SET_VALUE | KEY_WOW64_64KEY)
        .or_else(|_| hklm.open_subkey_with_flags(REG_PATH, KEY_SET_VALUE | KEY_WOW64_32KEY))
        .map_err(|e| format!("open key failed: {e}"))?;

    key.set_value(&name, &value).map_err(|e| e.to_string())?;
    Ok(json!({ "ok": true }))
}

#[cfg(not(windows))]
fn update_app_setting_in_registry_linux(_name: String, _value: String) -> Result<Value, String> {
    Ok(json!({ "ok": true, "message": "Settings not persisted on Linux (using defaults)" }))
}
