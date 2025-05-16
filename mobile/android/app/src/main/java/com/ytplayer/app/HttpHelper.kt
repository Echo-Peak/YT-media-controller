package com.youtube.media.controller

import okhttp3.*
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import org.json.JSONObject
import java.io.IOException

class HttpHelper (private val endpointUrl: String) {
    private val client = OkHttpClient()

    private fun postJson(path: String, jsonBody: JSONObject, callback: (success: Boolean, response: String?) -> Unit) {
        val requestBody = RequestBody.create(
            "application/json; charset=utf-8".toMediaTypeOrNull(),
            jsonBody.toString()
        )

        val request = Request.Builder()
            .url("$endpointUrl$path")
            .post(requestBody)
            .build()

        client.newCall(request).enqueue(object : Callback {
            override fun onFailure(call: Call, e: IOException) {
                callback(false, e.message)
            }

            override fun onResponse(call: Call, response: Response) {
                response.use {
                    callback(it.isSuccessful, it.body?.string())
                }
            }
        })
    }

    fun sendPlayEvent(sourceUrl: String, callback: (Boolean, String?) -> Unit = { _, _ -> }) {
        val json = JSONObject().apply {
            put("SourceUrl", sourceUrl)
        }
        postJson("/playVideo", json, callback)
    }

    fun sendQueueEvent(sourceUrl: String, callback: (Boolean, String?) -> Unit = { _, _ -> }) {
        val json = JSONObject().apply {
            put("SourceUrl", sourceUrl)
        }
        postJson("/queueVideo",json, callback)
    }
}