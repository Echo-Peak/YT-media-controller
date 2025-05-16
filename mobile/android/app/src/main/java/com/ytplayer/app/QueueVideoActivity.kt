package com.youtube.media.controller

import android.content.Intent
import android.os.Bundle
import android.util.Log
import androidx.appcompat.app.AppCompatActivity
import com.youtube.media.controller.helpers.StorePref
import com.youtube.media.controller.helpers.StoreService
import com.youtube.media.controller.helpers.Validate

class QueueVideoActivity : AppCompatActivity() {
    private lateinit var store: StoreService
    private val logTag = "QueueVideoActivity"

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        store = StoreService(this)
        handleIntent(intent)
        finish()
    }

    override fun onNewIntent(intent: Intent?) {
        super.onNewIntent(intent)
        handleIntent(intent)
        finish()
    }

    private fun handleIntent(intent: Intent?) {
        val ytUrl = intent?.getStringExtra(Intent.EXTRA_TEXT)
        Log.d(logTag, "Received share intent with URL: $ytUrl")

        if (ytUrl != null) {
            val isValidYtLink = Validate().isYouTubeUrl(ytUrl);
            if(!isValidYtLink){
                return
            }
            val backendUrl = store.getKey(StorePref.BACKEND_SERVER_URL)
            if (backendUrl != null) {
                val httpHelper = HttpHelper(backendUrl)
                httpHelper.sendQueueEvent(ytUrl)
                Log.d(logTag, "Sent queue event to backend: $backendUrl")
            } else {
                Log.e(logTag, "Backend URL not set in storage.")
            }
        } else {
            Log.e(logTag, "Intent EXTRA_TEXT was null.")
        }
    }
}