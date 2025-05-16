package com.youtube.media.controller

import android.content.Intent
import android.os.Bundle
import android.util.Log
import androidx.appcompat.app.AppCompatActivity
import com.youtube.media.controller.helpers.StorePref
import com.youtube.media.controller.helpers.StoreService
import com.youtube.media.controller.helpers.Validate


class PlayVideoActivity : AppCompatActivity() {
    private lateinit var store: StoreService
    private var logTag = "PlayVideoActivity"

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        store = StoreService(this)

        handleIntent(intent)
        finish()
    }

    override fun onNewIntent(intent: Intent?) {
        super.onNewIntent(intent)
        handleIntent(intent)
    }

    private fun handleIntent(intent: Intent?) {
        val ytUrl = intent?.getStringExtra(Intent.EXTRA_TEXT)
        if (ytUrl != null) {
            val isValidYtLink = Validate().isYouTubeUrl(ytUrl);
            if(!isValidYtLink){
                return
            }
            val backendUrl = store.getKey(StorePref.BACKEND_SERVER_URL)
            if (backendUrl != null) {
                val httpHelper = HttpHelper(backendUrl)
                httpHelper.sendPlayEvent(ytUrl)
                Log.d(logTag, "Sent play event for: $ytUrl")
            } else {
                Log.e(logTag, "Host URL not configured")
            }
        }
    }
}