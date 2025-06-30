package com.youtube.media.controller.helpers

import android.content.Context
import android.content.SharedPreferences

enum class StorePref(val key: String) {
    BACKEND_SERVER_URL("backendServerUrl")
}
class StoreService(context: Context) {
    private val appPrefs = "AppPrefs"
    private val sharedPreferences: SharedPreferences = context.getSharedPreferences(appPrefs, Context.MODE_PRIVATE)
    fun setKey(key: StorePref, value: String) {
        sharedPreferences.edit().putString(key.toString(), value).apply()
    }
    fun getKey(key: StorePref): String? {
        return sharedPreferences.getString(key.toString(), null)
    }
}